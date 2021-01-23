#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "EventosGenerar.h"
#include "EscribirUSBX52.h"
#include "ProcesarHID.h"
#define _PRIVATE_
#include "EventosProcesar.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, LimpiarEventos)
#pragma alloc_text (PAGE, ProcesarEventos)
#pragma alloc_text (PAGE, PrepararDirectX)
#pragma alloc_text (PAGE, ProcesarDirectX)
#pragma alloc_text (PAGE, PrepararTeclado)
#pragma alloc_text (PAGE, ProcesarTeclado)
#pragma alloc_text (PAGE, PrepararRaton)
#pragma alloc_text (PAGE, ProcesarRaton)
#pragma alloc_text (PAGE, ProcesarComandos)
#pragma alloc_text (PAGE, ProcesarEventoX52_Modos)
#pragma alloc_text (PAGE, ProcesarEventoRepeticiones_Delay)
#pragma alloc_text (PAGE, BorrarBloqueRepeat)
#pragma alloc_text (PAGE, CopiarColaConRepeticion1)
#pragma alloc_text (PAGE, CopiarColaConRepeticion2)
#pragma alloc_text (PAGE, EstaHold)
#pragma alloc_text (PAGE, TimerDelay)
#pragma alloc_text (PAGE, TimerDelayWI)

#endif

VOID LimpiarEventos(WDFDEVICE device)
{
	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->HID.WaitLockEventos, NULL);
	{
		while (WdfCollectionGetCount(GetDeviceContext(device)->HID.ColaEventos) > 0)
		{
			WdfObjectDelete(WdfCollectionGetItem(GetDeviceContext(device)->HID.ColaEventos, 0));
			WdfCollectionRemoveItem(GetDeviceContext(device)->HID.ColaEventos, 0);
		}

		while (WdfCollectionGetCount(GetDeviceContext(device)->HID.ListaTimersDelay) > 0)
		{
			WDFTIMER timer = (WDFTIMER)WdfCollectionGetItem(GetDeviceContext(device)->HID.ListaTimersDelay, 0);
			WdfTimerStop(timer, FALSE);
			WdfCollectionRemoveItem(GetDeviceContext(device)->HID.ListaTimersDelay, 0);
			WdfObjectDelete(timer);
		}

		WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
		{
			RtlZeroMemory(GetDeviceContext(device)->HID.Estado.Teclado, sizeof(GetDeviceContext(device)->HID.Estado.Teclado));
			RtlZeroMemory(GetDeviceContext(device)->HID.Estado.Raton, sizeof(GetDeviceContext(device)->HID.Estado.Raton));
			RtlZeroMemory(&GetDeviceContext(device)->HID.Estado.DirectX, sizeof(HID_INPUT_DATA));
		}
		WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);
	}
	WdfWaitLockRelease(GetDeviceContext(device)->HID.WaitLockEventos);
}

//Dentro del WaitLockEventos
UCHAR ProcesarEventos(WDFDEVICE device)
{
	PAGED_CODE();

	UCHAR vueltas;
	UCHAR procesado = 0;

	if (GetDeviceContext(device)->HID.TurnoReport != 1)
	{
		ProcesarComandos(device);
	}
	for (vueltas = 0; vueltas < 4; vueltas++) //por si se vacia la cola
	{
		switch (GetDeviceContext(device)->HID.TurnoReport)
		{
		case 0:
			GetDeviceContext(device)->HID.TurnoReport = 1;
			if (PrepararDirectX(device, FALSE))
			{
				procesado = 1;
			}
			break;
		case 1:
			GetDeviceContext(device)->HID.TurnoReport = 2;
			PrepararDirectX(device, TRUE);
			procesado = 2;
			break;
		case 2:
			GetDeviceContext(device)->HID.TurnoReport = 3;
			if (PrepararRaton(device))
			{
				procesado = 3;
			}
			break;
		case 3:
			GetDeviceContext(device)->HID.TurnoReport = 0;
			if (PrepararTeclado(device))
			{
				procesado = 4;
			}
			break;
		}
		if (procesado)
		{
			break;
		}
	}
	if (GetDeviceContext(device)->HID.TurnoReport != 1)
	{
		ProcesarComandos(device);
	}

	return procesado;
}

