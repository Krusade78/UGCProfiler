#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "TecladoRaton_read.h"
#include "X52_write.h"
#define _ACCIONES_
#include "acciones.h"
#undef _ACCIONES_

VOID LimpiarAcciones(WDFDEVICE device)
{
	WdfSpinLockAcquire(GetDeviceContext(device)->HID.SpinLockAcciones);
	if (!ColaEstaVacia(&GetDeviceContext(device)->HID.ColaAcciones))
	{
		PNODO siguiente = GetDeviceContext(device)->HID.ColaAcciones.principio;
		while (siguiente != NULL)
		{
			ColaBorrar((PCOLA)siguiente->Datos); siguiente->Datos = NULL;
			siguiente = siguiente->siguiente;
			ColaBorrarNodo(&GetDeviceContext(device)->HID.ColaAcciones, GetDeviceContext(device)->HID.ColaAcciones.principio);
		}

		for (ULONG i = 0; i < WdfCollectionGetCount(GetDeviceContext(device)->HID.ListaTimersDelay); i++)
		{
			WDFTIMER timer = (WDFTIMER)WdfCollectionGetItem(GetDeviceContext(device)->HID.ListaTimersDelay, i);
			DELAY_CONTEXT* ctx = WdfObjectGet_DELAY_CONTEXT(timer);
			WdfCollectionRemoveItem(GetDeviceContext(device)->HID.ListaTimersDelay, i);
			if (ctx->principio != NULL)
			{
				while (!ColaBorrarNodo(ctx, ctx->principio));
			}
			WdfObjectDelete(timer);
		}
	}
	WdfSpinLockRelease(GetDeviceContext(device)->HID.SpinLockAcciones);
}

VOID AccionarRaton(WDFDEVICE device, PUCHAR accion, BOOLEAN enDelay)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	PCOLA			eventos = ColaAllocate();
	if (eventos != NULL)
	{
		PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
		if (evt != NULL)
		{
			RtlCopyMemory(evt, accion, sizeof(UCHAR) * 2);
			if (!ColaPush(eventos, evt))
			{
				ExFreePoolWithTag(evt, (ULONG)'vepV');
				ColaBorrar(eventos); eventos = NULL;
			}
			else
			{
				BOOLEAN ok = FALSE;
				WdfSpinLockAcquire(devExt->SpinLockAcciones);
				{
					if (!ColaPush(&devExt->ColaAcciones, eventos))
						ExFreePoolWithTag(evt, (ULONG)'vepV');
					else
						ok = TRUE;
				}
				WdfSpinLockRelease(devExt->SpinLockAcciones);
				if (ok)
					ProcesarAcciones(device, enDelay);
			}
		}
	}
}

VOID AccionarComando(WDFDEVICE device, UINT16 accionId, UCHAR boton)
{
	HID_CONTEXT*		devExt = &GetDeviceContext(device)->HID;
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	
	if(accionId != 0)
	{
		PCOLA eventos = ColaAllocate();
		if(eventos != NULL)
		{
			BOOLEAN ok = TRUE;
			WdfSpinLockAcquire(idevExt->slComandos);
			{
				if (idevExt->nComandos == 0 || idevExt->Comandos == NULL)
				{
					ColaBorrar(eventos); eventos = NULL;
					ok = FALSE;
				}
				else
				{
					UCHAR idx;
					for (idx = 0; idx < idevExt->Comandos[accionId - 1].tam; idx++)
					{
						PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
						if (evt != NULL)
						{
							RtlCopyMemory(evt, &(idevExt->Comandos[accionId - 1].datos[idx]), sizeof(UCHAR) * 2);
							if (*((PUCHAR)evt) == 11 || *((PUCHAR)evt) == 12)
								((PUCHAR)evt)[1] = boton;

							if (!ColaPush(eventos, evt))
							{
								ExFreePoolWithTag(evt, (ULONG)'vepV');
								ColaBorrar(eventos); eventos = NULL;
								ok = FALSE;
								break;
							}
						}
						else
						{
							ColaBorrar(eventos); eventos = NULL;
							ok = FALSE;
							break;
						}
					}
				}
			}
			WdfSpinLockRelease(idevExt->slComandos);
			if(ok)
			{
				WdfSpinLockAcquire(devExt->SpinLockAcciones);
				{
					if (!ColaPush(&devExt->ColaAcciones, eventos))
					{
						ColaBorrar(eventos); eventos = NULL;
						ok = FALSE;
					}
				}
				WdfSpinLockRelease(devExt->SpinLockAcciones);
				if (ok)
					ProcesarAcciones(device, FALSE);
			}
		}
	}
}

