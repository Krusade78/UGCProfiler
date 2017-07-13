#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "Teclado_read.h"
#include "Raton_read.h"
#define _ACCIONES_
#include "acciones.h"
#undef _ACCIONES_

VOID AccionarRaton(WDFDEVICE device, PUCHAR accion)
{
	HID_CONTEXT devExt = GetDeviceContext(device)->HID;
	PCOLA eventos = ColaCrear();
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
				WdfSpinLockAcquire(devExt.SpinLockAcciones);
				{
					if (!ColaPush(&devExt.ColaAcciones, eventos))
						ExFreePoolWithTag(evt, (ULONG)'vepV');
					else
						ok = TRUE;
				}
				WdfSpinLockRelease(devExt.SpinLockAcciones);
				if (ok)
					ProcesarAcciones(device, FALSE);
			}
		}
	}
}

VOID AccionarComando(WDFDEVICE device, UINT16 accionId, UCHAR boton)
{
	HID_CONTEXT			devExt = GetDeviceContext(device)->HID;
	PROGRAMADO_CONTEXT	idevExt = GetDeviceContext(device)->Programacion;
	
	if(accionId != 0)
	{
		PCOLA eventos = ColaCrear();
		if(eventos != NULL)
		{
			BOOLEAN ok = TRUE;
			WdfSpinLockAcquire(idevExt.slComandos);
			{
				if (idevExt.nComandos == 0 || idevExt.Comandos == NULL)
				{
					ColaBorrar(eventos); eventos = NULL;
					ok = FALSE;
				}
				else
				{
					UCHAR idx;
					for (idx = 0; idx < idevExt.Comandos[accionId - 1].tam; idx++)
					{
						PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
						if (evt != NULL)
						{
							RtlCopyMemory(evt, &(idevExt.Comandos[accionId - 1].datos[idx]), sizeof(UCHAR) * 2);
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
			WdfSpinLockRelease(idevExt.slComandos);
			if(ok)
			{
				WdfSpinLockAcquire(devExt.SpinLockAcciones);
				{
					if (!ColaPush(&devExt.ColaAcciones, eventos))
					{
						ColaBorrar(eventos); eventos = NULL;
						ok = FALSE;
					}
				}
				WdfSpinLockRelease(devExt.SpinLockAcciones);
				if (ok)
					ProcesarAcciones(device, FALSE);
			}
		}
	}
}

VOID ProcesarAcciones(WDFDEVICE device, BOOLEAN enDelay)
{
	HID_CONTEXT hidCtx = GetDeviceContext(device)->HID;

	while (TRUE)
	{
		BOOLEAN soloHolds = FALSE;
		WdfSpinLockAcquire(hidCtx.SpinLockAcciones);
		{
			// Comprueba si hay acciones nuevas que no sean hold
			if (!ColaEstaVacia(&hidCtx.ColaAcciones))
			{
				PNODO nsiguiente = hidCtx.ColaAcciones.principio;
				soloHolds = TRUE;
				while (nsiguiente != NULL)
				{
					PCOLA cola = (PCOLA)nsiguiente->Datos;
					if ((((PEVENTO)cola->principio->Datos)->tipo & 0x1f) != 11)
					{
						soloHolds = FALSE;
						break;
					}
					nsiguiente = nsiguiente->siguiente;
				}
			}
			if ((ColaEstaVacia(&hidCtx.ColaAcciones) || soloHolds))
			{
				WdfSpinLockRelease(hidCtx.SpinLockAcciones);
				return;
			}
			else
			{
				ProcesarComandos(device, enDelay);
			}

		}
		WdfSpinLockRelease(hidCtx.SpinLockAcciones);
	}
}

VOID ProcesarComandos(_In_ WDFDEVICE device, BOOLEAN enDelay)
{
	HID_CONTEXT			devExt = GetDeviceContext(device)->HID;
	PNODO				posAccion = devExt.ColaAcciones.principio;

	while (posAccion != NULL)
	{
		PNODO psiguiente = posAccion->siguiente;
		PNODO nodoComando = ((PCOLA)posAccion->Datos)->principio;

		while (nodoComando != NULL)
		{
#pragma region "Bucle de comandos"
			PNODO canterior = nodoComando->anterior;
			EVENTO evento;
			RtlCopyMemory(&evento, (PUCHAR)nodoComando->Datos, sizeof(UCHAR) * 2);

			if (((evento.tipo & 0x1f) > 0) && ((evento.tipo & 0x1f) < 10))
			{
				if (ProcesarEventoRaton(device, evento.tipo, evento.dato))
				{
					ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
				}
				psiguiente = NULL; //salir
				break;
			}
			else if ((evento.tipo & 0x1f) == 0)
			{
				if (ProcesarEventoTeclado(device, evento.tipo, evento.dato))
				{
					ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
				}
				psiguiente = NULL; //salir
				break;
			}
			else if (((evento.tipo & 0x1f) == 18) || ((evento.tipo & 0x1f) == 19))
			{
				ProcesarDirectX(device, enDelay, evento.tipo, evento.dato);
				ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
				if (enDelay)
				{
					psiguiente = NULL; //salir
					break;
				}
			}
			else if ((((evento.tipo & 0x1f) > 13) && ((evento.tipo & 0x1f) < 17)) || ((evento.tipo & 0x1f) >= 20))
			{
				ProcesarEventoX52_Modos(device, (PCOLA)posAccion->Datos, nodoComando, &evento);
				ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
			}
			else if ((((evento.tipo & 0x1f) >= 10) && ((evento.tipo & 0x1f) <= 13)) || ((evento.tipo & 0x1f) == 17))
			{
				if (!ProcesarEventoRepeticiones_Delay(device, (PCOLA)posAccion->Datos, nodoComando, &evento))
					break;
				else
				{
					if ((evento.tipo & 0x1f) == 17)
						canterior = nodoComando;
				}
			}

			if (canterior == NULL)
				nodoComando = ((PCOLA)posAccion->Datos)->principio;
			else
				nodoComando = canterior->siguiente;

#pragma endregion
		}

		if (ColaEstaVacia((PCOLA)posAccion->Datos))
		{ // Fin eventos
			ColaBorrar((PCOLA)posAccion->Datos); posAccion->Datos = NULL;
			ColaBorrarNodo(&devExt.ColaAcciones, posAccion);
		}

		posAccion = psiguiente;
	}
}

VOID ProcesarDirectX(WDFDEVICE device, BOOLEAN enDelay, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT	devExt = GetDeviceContext(device)->HID;
	WDFREQUEST	request;
	BOOLEAN		soltar;
	NTSTATUS	status;
	PVOID		buffer;

	soltar = ((tipo >> 5) == 1) ? TRUE : FALSE;
	tipo &= 0x1f;

	switch (tipo)
	{
	case 18: // Botón DX
		if (!soltar)
			devExt.stDirectX.Botones[dato / 8] |= 1 << (dato % 8);
		else
			devExt.stDirectX.Botones[dato / 8] &= ~(1 << (dato % 8));

		break;
	case 19: // Seta DX
		if (!soltar)
			devExt.stDirectX.Setas[dato / 8] = (dato % 8) + 1;
		else
			devExt.stDirectX.Setas[dato / 8] = 0;
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

VOID ProcesarEventoX52_Modos(WDFDEVICE device, PCOLA cola, PNODO nodo, PEVENTO evento)
{
	HID_CONTEXT devExt = GetDeviceContext(device)->HID;

	evento->tipo &= 0x1f;

	switch (evento->tipo)
	{
	case 14: //Cambio modo
		devExt.EstadoModos = evento->dato;
		break;
	case 15: //Cambio modo Aux
		devExt.EstadoAux = evento->dato;
		break;
	case 16: //Cambio modo Pinkie
		devExt.EstadoPinkie = evento->dato;
		break;
	case 20: //mfd_luz
	{
		UCHAR params;
		params = evento->dato;
		EnviarOrdenX52(devExt, 0, &params, 1);
	}
	break;
	case 21: // luz
	{
		UCHAR params;
		params = evento->dato;
		EnviarOrdenX52(devExt, 1, &params, 1);
	}
	break;
	case 22: // info luz
	{
		UCHAR params;
		params = evento->dato;
		EnviarOrdenX52(devExt, 2, &params, 1);
	}
	break;
	case 23: // pinkie;
	{
		UCHAR params;
		params = evento->dato;
		EnviarOrdenX52(devExt, 3, &params, 1);
	}
	break;
	case 24: // texto
	{
		UCHAR texto[17];
		{
			PNODO nodos = nodo->siguiente;
			UCHAR idx = 1;
			RtlZeroMemory(texto, 17);
			texto[0] = evento->dato;
			while (*((PUCHAR)nodos->Datos) != 56) // fin texto
			{
				texto[idx] = *((PUCHAR)nodos->Datos + 1);
				idx++;
				ColaBorrarNodo(cola, nodos);
				nodos = nodos->siguiente;
			}
			ColaBorrarNodo(cola, nodos);
			EnviarOrdenX52(devExt, 4, texto, idx);
		}
	}
	break;
	case 25: // hora
	{
		UCHAR params[3];
		params[0] = evento->dato;
		params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		params[2] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		EnviarOrdenX52(devExt, 5, params, 3);
	}
	break;
	case 26: // hora 24
	{
		UCHAR params[3];
		params[0] = evento->dato;
		params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		params[2] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		EnviarOrdenX52(devExt, 6, params, 3);
	}
	break;
	case 27: // fecha
	{
		UCHAR params[2];
		params[0] = evento->dato;
		params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
		ColaBorrarNodo(cola, nodo->siguiente);
		EnviarOrdenX52(devExt, 7, params, 2);
	}
	break;
	}
}

#pragma region "Repeticiones y retardo"

BOOLEAN ProcesarEventoRepeticiones_Delay(WDFDEVICE device, PCOLA cola, PNODO nodo, PEVENTO evento)
/**** return TRUE; continua en la misma cola
**** return FALSE; salta a la siguiente accion */
{
	HID_CONTEXT	devExt = GetDeviceContext(device)->HID;

	switch (evento->tipo & 0x1f)
	{
		case 10: // Delay
		{
			WDF_TIMER_CONFIG		timerConfig;
			WDF_OBJECT_ATTRIBUTES	timerAttributes;
			PDELAY_CONTEXT			ctx;
			WDFTIMER				timerHandle;

			WDF_TIMER_CONFIG_INIT(&timerConfig, TimerDelay);
			timerConfig.AutomaticSerialization = TRUE;

			WDF_OBJECT_ATTRIBUTES_INIT(&timerAttributes);
			timerAttributes.ParentObject = device;
			WDF_OBJECT_ATTRIBUTES_SET_CONTEXT_TYPE(&timerAttributes, DELAY_CONTEXT);

			if (NT_SUCCESS(WdfTimerCreate(&timerConfig, &timerAttributes, &timerHandle)))
			{
				LONG timeout = (-10 * 1000 * 100 * evento->dato);

				ctx = WdfObjectGet_DELAY_CONTEXT(timerHandle);
				ctx->NodoFin = cola->fin;
				if (nodo->anterior != NULL) //Se supone que solo puede haber un 17 antes
				{
					ctx->NodoIni = nodo->anterior;
					ColaBorrarNodo(cola, nodo);
					cola->principio = NULL;
					cola->fin = NULL;
				}
				else
				{
					ctx->NodoIni = nodo->siguiente;
					nodo->siguiente = NULL;
					cola->fin = nodo;
					ColaBorrarNodo(cola, nodo);
				}

				WdfTimerStart(timerHandle, timeout);
			}

			return FALSE;
		}
		break;
	case 11: // Autorepeat hold
		if (!EstaHold(devExt, evento->dato))
			ColaBorrarNodo(cola, nodo);
		else
			return FALSE;

		break;
#pragma region "Autorepeat infinito"
	case 12:
		if (!EstaHold(devExt, evento->dato))
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
		if (evento->dato == 0)
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
			PNODO pos = devExt.ColaAcciones.principio;
			while (pos != NULL)
			{
				PNODO nodo1 = ((PCOLA)pos->Datos)->principio;
				PNODO nodo2 = nodo1->siguiente;
				UCHAR tipo1 = ((PEVENTO)nodo1->Datos)->tipo & 0x1f;
				UCHAR tipo2 = 12;
				if (nodo2 != NULL)
					tipo2 = ((PEVENTO)nodo2->Datos)->tipo & 0x1f;

				if ((tipo1 == 17) && ((tipo2 == 12) || (tipo2 == 13)))
					pos = pos->siguiente;
				else
					return FALSE;
			}
			pos = devExt.ColaAcciones.principio;
			while (pos != NULL)
			{
				ColaBorrarNodo((PCOLA)pos->Datos, ((PCOLA)pos->Datos)->principio); // Borra la cabeza (17)
				pos = pos->siguiente;
			}

			evento->tipo = 0xff; // hace que canterior funcione normal en vez hacer el saltar el 17
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

BOOLEAN EstaHold(HID_CONTEXT devExt, UCHAR boton)
{
	if (boton < 100)
		return (((devExt.DeltaHidData.Botones[boton / 8] >> (boton % 8)) & 1) == 1);
	else
	{ //Seta
		boton -= 100;
		return (devExt.DeltaHidData.Setas[boton / 8] == (boton % 8));
	}
}

VOID TimerDelay(IN  WDFTIMER Timer)
{
	PDELAY_CONTEXT		ctx = WdfObjectGet_DELAY_CONTEXT(Timer);
	HID_CONTEXT			devExt = GetDeviceContext(WdfTimerGetParentObject(Timer))->HID;
	PCOLA				eventos;

	eventos = ColaCrear();
	if (eventos != NULL)
	{
		BOOLEAN ok = FALSE;
		eventos->principio = ctx->NodoIni;
		eventos->fin = ctx->NodoFin;
		WdfSpinLockAcquire(devExt.SpinLockAcciones);
		{
			if (!ColaPush(&devExt.ColaAcciones, eventos))
			{
				ColaBorrar(eventos); eventos = NULL;
			}
			else
				ok = TRUE;
		}
		WdfSpinLockRelease(devExt.SpinLockAcciones);
		if (ok)
			ProcesarAcciones(WdfTimerGetParentObject(Timer));
	}
	WdfObjectDelete(Timer);
}
#pragma endregion