#pragma region "DirectX"
BOOLEAN PrepararDirectX(WDFDEVICE device, BOOLEAN report2)
{
	PAGED_CODE();

	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			vacio = TRUE;
	ULONG			posEvento = 0;
	BOOLEAN			dxPosCambiada = FALSE;

	while (posEvento < WdfCollectionGetCount(devExt->ColaEventos))
	{
		WDFCOLLECTION colaComandos = WdfCollectionGetItem(devExt->ColaEventos, posEvento);
		ULONG posComando = 0;
		BOOLEAN setaCambiada = FALSE;
		BOOLEAN botonCambiado = FALSE;

		while (posComando < WdfCollectionGetCount(colaComandos))
		{
			PEV_COMANDO comando = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, posComando), NULL);
			if (comando == NULL)
			{
				break;
			}

			if (comando->Tipo == TipoComando_Reservado_DxPosicion)
			{
				//Se genera en el driver y sólo tiene un comando
				if (!dxPosCambiada)
				{
					if (!report2)
					{
						PHID_INPUT_DATA dato = (PHID_INPUT_DATA)((PUCHAR)WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, posComando), NULL) + 1);
						WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
						{
							if (GetDeviceContext(device)->USBaHID.ModoRaw)
							{
								RtlCopyMemory(&devExt->Estado.DirectX, dato, sizeof(HID_INPUT_DATA));
							}
							else
							{
								RtlCopyMemory(devExt->Estado.DirectX.Ejes, dato->Ejes, sizeof(devExt->Estado.DirectX.Ejes));
								devExt->Estado.DirectX.MiniStick = dato->MiniStick;
							}
						}
						WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
					}
					else
					{
						WdfObjectDelete(WdfCollectionGetItem(colaComandos, posComando));
						WdfCollectionRemoveItem(colaComandos, posComando);
					}
					dxPosCambiada = TRUE;
					vacio = FALSE;
					break;
				}
			}
			else if ((comando->Tipo & 0x7f) == TipoComando_DxBoton)
			{
				if (!botonCambiado)
				{
					botonCambiado = TRUE;
					ProcesarDirectX(device, comando);
					WdfObjectDelete(WdfCollectionGetItem(colaComandos, posComando));
					WdfCollectionRemoveItem(colaComandos, posComando);
					continue;
				}
			}
			else if ((comando->Tipo & 0x7f) == TipoComando_DxSeta)
			{
				if (!setaCambiada)
				{
					setaCambiada = TRUE;
					ProcesarDirectX(device, comando);
					WdfObjectDelete(WdfCollectionGetItem(colaComandos, posComando));
					WdfCollectionRemoveItem(colaComandos, posComando);
					continue;
				}
			}
			else
			{
				break;
			}

			if (botonCambiado && setaCambiada)
				break;

			posComando++;
		}

		if (WdfCollectionGetCount(colaComandos) == 0)
		{ // Fin eventos
			WdfObjectDelete(colaComandos);
			WdfCollectionRemoveItem(devExt->ColaEventos, posEvento);
		}
		else
		{
			posEvento++;
		}
	}

	return !vacio;
}

VOID ProcesarDirectX(WDFDEVICE device, PEV_COMANDO comando)
{
	PAGED_CODE();

	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;
	BOOLEAN		 soltar = ((comando->Tipo & TipoComando_Soltar) == TipoComando_Soltar);

	WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
	{
		if ((comando->Tipo & 0x7f) == TipoComando_DxBoton) // Botón DX
		{
			if (!soltar)
				devExt->Estado.DirectX.Botones[comando->Dato / 8] |= 1 << (comando->Dato % 8);
			else
				devExt->Estado.DirectX.Botones[comando->Dato / 8] &= ~(1 << (comando->Dato % 8));
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_DxSeta) // Seta DX
		{
			if (!soltar)
				devExt->Estado.DirectX.Setas[comando->Dato / 8] = (comando->Dato % 8) + 1;
			else
				devExt->Estado.DirectX.Setas[comando->Dato / 8] = 0;
		}
	}
	WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
}
#pragma endregion