VOID ProcesarAcciones(WDFDEVICE device, BOOLEAN enDelay)
{
	HID_CONTEXT*	hidCtx = &GetDeviceContext(device)->HID;
	BOOLEAN			soloHolds = FALSE;

	WdfSpinLockAcquire(hidCtx->SpinLockAcciones);
	{
		// Comprueba si hay acciones nuevas que no sean hold
		if (!ColaEstaVacia(&hidCtx->ColaAcciones))
		{
			PNODO nsiguiente = hidCtx->ColaAcciones.principio;
			soloHolds = TRUE;
			while (nsiguiente != NULL)
			{
				PCOLA cola = (PCOLA)nsiguiente->Datos;
				if ((((PUCHAR)cola->principio->Datos)[0] & 0x1f) != 11) //tipo
				{
					soloHolds = FALSE;
					break;
				}
				nsiguiente = nsiguiente->siguiente;
			}

			if (!soloHolds)
			{
				ProcesarComandos(device, enDelay);
			}
		}
	}
	WdfSpinLockRelease(hidCtx->SpinLockAcciones);
}

VOID ProcesarComandos(_In_ WDFDEVICE device, _In_ BOOLEAN enDelay)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	PNODO			posAccion = devExt->ColaAcciones.principio;

	while (posAccion != NULL)
	{
		PCOLA	colaComandos = (PCOLA)posAccion->Datos;
		PNODO	posComando = colaComandos->principio;
		BOOLEAN	finProceso = FALSE;

		while (posComando != NULL)
		{
#pragma region "Bucle de comandos"
			PNODO canterior = posComando->anterior;
			struct
			{
				UCHAR tipo;
				UCHAR dato;
			} evento;
			RtlCopyMemory(&evento, (PUCHAR)posComando->Datos, sizeof(UCHAR) * 2);

			if (((evento.tipo & 0x1f) > 0) && ((evento.tipo & 0x1f) < 10))
			{
				if (ProcesarEventoRaton(device, evento.tipo, evento.dato))
				{
					ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
				}
				else
				{
					finProceso = TRUE;
					break;
				}
			}
			else if ((evento.tipo & 0x1f) == 0)
			{
				if (ProcesarEventoTeclado(device, evento.tipo, evento.dato))
				{
					ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
				}
				else
				{
					finProceso = TRUE;
					break;
				}
			}
			else if (((evento.tipo & 0x1f) == 18) || ((evento.tipo & 0x1f) == 19))
			{
				ProcesarDirectX(device, enDelay, evento.tipo, evento.dato);
				if (!enDelay)
				{
					ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
				}
				else
				{
					finProceso = TRUE;
					break;
				}
			}
			else if ((((evento.tipo & 0x1f) > 13) && ((evento.tipo & 0x1f) < 17)) || ((evento.tipo & 0x1f) >= 20))
			{
				ProcesarEventoX52_Modos(device, (PCOLA)posAccion->Datos, posComando, evento.tipo, evento.dato);
				ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
			}
			else if ((((evento.tipo & 0x1f) >= 10) && ((evento.tipo & 0x1f) <= 13)) || ((evento.tipo & 0x1f) == 17))
			{
				if (!ProcesarEventoRepeticiones_Delay(device, (PCOLA)posAccion->Datos, posComando, &evento.tipo, evento.dato))
					break;
				else
				{
					if ((evento.tipo & 0x1f) == 17)
						canterior = posComando;
				}
			}

			if (canterior == NULL)
				posComando = ((PCOLA)posAccion->Datos)->principio;
			else
				posComando = canterior->siguiente;

#pragma endregion
		}

		if (ColaEstaVacia(colaComandos))
		{ // Fin eventos
			ColaBorrar(colaComandos); posAccion->Datos = NULL;
			ColaBorrarNodo(&devExt->ColaAcciones, posAccion);
		}

		if (finProceso)
			break;
		else
			posAccion = posAccion->siguiente;
	}
}

