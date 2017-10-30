#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "RequestHID_read.h"
#include "X52_write.h"
#define _PRIVATE_
#include "AccionesProcesar.h"
#undef _PRIVATE_

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

BOOLEAN ProcesarAcciones(WDFDEVICE device, WDFREQUEST request)
{
	BOOLEAN procesado = FALSE;
	UCHAR max = 3; //por si se vacia la cola

	ProcesarComandos(device);
	do
	{
		switch (GetDeviceContext(device)->HID.TurnoReport)
		{
		case 0:
			GetDeviceContext(device)->HID.TurnoReport = 1;
			procesado = PrepararDirectX(device, request);
			break;
		case 1:
			GetDeviceContext(device)->HID.TurnoReport = 2;
			procesado =	PrepararRaton(device, request);
			break;
		case 2:
			GetDeviceContext(device)->HID.TurnoReport = 0;
			procesado = PrepararTeclado(device, request);
			break;
		}
		max--;
	} while (!procesado && (max > 0));
	ProcesarComandos(device);

	return (max > 0);
}

#pragma region "DirectX"
BOOLEAN PrepararDirectX(WDFDEVICE device, WDFREQUEST request)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			vacio = TRUE;

	WdfSpinLockAcquire(devExt->SpinLockAcciones);
	{
		PNODO	posAccion = devExt->ColaAcciones.principio;
		BOOLEAN	dxPosCambiada = FALSE;

		while (posAccion != NULL)
		{
			PCOLA	colaComandos = (PCOLA)posAccion->Datos;
			PNODO	posComando = colaComandos->principio;
			BOOLEAN setaCambiada = FALSE;
			BOOLEAN botonCambiado = FALSE;

			while (posComando != NULL)
			{
				PNODO posAnterior = posComando->anterior;
				struct
				{
					UCHAR tipo;
					UCHAR dato;
				} evento;
				RtlCopyMemory(&evento, (PUCHAR)posComando->Datos, sizeof(UCHAR) * 2);

				if (evento.tipo == TipoComando_DxPosicion)
				{
					if (!dxPosCambiada)
					{
						PHID_INPUT_DATA dato = (PHID_INPUT_DATA)((PUCHAR)posComando->Datos + 1);
						RtlCopyMemory(devExt->stDirectX.Ejes, dato->Ejes, sizeof(devExt->stDirectX.Ejes));
						devExt->stDirectX.MiniStick = dato->MiniStick;
						ColaBorrarNodo(colaComandos, posComando);
						dxPosCambiada = TRUE;
						vacio = FALSE;
						break;
					}
				}
				else if ((evento.tipo & 0x1f) == TipoComando_DxBoton)
				{
					if (!botonCambiado)
					{
						botonCambiado = TRUE;
						ProcesarDirectX(device, evento.tipo, evento.dato);
						ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
					}
				}
				if ((evento.tipo & 0x1f) == TipoComando_DxSeta)
				{
					if (!setaCambiada)
					{
						setaCambiada = TRUE;
						ProcesarDirectX(device, evento.tipo, evento.dato);
						ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
					}
				}
				else
				{
					break;
				}

				if (botonCambiado && setaCambiada && botonCambiado)
					break;

				if (posAnterior == NULL)
					posComando = ((PCOLA)posAccion->Datos)->principio;
				else
					posComando = posAnterior->siguiente;
			}

			if (ColaEstaVacia(colaComandos))
			{ // Fin eventos
				ColaBorrar(colaComandos); posAccion->Datos = NULL;
				ColaBorrarNodo(&devExt->ColaAcciones, posAccion);
			}

			posAccion = posAccion->siguiente;
		}
	}
	WdfSpinLockRelease(devExt->SpinLockAcciones);

	if (!vacio)
		CompletarRequestDirectX(device, request);

	return !vacio;
}

VOID ProcesarDirectX(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			soltar;

	soltar = ((tipo & 32) == 32) ? TRUE : FALSE;
	tipo &= 0x1f;

	switch (tipo)
	{
	case TipoComando_DxBoton: // Botón DX
		if (!soltar)
			devExt->stDirectX.Botones[dato / 8] |= 1 << (dato % 8);
		else
			devExt->stDirectX.Botones[dato / 8] &= ~(1 << (dato % 8));

		break;
	case TipoComando_DxSeta: // Seta DX
		if (!soltar)
			devExt->stDirectX.Setas[dato / 8] = (dato % 8) + 1;
		else
			devExt->stDirectX.Setas[dato / 8] = 0;
		break;
	}
}
#pragma endregion