#pragma region "Ratón"
BOOLEAN PrepararRaton(WDFDEVICE device)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			vacio = TRUE;
	ULONG			posEvento = 0;

	while (posEvento < WdfCollectionGetCount(devExt->ColaEventos))
	{
		WDFCOLLECTION colaComandos = WdfCollectionGetItem(devExt->ColaEventos, posEvento);

		if (WdfCollectionGetCount(colaComandos) > 0)
		{
			PEV_COMANDO comando = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, 0), NULL);

			if (ProcesarRaton(device, colaComandos, comando))
			{
				vacio = FALSE;
			}
		}

		if (WdfCollectionGetCount(colaComandos) == 0)
		{ // Fin eventos
			WdfObjectDelete(colaComandos);
			WdfCollectionRemoveItem(devExt->ColaEventos, posEvento);
		}
		else
		{
			posEvento++;
		}
	}

	return !vacio;
}

BOOLEAN ProcesarRaton(WDFDEVICE device, WDFCOLLECTION colaComandos, PEV_COMANDO comando)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			soltar = ((comando->Tipo & TipoComando_Soltar) == TipoComando_Soltar);
	BOOLEAN			procesado = TRUE;

	WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
	{
		if ((comando->Tipo & 0x7f) == TipoComando_RatonBt1)
		{
			if (!soltar)
				devExt->Estado.Raton[0] |= 1;
			else
				devExt->Estado.Raton[0] &= 254;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonBt2)
		{
			if (!soltar)
				devExt->Estado.Raton[0] |= 2;
			else
				devExt->Estado.Raton[0] &= 253;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonBt3)
		{
			if (!soltar)
				devExt->Estado.Raton[0] |= 4;
			else
				devExt->Estado.Raton[0] &= 251;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonIzq) //Eje -x
		{
			if (!soltar)
				devExt->Estado.Raton[1] = -comando->Dato;
			else
				devExt->Estado.Raton[1] = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonDer) //Eje x
		{
			if (!soltar)
				devExt->Estado.Raton[1] = comando->Dato;
			else
				devExt->Estado.Raton[1] = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonArr) //Eje -y
		{
			if (!soltar)
				devExt->Estado.Raton[2] = -comando->Dato;
			else
				devExt->Estado.Raton[2] = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonAba) //Eje y
		{
			if (!soltar)
				devExt->Estado.Raton[2] = comando->Dato;
			else
				devExt->Estado.Raton[2] = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonWhArr) // Wheel up
		{
			if (!soltar)
				devExt->Estado.Raton[3] = 127;
			else
				devExt->Estado.Raton[3] = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando_RatonWhAba) // Wheel down
		{
			if (!soltar)
				devExt->Estado.Raton[3] = (UCHAR)-127;
			else
				devExt->Estado.Raton[3] = 0;
		}
		else
		{
			procesado = FALSE;
		}
	}
	WdfWaitLockRelease(devExt->Estado.WaitLockEstado);

	if (procesado)
	{
		WdfObjectDelete(WdfCollectionGetItem(colaComandos, 0));
		WdfCollectionRemoveItem(colaComandos, 0);
	}
	return procesado;
}
#pragma endregion