VOID ProcesarDirectX(WDFDEVICE device, BOOLEAN enDelay, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			soltar;

	soltar = ((tipo >> 5) == 1) ? TRUE : FALSE;
	tipo &= 0x1f;

	switch (tipo)
	{
	case 18: // Botón DX
		if (!soltar)
			devExt->stBotones[dato / 8] |= 1 << (dato % 8);
		else
			devExt->stBotones[dato / 8] &= ~(1 << (dato % 8));

		break;
	case 19: // Seta DX
		if (!soltar)
			devExt->stSetas[dato / 8] = (dato % 8) + 1;
		else
			devExt->stSetas[dato / 8] = 0;
		break;
	}

	if (enDelay)
	{
		// Cancela una de las request enviadas hacia abajo para que se vuelva a hacer la petición
		// y se lea el estado nuevo
		WDFREQUEST request = NULL;
		WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
		{
			if (WdfCollectionGetCount(GetDeviceContext(device)->EntradaX52.ListaRequest) > 0)
			{
				request = (WDFREQUEST)WdfCollectionGetFirstItem(GetDeviceContext(device)->EntradaX52.ListaRequest);
				WdfObjectReference(request);
				WdfCollectionRemoveItem(GetDeviceContext(device)->EntradaX52.ListaRequest, 0);
			}
		}
		WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
		if (request != NULL)
		{
			WdfRequestCancelSentRequest(request);
			WdfObjectDereference(request);
		}
	}
}

VOID ProcesarEventoX52_Modos(WDFDEVICE device, PCOLA cola, PNODO nodo, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;

	tipo &= 0x1f;

	switch (tipo)
	{
	case 14: //Cambio modo
		devExt->EstadoModos = dato;
		break;
	case 16: //Cambio modo Pinkie
		devExt->EstadoPinkie = dato;
		break;
	case 20: //mfd_luz
	{
		UCHAR params = dato;
		Luz_MFD(device, &params);
	}
	break;
	case 21: // luz
	{
		UCHAR params = dato;
		Luz_Global(device, &params);
	}
	break;
	case 22: // info luz
	{
		UCHAR params = dato;
		Luz_Info(device, &params);
	}
	break;
	case 23: // pinkie;
	{
		UCHAR params = dato;
		Set_Pinkie(device, &params);
	}
	break;
	case 24: // texto
	{
		UCHAR texto[17];
		{
			PNODO nodos = nodo->siguiente;
			UCHAR idx = 1;
			RtlZeroMemory(texto, 17);
			texto[0] = dato;
			while (*((PUCHAR)nodos->Datos) != 56) // fin texto
			{
				texto[idx] = *((PUCHAR)nodos->Datos + 1);
				idx++;
				ColaBorrarNodo(cola, nodos);
				nodos = nodos->siguiente;
			}
			ColaBorrarNodo(cola, nodos);
			Set_Texto(device, texto, idx);
		}
	}
	break;
	case 25: // hora
	{
		UCHAR params[3];
		params[0] = dato;
		params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		params[2] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		Set_Hora(device, params);
	}
	break;
	case 26: // hora 24
	{
		UCHAR params[3];
		params[0] = dato;
		params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		params[2] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		Set_Hora24(device, params);
	}
	break;
	case 27: // fecha
	{
		UCHAR params[2];
		params[0] = dato;
		params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		Set_Fecha(device, params);
	}
	break;
	}
}

#pragma region "Repeticiones y retardo"