#pragma region "Ratón"
BOOLEAN PrepararRaton(WDFDEVICE device, WDFREQUEST request)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			vacio = TRUE;
	WdfSpinLockAcquire(devExt->SpinLockAcciones);
	{
		PNODO posAccion = devExt->ColaAcciones.principio;

		while (posAccion != NULL)
		{
			PCOLA	colaComandos = (PCOLA)posAccion->Datos;
			PNODO	posComando = colaComandos->principio;

			if (posComando != NULL)
			{
				struct
				{
					UCHAR tipo;
					UCHAR dato;
				} evento;
				RtlCopyMemory(&evento, (PUCHAR)posComando->Datos, sizeof(UCHAR) * 2);

				if (((evento.tipo & 0x1f) > TipoComando_Tecla) && ((evento.tipo & 0x1f) < TipoComando_Delay))
				{
					ProcesarRaton(device, evento.tipo, evento.dato);
					ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
					vacio = FALSE;
				}
			}

			if (ColaEstaVacia(colaComandos))
			{ // Fin eventos
				ColaBorrar(colaComandos); posAccion->Datos = NULL;
				ColaBorrarNodo(&devExt->ColaAcciones, posAccion);
			}

			posAccion = posAccion->siguiente;
		}
	}
	WdfSpinLockRelease(devExt->SpinLockAcciones);

	if (!vacio)
		CompletarRequestRaton(device, request);

	return !vacio;
}

VOID ProcesarRaton(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			soltar;

	WdfTimerStop(devExt->RatonTimer, FALSE);

	soltar = ((tipo & 32) == 32) ? TRUE : FALSE;

	switch (tipo & 0x1f)
	{
	case TipoComando_RatonBt1:
		if (!soltar)
			devExt->stRaton[0] |= 1;
		else
			devExt->stRaton[0] &= 254;
		break;
	case TipoComando_RatonBt2:
		if (!soltar)
			devExt->stRaton[0] |= 2;
		else
			devExt->stRaton[0] &= 253;
		break;
	case TipoComando_RatonBt3:
		if (!soltar)
			devExt->stRaton[0] |= 4;
		else
			devExt->stRaton[0] &= 251;
		break;
	case TipoComando_RatonIzq: //Eje -x
		if (!soltar)
			devExt->stRaton[1] = -dato;
		else
			devExt->stRaton[1] = 0;
		break;
	case TipoComando_RatonDer: //Eje x
		if (!soltar)
			devExt->stRaton[1] = dato;
		else
			devExt->stRaton[1] = 0;
		break;
	case TipoComando_RatonArr: //Eje -y
		if (!soltar)
			devExt->stRaton[2] = -dato;
		else
			devExt->stRaton[2] = 0;
		break;
	case TipoComando_RatonAba: //Eje y
		if (!soltar)
			devExt->stRaton[2] = dato;
		else
			devExt->stRaton[2] = 0;
		break;
	case TipoComando_RatonWhArr: // Wheel up
		if (!soltar)
			devExt->stRaton[3] = 127;
		else
			devExt->stRaton[3] = 0;
		break;
	case TipoComando_RatonWhAba: // Wheel down
		if (!soltar)
			devExt->stRaton[3] = (UCHAR)-127;
		else
			devExt->stRaton[3] = 0;
		break;
	}
}
#pragma endregion

#pragma region "Teclado"
BOOLEAN PrepararTeclado(WDFDEVICE device, WDFREQUEST request)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			vacio = TRUE;
	WdfSpinLockAcquire(devExt->SpinLockAcciones);
	{
		PNODO posAccion = devExt->ColaAcciones.principio;

		while (posAccion != NULL)
		{
			PCOLA	colaComandos = (PCOLA)posAccion->Datos;
			PNODO	posComando = colaComandos->principio;

			if (posComando != NULL)
			{
				struct
				{
					UCHAR tipo;
					UCHAR dato;
				} evento;
				RtlCopyMemory(&evento, (PUCHAR)posComando->Datos, sizeof(UCHAR) * 2);

				if ((evento.tipo & 0x1f) == TipoComando_Tecla)
				{
					ProcesarTeclado(device, evento.tipo, evento.dato);
					ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
					vacio = FALSE;
				}
			}

			if (ColaEstaVacia(colaComandos))
			{ // Fin eventos
				ColaBorrar(colaComandos); posAccion->Datos = NULL;
				ColaBorrarNodo(&devExt->ColaAcciones, posAccion);
			}

			posAccion = posAccion->siguiente;
		}
	}
	WdfSpinLockRelease(devExt->SpinLockAcciones);

	if (!vacio)
		CompletarRequestTeclado(device, request);

	return !vacio;
}