#pragma region "Teclado"
BOOLEAN PrepararTeclado(WDFDEVICE device)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			vacio = TRUE;
	ULONG			posEvento = 0;

	while (posEvento < WdfCollectionGetCount(devExt->ColaEventos))
	{
		WDFCOLLECTION colaComandos = WdfCollectionGetItem(devExt->ColaEventos, posEvento);

		if (WdfCollectionGetCount(colaComandos) > 0)
		{
			PEV_COMANDO comando = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, 0), NULL);
			if (comando == NULL)
			{
				break;
			}

			if ((comando->Tipo & 0x7f) == TipoComando_Tecla)
			{
				ProcesarTeclado(device, comando);
				WdfObjectDelete(WdfCollectionGetItem(colaComandos, 0));
				WdfCollectionRemoveItem(colaComandos, 0);
				vacio = FALSE;
			}
		}

		if (WdfCollectionGetCount(colaComandos) == 0)
		{ // Fin eventos
			WdfObjectDelete(colaComandos);
			WdfCollectionRemoveItem(devExt->ColaEventos, posEvento);
		}
		else
		{
			posEvento++;
		}
	}

	return !vacio;
}

VOID ProcesarTeclado(WDFDEVICE device, PEV_COMANDO comando)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	BOOLEAN			soltar = ((comando->Tipo & TipoComando_Soltar) == TipoComando_Soltar);
	WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
	{
		if (!soltar)
			devExt->Estado.Teclado[comando->Dato / 8] |= 1 << (comando->Dato % 8);
		else
			devExt->Estado.Teclado[comando->Dato / 8] &= ~(1 << (comando->Dato % 8));
	}
	WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
}
#pragma endregion

VOID ProcesarComandos(WDFDEVICE device)
{
	PAGED_CODE();
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;
	ULONG		 posEvento = 0;

	while (posEvento < WdfCollectionGetCount(devExt->ColaEventos))
	{
		WDFCOLLECTION colaComandos = WdfCollectionGetItem(devExt->ColaEventos, posEvento);

		while (0 < WdfCollectionGetCount(colaComandos))
		{
			PEV_COMANDO comando = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, 0), NULL);
			if (comando == NULL)
			{
				break;
			}
			
			if (ProcesarEventoX52_Modos(device, colaComandos, comando))
			{
				continue;
			}
			else if ((comando->Tipo == TipoComando_Delay) || (comando->Tipo == TipoComando_Hold) || ((comando->Tipo & 0x7f) == TipoComando_Repeat) || ((comando->Tipo & 0x7f) == TipoComando_RepeatN))
			{
				UCHAR ret = ProcesarEventoRepeticiones_Delay(device, colaComandos, posEvento, comando);
				if (ret == 0)
					break;
				else if (ret == 2)
				{
					continue;
				}
			}
			else
				break;
		}

		if (WdfCollectionGetCount(colaComandos) == 0)
		{ // Fin eventos
			WdfObjectDelete(colaComandos);
			WdfCollectionRemoveItem(devExt->ColaEventos, posEvento);
		}
		else
		{
			posEvento++;
		}
	}
}