BOOLEAN ProcesarEventoRepeticiones_Delay(WDFDEVICE device, PCOLA cola, PNODO nodo, UCHAR* tipo, UCHAR dato)
/**** return TRUE; continua en la misma cola
**** return FALSE; salta a la siguiente accion */
{
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;

	switch (*tipo & 0x1f)
	{
		case 10: // Delay
		{
			WDF_TIMER_CONFIG		timerConfig;
			WDF_OBJECT_ATTRIBUTES	timerAttributes;
			DELAY_CONTEXT*			ctx;
			WDFTIMER				timerHandle;

			WDF_TIMER_CONFIG_INIT(&timerConfig, TimerDelay);
			timerConfig.AutomaticSerialization = TRUE;

			WDF_OBJECT_ATTRIBUTES_INIT(&timerAttributes);
			timerAttributes.ParentObject = device;
			WDF_OBJECT_ATTRIBUTES_SET_CONTEXT_TYPE(&timerAttributes, DELAY_CONTEXT);

			if (NT_SUCCESS(WdfTimerCreate(&timerConfig, &timerAttributes, &timerHandle)))
			{
				LONG timeout = (-10 * 1000 * 100 * dato);

				ctx = WdfObjectGet_DELAY_CONTEXT(timerHandle);
				ctx->fin = cola->fin;
				if (nodo->anterior != NULL) //Se supone que solo puede haber un 17 antes
				{
					ctx->principio = nodo->anterior;
					ColaBorrarNodo(cola, nodo);
					cola->principio = NULL;
					cola->fin = NULL;
				}
				else
				{
					ctx->principio = nodo->siguiente;
					nodo->siguiente = NULL;
					cola->fin = nodo;
					ColaBorrarNodo(cola, nodo);
				}

				WdfCollectionAdd(devExt->ListaTimersDelay, timerHandle);
				WdfTimerStart(timerHandle, timeout);
			}

			return FALSE;
		}
		break;
	case 11: // Autorepeat hold
		if (!EstaHold(devExt, dato))
			ColaBorrarNodo(cola, nodo);
		else
			return FALSE;

		break;
#pragma region "Autorepeat infinito"
	case 12:
		if (!EstaHold(devExt, dato))
		{
			PNODO nodos = nodo->siguiente;
			while (*((PUCHAR)nodos->Datos) != 44) // fin autorepeat infinito
			{
				ColaBorrarNodo(cola, nodos);
				nodos = nodo->siguiente;
			}
			ColaBorrarNodo(cola, nodos); // 44
			ColaBorrarNodo(cola, nodo); //12
		}
		else
		{
			COLA nodosNuevos;

			if (!ReservarMemoriaRepeticiones(&nodosNuevos, nodo, 44))
			{
				PNODO nodos = nodo->siguiente;
				while (*((PUCHAR)nodos->Datos) != 44) // fin autorepeat infinito
				{
					ColaBorrarNodo(cola, nodos);
					nodos = nodo->siguiente;
				}
				ColaBorrarNodo(cola, nodos); // 44
				ColaBorrarNodo(cola, nodo); //12
			}
			else
			{
				PNODO nodos = nodo->siguiente;
				PNODO pos = nodo->anterior;

				nodosNuevos.fin->siguiente = nodo;
				nodo->anterior = nodosNuevos.fin;
				if (pos == NULL)
					cola->principio = nodosNuevos.principio;
				else
					pos->siguiente = nodosNuevos.principio;

				pos = nodosNuevos.principio->siguiente;

				// primer nodo repetición "17"
				*((PUCHAR)nodosNuevos.principio->Datos) = 17;
				*((PUCHAR)nodosNuevos.principio->Datos + 1) = 0;

				// resto
				while (*((PUCHAR)nodos->Datos) != 44) // fin autorepeat infinito
				{
					RtlCopyMemory(pos->Datos, nodos->Datos, 2);
					pos = pos->siguiente;
					nodos = nodos->siguiente;
				}
			}

			return FALSE;
		}
		break;
#pragma endregion
#pragma region "Autorepeat N"
	case 13:
		if (dato == 0)
			limpiar13 :
		{
			ColaBorrarNodo(cola, nodo);
		}
		else
		{
			COLA nodosNuevos;

			*((PUCHAR)nodo->Datos + 1) -= 1; //Reduce la cuenta (evento.dato)

			if (!ReservarMemoriaRepeticiones(&nodosNuevos, nodo, 45))
				goto limpiar13;
			else
			{
				//--------------------------------------------------------------------
				PNODO nodos = nodo->siguiente;
				PNODO pos = nodo->anterior;

				nodosNuevos.fin->siguiente = nodo;
				nodo->anterior = nodosNuevos.fin;
				if (pos == NULL)
					cola->principio = nodosNuevos.principio;
				else
					pos->siguiente = nodosNuevos.principio;

				pos = nodosNuevos.principio->siguiente;

				// primer nodo repetición "17"
				*((PUCHAR)nodosNuevos.principio->Datos) = 17;
				*((PUCHAR)nodosNuevos.principio->Datos + 1) = 0;

				// resto
				while (*((PUCHAR)nodos->Datos) != 45) // fin autorepeat infinito
				{
					RtlCopyMemory(pos->Datos, nodos->Datos, 2);
					pos = pos->siguiente;
					nodos = nodos->siguiente;
				}
				//--------------------------------------------------------------------
			}

			return FALSE;
		}
		break;
#pragma endregion
	case 17: // estoy procesando repeat
		if (*((PUCHAR)(nodo->siguiente->Datos)) == 12 || *((PUCHAR)(nodo->siguiente->Datos)) == 13)
		{
			PNODO pos = devExt->ColaAcciones.principio;
			while (pos != NULL)
			{
				PNODO nodo1 = ((PCOLA)pos->Datos)->principio;
				PNODO nodo2 = nodo1->siguiente;
				UCHAR tipo1 = ((PUCHAR)nodo1->Datos)[0] & 0x1f;
				UCHAR tipo2 = 12;
				if (nodo2 != NULL)
					tipo2 = ((PUCHAR)nodo2->Datos)[0] & 0x1f;

				if ((tipo1 == 17) && ((tipo2 == 12) || (tipo2 == 13)))
					pos = pos->siguiente;
				else
					return FALSE;
			}
			pos = devExt->ColaAcciones.principio;
			while (pos != NULL)
			{
				ColaBorrarNodo((PCOLA)pos->Datos, ((PCOLA)pos->Datos)->principio); // Borra la cabeza (17)
				pos = pos->siguiente;
			}

			*tipo = 0xff; // hace que canterior funcione normal en vez hacer el saltar el 17
		}
		break;
	}

	return TRUE;
}