VOID ProcesarTeclado(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			soltar;

	soltar = ((tipo & 32) == 32) ? TRUE : FALSE;
	tipo &= 0x1f;

	if (!soltar)
		devExt->stTeclado[dato / 8] |= 1 << (dato % 8);
	else
		devExt->stTeclado[dato / 8] &= ~(1 << (dato % 8));
}
#pragma endregion

VOID ProcesarComandos(_In_ WDFDEVICE device)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	WdfSpinLockAcquire(devExt->SpinLockAcciones);
	{
		PNODO			posAccion = devExt->ColaAcciones.principio;

		while (posAccion != NULL)
		{
			PCOLA	colaComandos = (PCOLA)posAccion->Datos;
			PNODO	posComando = colaComandos->principio;

			while (posComando != NULL)
			{
				PNODO canterior = posComando->anterior;
				struct
				{
					UCHAR tipo;
					UCHAR dato;
				} evento;
				RtlCopyMemory(&evento, (PUCHAR)posComando->Datos, sizeof(UCHAR) * 2);

				if ((evento.tipo == TipoComando_Modo) || (evento.tipo == TipoComando_Pinkie) || (((evento.tipo & 0x1f) >= TipoComando_MfdLuz) && ((evento.tipo & 0x1f) <= TipoComando_MfdFecha)))
				{
					ProcesarEventoX52_Modos(device, (PCOLA)posAccion->Datos, posComando, evento.tipo, evento.dato);
					ColaBorrarNodo((PCOLA)posAccion->Datos, posComando);
				}
				else if (((evento.tipo >= TipoComando_Delay) && (evento.tipo <= TipoComando_RepeatN)) || (evento.tipo == TipoComando_RepeatIni))
				{
					UCHAR ret = ProcesarEventoRepeticiones_Delay(device, (PCOLA)posAccion->Datos, posComando, evento.tipo, evento.dato);
					if (ret == 0)
						break;
					else if (ret == 2)
					{
						canterior = posComando;
					}
				}
				else
					break;

				if (canterior == NULL)
					posComando = ((PCOLA)posAccion->Datos)->principio;
				else
					posComando = canterior->siguiente;
			}

			if (ColaEstaVacia(colaComandos))
			{ // Fin eventos
				ColaBorrar(colaComandos); posAccion->Datos = NULL;
				ColaBorrarNodo(&devExt->ColaAcciones, posAccion);
			}

			posAccion = posAccion->siguiente;
		}
	}
	WdfSpinLockRelease(devExt->SpinLockAcciones);
}

VOID ProcesarEventoX52_Modos(WDFDEVICE device, PCOLA cola, PNODO nodo, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;

	switch (tipo)
	{
		case TipoComando_Modo: //Cambio modo
			devExt->EstadoModos = dato;
			break;
		case TipoComando_Pinkie: //Cambio modo Pinkie
			devExt->EstadoPinkie = dato;
			break;
		case TipoComando_MfdLuz: //mfd_luz
		{
			UCHAR params = dato;
			Luz_MFD(device, &params);
			break;
		}
		case TipoComando_Luz: // luz
		{
			UCHAR params = dato;
			Luz_Global(device, &params);
			break;
		}
		case TipoComando_InfoLuz: // info luz
		{
			UCHAR params = dato;
			Luz_Info(device, &params);
			break;
		}
		case TipoComando_MfdPinkie: // pinkie;
		{
			UCHAR params = dato;
			Set_Pinkie(device, &params);
			break;
		}
		case TipoComando_MfdTexto: // texto
		{
			UCHAR texto[17];
			PNODO nodos = nodo->siguiente;
			UCHAR idx = 1;
			RtlZeroMemory(texto, 17);
			texto[0] = dato;
			while (*((PUCHAR)nodos->Datos) != TipoComando_MfdTextoFin) // fin texto
			{
				texto[idx] = *((PUCHAR)nodos->Datos + 1);
				idx++;
				ColaBorrarNodo(cola, nodos);
				nodos = nodos->siguiente;
			}
			ColaBorrarNodo(cola, nodos);
			Set_Texto(device, texto, idx);
			break;
		}
		case TipoComando_MfdHora: // hora
		{
			UCHAR params[3];
			params[0] = dato;
			params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
			ColaBorrarNodo(cola, nodo->siguiente);
			params[2] = *((PUCHAR)nodo->siguiente->Datos + 1);
			ColaBorrarNodo(cola, nodo->siguiente);
			Set_Hora(device, params);
			break;
		}
		case TipoComando_MfdHora24: // hora 24
		{
			UCHAR params[3];
			params[0] = dato;
			params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
			ColaBorrarNodo(cola, nodo->siguiente);
			params[2] = *((PUCHAR)nodo->siguiente->Datos + 1);
			ColaBorrarNodo(cola, nodo->siguiente);
			Set_Hora24(device, params);
			break;
		}
		case TipoComando_MfdFecha: // fecha
		{
			UCHAR params[2];
			params[0] = dato;
			params[1] = *((PUCHAR)nodo->siguiente->Datos + 1);
			ColaBorrarNodo(cola, nodo->siguiente);
			Set_Fecha(device, params);
			break;
		}
	}
}