/// <summary>
/// Comandos del X52
/// </summary>
/// <returns><para>TRUE: procesado y continuar</para><para>FALSE: no procesado</para></returns>
BOOLEAN ProcesarEventoX52_Modos(WDFDEVICE device, WDFCOLLECTION colaComandos, PEV_COMANDO comando)
{
	PAGED_CODE();

	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;
	BOOLEAN procesado = TRUE;

	switch (comando->Tipo)
	{
		case TipoComando_Modo: //Cambio modo
			WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
			{
				devExt->Estado.Modos = comando->Dato;
			}
			WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
			break;
		case TipoComando_Pinkie: //Cambio modo Pinkie
			WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
			{
				devExt->Estado.Pinkie = comando->Dato;
			}
			WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
			break;
		case TipoComando_MfdLuz: //mfd_luz
		{
			UCHAR params = comando->Dato;
			Luz_MFD(device, &params);
			break;
		}
		case TipoComando_Luz: // luz
		{
			UCHAR params = comando->Dato;
			Luz_Global(device, &params);
			break;
		}
		case TipoComando_InfoLuz: // info luz
		{
			UCHAR params = comando->Dato;
			Luz_Info(device, &params);
			break;
		}
		case TipoComando_MfdPinkie: // pinkie;
		{
			UCHAR params = comando->Dato;
			Set_Pinkie(device, &params);
			break;
		}
		case TipoComando_MfdTextoIni: // texto
		{
			UCHAR texto[17];
			ULONG posTxt = 1;
			UCHAR tam = 1;
			RtlZeroMemory(texto, 17);
			texto[0] = comando->Dato; //linea
			while (posTxt < WdfCollectionGetCount(colaComandos))
			{
				WDFMEMORY mem = WdfCollectionGetItem(colaComandos, 1);
				EV_COMANDO comTxt;
				RtlCopyMemory(&comTxt, WdfMemoryGetBuffer(mem, NULL), sizeof(EV_COMANDO));
				WdfObjectDelete(mem);
				WdfCollectionRemoveItem(colaComandos, 1);
				if (comTxt.Tipo == TipoComando_MfdTextoFin) // fin texto
				{
					break;
				}
				if (tam < 18)
				{
					texto[tam] = comTxt.Dato;
					tam++;
				}
			}
			Set_Texto(device, texto, tam);
			break;
		}
		case TipoComando_MfdHora: // hora
		{
			UCHAR params[3];
			WDFMEMORY resto;
			params[0] = comando->Dato;

			resto = WdfCollectionGetItem(colaComandos, 1);
			params[1] = ((PEV_COMANDO)WdfMemoryGetBuffer(resto, NULL))->Dato;
			WdfObjectDelete(resto);
			WdfCollectionRemoveItem(colaComandos, 1);

			resto = WdfCollectionGetItem(colaComandos, 1);
			params[2] = ((PEV_COMANDO)WdfMemoryGetBuffer(resto, NULL))->Dato;
			WdfObjectDelete(resto);
			WdfCollectionRemoveItem(colaComandos, 1);

			Set_Hora(device, params);
			break;
		}
		case TipoComando_MfdHora24: // hora 24
		{
			UCHAR params[3];
			WDFMEMORY resto;
			params[0] = comando->Dato;

			resto = WdfCollectionGetItem(colaComandos, 1);
			params[1] = ((PEV_COMANDO)WdfMemoryGetBuffer(resto, NULL))->Dato;
			WdfObjectDelete(resto);
			WdfCollectionRemoveItem(colaComandos, 1);

			resto = WdfCollectionGetItem(colaComandos, 1);
			params[2] = ((PEV_COMANDO)WdfMemoryGetBuffer(resto, NULL))->Dato;
			WdfObjectDelete(resto);
			WdfCollectionRemoveItem(colaComandos, 1);

			Set_Hora24(device, params);
			break;
		}
		case TipoComando_MfdFecha: // fecha
		{
			UCHAR params[2];
			WDFMEMORY resto;
			params[0] = comando->Dato;

			resto = WdfCollectionGetItem(colaComandos, 1);
			params[1] = ((PEV_COMANDO)WdfMemoryGetBuffer(resto, NULL))->Dato;
			WdfObjectDelete(resto);
			WdfCollectionRemoveItem(colaComandos, 1);

			Set_Fecha(device, params);
			break;
		}
		default:
			procesado = FALSE;
			break;
	}
	if (procesado)
	{
		WdfObjectDelete(WdfCollectionGetItem(colaComandos, 0));
		WdfCollectionRemoveItem(colaComandos, 0);
	}

	return procesado;
}