BOOLEAN ReservarMemoriaRepeticiones(PCOLA cola, PNODO nodo, UCHAR eof)
{
	UINT16* datos = NULL;

	PNODO nodos = nodo->siguiente;
	UCHAR idx = 0;

	cola->principio = NULL;
	cola->fin = NULL;

	datos = (UINT16*)ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'npHV');
	if (datos == NULL)
		return FALSE;
	else
	{
		if (!ColaPush(cola, datos))
		{
			ExFreePoolWithTag((PVOID)datos, (ULONG)'npHV'); datos = NULL;
			return FALSE;
		}
		else
		{
			while (*((PUCHAR)nodos->Datos) != eof) // fin autorepeat infinito
			{
				datos = (UINT16*)ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'npHV');
				if (datos == NULL)
				{
					while (!ColaBorrarNodo(cola, cola->principio));
					return FALSE;
				}
				else
				{
					if (!ColaPush(cola, datos))
					{
						ExFreePoolWithTag((PVOID)datos, (ULONG)'npHV'); datos = NULL;
						while (!ColaBorrarNodo(cola, cola->principio));
						return FALSE;;
					}
				}

				nodos = nodos->siguiente;
				idx++;
			}
			if (idx == 0)
			{
				while (!ColaBorrarNodo(cola, cola->principio));
				return FALSE;
			}
		}
	}

	return TRUE;
}

BOOLEAN EstaHold(HID_CONTEXT* devExt, UCHAR boton)
{
	if (boton < 100)
		return (((devExt->DeltaHidData.Botones[boton / 8] >> (boton % 8)) & 1) == 1);
	else
	{ //Seta
		boton -= 100;
		return (devExt->DeltaHidData.Setas[boton / 8] == (boton % 8));
	}
}

VOID TimerDelay(IN  WDFTIMER Timer)
{
	DELAY_CONTEXT*		ctx = WdfObjectGet_DELAY_CONTEXT(Timer);
	HID_CONTEXT*		devExt = &GetDeviceContext(WdfTimerGetParentObject(Timer))->HID;
	PCOLA				eventos;

	eventos = ColaAllocate();
	if (eventos != NULL)
	{
		BOOLEAN ok = FALSE;
		eventos->principio = ctx->principio;
		eventos->fin = ctx->fin;
		WdfSpinLockAcquire(devExt->SpinLockAcciones);
		{
			if (!ColaPush(&devExt->ColaAcciones, eventos))
			{
				ColaBorrar(eventos); eventos = NULL;
			}
			else
			{
				ok = TRUE;
			}
		}
		WdfSpinLockRelease(devExt->SpinLockAcciones);
		if (ok)
		{
			ProcesarAcciones(WdfTimerGetParentObject(Timer), TRUE);
		}
	}
	WdfObjectDelete(Timer);
}
#pragma endregion