#pragma region "Repeticiones y retardo"

UCHAR ProcesarEventoRepeticiones_Delay(WDFDEVICE device, PCOLA cola, PNODO nodo, UCHAR tipo, UCHAR dato)
/**** return TRUE; continua en la misma cola
**** return FALSE; salta a la siguiente accion */
{
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;

	switch (tipo & 0x1f)
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
	case TipoComando_Repeat:
		if (!EstaHold(devExt, dato))
		{
			PNODO nodos = nodo->siguiente;
			while (*((PUCHAR)nodos->Datos) != TipoComando_RepeatFin) // fin autorepeat infinito
			{
				ColaBorrarNodo(cola, nodos);
				nodos = nodo->siguiente;
			}
			ColaBorrarNodo(cola, nodos); // TipoComando_RepeatFin
			ColaBorrarNodo(cola, nodo); //TipoComando_Repeat
		}
		else
		{
			COLA nodosNuevos;

			if (!ReservarMemoriaRepeticiones(&nodosNuevos, nodo, TipoComando_RepeatFin))
			{
				PNODO nodos = nodo->siguiente;
				while (*((PUCHAR)nodos->Datos) != TipoComando_RepeatFin) // fin autorepeat infinito
				{
					ColaBorrarNodo(cola, nodos);
					nodos = nodo->siguiente;
				}
				ColaBorrarNodo(cola, nodos); // TipoComando_RepeatFin
				ColaBorrarNodo(cola, nodo); //TipoComando_Repeat
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
				*((PUCHAR)nodosNuevos.principio->Datos) = TipoComando_RepeatIni;
				*((PUCHAR)nodosNuevos.principio->Datos + 1) = 0;

				// resto
				while (*((PUCHAR)nodos->Datos) != TipoComando_RepeatFin) // fin autorepeat infinito
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
	case TipoComando_RepeatN:
		if (dato == 0)
		{
			ColaBorrarNodo(cola, nodo);
		}
		else
		{
			COLA nodosNuevos;

			*((PUCHAR)nodo->Datos + 1) -= 1; //Reduce la cuenta (evento.dato)

			if (!ReservarMemoriaRepeticiones(&nodosNuevos, nodo, TipoComando_RepeatNFin))
			{
				ColaBorrarNodo(cola, nodo);
			}
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
				*((PUCHAR)nodosNuevos.principio->Datos) = TipoComando_RepeatIni;
				*((PUCHAR)nodosNuevos.principio->Datos + 1) = 0;

				// resto
				while (*((PUCHAR)nodos->Datos) != TipoComando_RepeatNFin) // fin autorepeat infinito
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
	case TipoComando_RepeatIni: // estoy procesando repeat
		if (*((PUCHAR)(nodo->siguiente->Datos)) == TipoComando_Repeat || *((PUCHAR)(nodo->siguiente->Datos)) == TipoComando_RepeatN)
		{
			PNODO pos = devExt->ColaAcciones.principio;
			while (pos != NULL)
			{
				PNODO nodo1 = ((PCOLA)pos->Datos)->principio;
				PNODO nodo2 = nodo1->siguiente;
				UCHAR tipo1 = ((PUCHAR)nodo1->Datos)[0] & 0x1f;
				UCHAR tipo2 = TipoComando_Repeat;
				if (nodo2 != NULL)
					tipo2 = ((PUCHAR)nodo2->Datos)[0] & 0x1f;

				if ((tipo1 == TipoComando_RepeatIni) && ((tipo2 == TipoComando_Repeat) || (tipo2 == TipoComando_RepeatN)))
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

			return 2; // hace que canterior funcione normal en vez hacer el saltar el 17
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
			ProcesarRequest(WdfTimerGetParentObject(Timer));
		}
	}
	WdfObjectDelete(Timer);
}
#pragma endregion