#pragma region "Repeticiones y retardo"
/// <summary>
/// Hold, Delay y Repeticiones
/// </summary>
/// <returns><para>[0] salta a la siguiente accion</para>
/// <para>[1] continua en la misma cola</para>
/// <para>[2] continua en la misma cola pero en la misma posicion porque he borrado el nodo actual</para>
/// </returns>
UCHAR ProcesarEventoRepeticiones_Delay(WDFDEVICE device, WDFCOLLECTION colaComandos, ULONG posEvento, PEV_COMANDO comando)
{
	PAGED_CODE();

	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;

	if (comando->Tipo == TipoComando_Delay) // Delay
	{
		WDF_TIMER_CONFIG		timerConfig;
		WDF_OBJECT_ATTRIBUTES	timerAttributes;
		DELAY_CONTEXT*			ctx;
		WDFTIMER				timerHandle;

		WDF_TIMER_CONFIG_INIT(&timerConfig, TimerDelay);
		timerConfig.AutomaticSerialization = TRUE;
		WDF_OBJECT_ATTRIBUTES_INIT(&timerAttributes);
		timerAttributes.ExecutionLevel = WdfExecutionLevelPassive;
		timerAttributes.ParentObject = device;
		WDF_OBJECT_ATTRIBUTES_SET_CONTEXT_TYPE(&timerAttributes, DELAY_CONTEXT);
		if (NT_SUCCESS(WdfTimerCreate(&timerConfig, &timerAttributes, &timerHandle)))
		{
			if (NT_SUCCESS(WdfCollectionAdd(devExt->ListaTimersDelay, timerHandle)))
			{
				LONG timeout = (-10 * 1000 * 100 * comando->Dato);

				comando->Dato = 0; //no vuelve a crear el timer
				ctx = WdfObjectGet_DELAY_CONTEXT(timerHandle);
				ctx->Cola = colaComandos;

				WdfCollectionRemoveItem(devExt->ColaEventos, posEvento);
				WdfTimerStart(timerHandle, timeout);
			}
			else
			{
				WdfObjectDelete(timerHandle);
			}
		}
		WdfObjectDelete(WdfCollectionGetItem(colaComandos, 0));
		WdfCollectionRemoveItem(colaComandos, 0);
		return 0;
	}
	else if (comando->Tipo == TipoComando_Hold) // Autorepeat hold
	{
		if (!EstaHold(device, comando))
		{
			WdfObjectDelete(WdfCollectionGetItem(colaComandos, 0));
			WdfCollectionRemoveItem(colaComandos, 0);
			return 2;
		}
		else
			return 0;
	}
	else if ((comando->Tipo &0x7f) == TipoComando_Repeat)
	{
		if (!EstaHold(device, comando)) // fin autorepeat infinito
		{
			BorrarBloqueRepeat(colaComandos, 0, TipoComando_Repeat);
		}
		else
		{
			if (!CopiarColaConRepeticion1(colaComandos, 0, TipoComando_Repeat))
			{
				return 0;
			}
		}
		return 2;
	}
	else if ((comando->Tipo & 0x7f) == TipoComando_RepeatN)
	{
		if (comando->Dato == 0)
		{
			BorrarBloqueRepeat(colaComandos, 0, TipoComando_RepeatN);
		}
		else
		{
			comando->Dato--;

			if (!CopiarColaConRepeticion1(colaComandos, 0, TipoComando_RepeatN))
			{
				return 0;
			}
		}

		return 2;
	}

	return 1; //aqui ni llega nunca
}

VOID BorrarBloqueRepeat(WDFCOLLECTION colaComandos, ULONG posComando, UCHAR tipoComando)
{
	PAGED_CODE();

	ULONG pos = posComando + 1;
	UCHAR anidados = (tipoComando == TipoComando_RepeatN) ? 1 : 0;;
	while (pos < WdfCollectionGetCount(colaComandos))
	{
		PEV_COMANDO fin = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, pos), NULL);
		if ((fin->Tipo & 0x7f) == TipoComando_RepeatN)
		{
			if ((fin->Tipo & TipoComando_Soltar) == TipoComando_Soltar)
				anidados--;
			else
				anidados++;
		}
		if (((fin->Tipo & 0x7f) == tipoComando) && ((fin->Tipo & TipoComando_Soltar) == TipoComando_Soltar) && (anidados == 0))
		{
			WdfObjectDelete(WdfCollectionGetItem(colaComandos, pos));
			WdfCollectionRemoveItem(colaComandos, pos);
			break;
		}
		pos++;
	}
	WdfObjectDelete(WdfCollectionGetItem(colaComandos, posComando));
	WdfCollectionRemoveItem(colaComandos, posComando);
}

BOOLEAN CopiarColaConRepeticion1(WDFCOLLECTION colaComandos, ULONG posComando, UCHAR tipoComando)
{
	PAGED_CODE();

	WDFCOLLECTION colaAux;
	if (!NT_SUCCESS(WdfCollectionCreate(WDF_NO_OBJECT_ATTRIBUTES, &colaAux)))
	{
		return FALSE;
	}
	else
	{
		if (!CopiarColaConRepeticion2(colaComandos, colaAux, posComando, tipoComando))
		{
			while (0 < WdfCollectionGetCount(colaAux))
			{
				WdfObjectDelete(WdfCollectionGetFirstItem(colaAux));
				WdfCollectionRemoveItem(colaAux, 0);
			}
			WdfObjectDelete(colaAux);
			return FALSE;
		}
		else
		{
			while (0 < WdfCollectionGetCount(colaAux))
			{
				if (!NT_SUCCESS(WdfCollectionAdd(colaComandos, WdfCollectionGetFirstItem(colaAux))))
				{
					while (0 < WdfCollectionGetCount(colaAux))
					{
						WdfObjectDelete(WdfCollectionGetFirstItem(colaAux));
						WdfCollectionRemoveItem(colaAux, 0);
					}
					WdfObjectDelete(colaAux);

					//colaComandos está a medias. borro todos los nodos pero dejo la cola para que al salir se borre también sin fallar
					while (0 < WdfCollectionGetCount(colaComandos))
					{
						WdfObjectDelete(WdfCollectionGetFirstItem(colaComandos));
						WdfCollectionRemoveItem(colaAux, 0);
					}
					return FALSE;
				}
				WdfCollectionRemoveItem(colaAux, 0);
			}
			WdfObjectDelete(colaAux);
		}
	}

	return TRUE;
}

BOOLEAN CopiarColaConRepeticion2(WDFCOLLECTION colaComandos, WDFCOLLECTION colaAux, ULONG posComando, UCHAR tipoComando)
{
	PAGED_CODE();

	ULONG posCopia = posComando + 1;
	UCHAR anidados = (tipoComando == TipoComando_RepeatN) ? 1 : 0;

	while (posCopia < WdfCollectionGetCount(colaComandos))
	{
		PEV_COMANDO comOrigen = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, posCopia), NULL);
		if ((comOrigen->Tipo & 0x7f) == TipoComando_RepeatN)
		{
			if ((comOrigen->Tipo & TipoComando_Soltar) == TipoComando_Soltar)
				anidados--;
			else
				anidados++;
		}
		if (((comOrigen->Tipo & 0x7f) == tipoComando) && ((comOrigen->Tipo & TipoComando_Soltar) == TipoComando_Soltar) && (anidados == 0))
		{
			break;
		}
		else
		{
			WDFMEMORY mem;
			WDF_OBJECT_ATTRIBUTES attributes;
			WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = colaComandos;
			if (!NT_SUCCESS(WdfMemoryCreate(&attributes, PagedPool, (ULONG)'auxC', sizeof(EV_COMANDO), &mem, NULL)))
			{
				return FALSE;
			}
			if (!NT_SUCCESS(WdfMemoryCopyFromBuffer(mem, 0, comOrigen, sizeof(EV_COMANDO))))
			{
				WdfObjectDelete(mem);
				return FALSE;
			}
			if (!NT_SUCCESS(WdfCollectionAdd(colaAux, mem)))
			{
				WdfObjectDelete(mem);
				return FALSE;
			}
		}
		posCopia++;
	}

	posCopia = posComando;
	while (posCopia < WdfCollectionGetCount(colaComandos))
	{
		if (!NT_SUCCESS(WdfCollectionAdd(colaAux, WdfCollectionGetItem(colaComandos, posCopia++))))
		{
			posCopia--;
			while (posCopia != posComando)
			{
				WdfCollectionRemoveItem(colaAux, WdfCollectionGetCount(colaAux) - 1);
				posCopia--;
			}
			return FALSE;
		}
	}
	while (posComando < WdfCollectionGetCount(colaComandos))
	{
		WdfCollectionRemoveItem(colaComandos, posComando);
	}

	return TRUE;
}

BOOLEAN EstaHold(WDFDEVICE device, PEV_COMANDO comando)
{
	PAGED_CODE();

	BOOLEAN pulsado;

	WdfWaitLockAcquire(GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado, NULL);
	{
		if ((comando->Dato & 128) == 128) //eje
		{
			WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
			{
				PEV_COMANDO_EXTENDIDO exComando = (PEV_COMANDO_EXTENDIDO)comando;
				UCHAR modo = GetDeviceContext(device)->HID.Estado.Modos;
				UCHAR pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
				USHORT posIncremental = GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental[pinkie][modo][(exComando->Origen & 127)];
				UCHAR banda = GetDeviceContext(device)->USBaHID.UltimoEstado.Banda[pinkie][modo][(exComando->Origen & 127)];
				pulsado = !((exComando->Modo == modo) && (exComando->Pinkie == pinkie) && ((exComando->Incremental != posIncremental) || (exComando->Banda != banda)));
			}
			WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);
		}
		else
		{
			if ((comando->Dato & 64) == 64) //seta
			{
				pulsado = (GetDeviceContext(device)->USBaHID.UltimoEstado.DeltaHidData.Setas[(comando->Dato & 63) / 8] != 0);
			}
			else //botón
			{
				pulsado = (GetDeviceContext(device)->USBaHID.UltimoEstado.DeltaHidData.Botones[comando->Dato / 8] >> (comando->Dato % 8)) & 1;
			}
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado);

	return pulsado;
}

VOID TimerDelay(IN  WDFTIMER Timer)
{
	PAGED_CODE();

	NTSTATUS				status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_WORKITEM_CONFIG		workitemConfig;
	WDFWORKITEM				workItem;
	WDFDEVICE				device = WdfTimerGetParentObject(Timer);
	DELAY_CONTEXT*			ctx = WdfObjectGet_DELAY_CONTEXT(Timer);
	HID_CONTEXT*			devExt = &GetDeviceContext(device)->HID;
	BOOLEAN					saltar = FALSE;

	WdfWaitLockAcquire(devExt->WaitLockEventos, NULL);
	{
		if (WdfCollectionGetCount(ctx->Cola) > 0)
		{
			if (!NT_SUCCESS(WdfCollectionAdd(devExt->ColaEventos, ctx->Cola)))
			{
				WdfObjectDelete(ctx->Cola);
				saltar = TRUE;
			}
		}
		else
		{
			WdfObjectDelete(ctx->Cola);
			saltar = TRUE;
		}
		WdfCollectionRemove(devExt->ListaTimersDelay, Timer);
	}
	WdfWaitLockRelease(devExt->WaitLockEventos);

	if (!saltar)
	{
		WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
		WDF_WORKITEM_CONFIG_INIT(&workitemConfig, TimerDelayWI);
		status = WdfWorkItemCreate(&workitemConfig, &attributes, &workItem);
		if (NT_SUCCESS(status))
		{
			WdfWorkItemEnqueue(workItem);
		}
	}
	WdfObjectDelete(Timer);
}

VOID TimerDelayWI(_In_ WDFWORKITEM workItem)
{
	PAGED_CODE();

	WDFDEVICE device = (WDFDEVICE)WdfWorkItemGetParentObject(workItem);
	WdfObjectDelete(workItem);

	ProcesarRequestHIDForzada(device);
}
#pragma endregion
