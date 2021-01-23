/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

teclado_read.c

Abstract:

Procesar Request para el HID.

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#define _PUBLIC_
#include "EventosGenerar.h"
#include "EventosProcesar.h"
#define _PRIVATE_
#include "ProcesarHID.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, EvtRequestHID)
#pragma alloc_text (PAGE, EvtTickRaton)
#pragma alloc_text (PAGE, ProcesarRequestHIDForzada)
#pragma alloc_text (PAGE, ProcesarRequest)
#pragma alloc_text (PAGE, CompletarRequestDirectX1)
#pragma alloc_text (PAGE, CompletarRequestDirectX2)
#pragma alloc_text (PAGE, CompletarRequestTeclado)
#pragma alloc_text (PAGE, CompletarRequestRaton)

#endif

#pragma region "Públicas"
VOID EvtRequestHID(_In_ WDFQUEUE Queue, _In_ WDFREQUEST Request)
{
	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(WdfIoQueueGetDevice(Queue))->HID.WaitLockRequest, NULL);
	ProcesarRequest(WdfIoQueueGetDevice(Queue), Request);
	WdfWaitLockRelease(GetDeviceContext(WdfIoQueueGetDevice(Queue))->HID.WaitLockRequest);
}

VOID ProcesarRequestHIDForzada(WDFDEVICE device)
{
	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->HID.WaitLockRequest, NULL);
	ProcesarRequest(device, NULL);
	WdfWaitLockRelease(GetDeviceContext(device)->HID.WaitLockRequest);
}

VOID EvtTickRaton(_In_ WDFTIMER Timer)
{
	PAGED_CODE();

	WDFDEVICE	device = WdfTimerGetParentObject(Timer);
	EV_COMANDO	comando;
	comando.Tipo = 0;
	comando.Dato = 0;
	//
	//	if (!GetDeviceContext(device)->HID.RatonActivado)
	//		return;
	//
	WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
	{
		if (GetDeviceContext(device)->HID.Estado.Raton[1] != 0)
		{
			if (GetDeviceContext(device)->HID.Estado.Raton[1] < 0)
			{
				comando.Tipo = TipoComando_RatonIzq;
			}
			else
			{
				comando.Tipo = TipoComando_RatonDer;
			}
			comando.Dato = GetDeviceContext(device)->HID.Estado.Raton[1];
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);
	if (comando.Tipo != 0)
	{
		GenerarEventoRaton(device, &comando);
	}
	
	comando.Tipo = 0;
	WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
	{
		if (GetDeviceContext(device)->HID.Estado.Raton[2] != 0)
		{
			if (GetDeviceContext(device)->HID.Estado.Raton[2] < 0)
			{
				comando.Tipo = TipoComando_RatonArr;
			}
			else
			{
				comando.Tipo = TipoComando_RatonAba;
			}
			comando.Dato = GetDeviceContext(device)->HID.Estado.Raton[2];
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);
	if (comando.Tipo != 0)
	{
		GenerarEventoRaton(device, &comando);
	}

	ProcesarRequestHIDForzada(device);
}
#pragma endregion

VOID ProcesarRequest(_In_ WDFDEVICE Device, WDFREQUEST Request)
{
	PAGED_CODE();

	PDEVICE_CONTEXT	devExt = GetDeviceContext(Device);
	BOOLEAN			forzada = FALSE;

	while (TRUE)
	{
		BOOLEAN vacia;
		WdfWaitLockAcquire(devExt->HID.WaitLockEventos, NULL);

		vacia = WdfCollectionGetCount(devExt->HID.ColaEventos) == 0;
		if (!vacia)
		{
			BOOLEAN soloHolds = TRUE;
			ULONG	posEvento = 0;

			while (posEvento < WdfCollectionGetCount(devExt->HID.ColaEventos))
			{
				WDFCOLLECTION colaComandos = WdfCollectionGetItem(devExt->HID.ColaEventos, posEvento);
				if (colaComandos != NULL)
				{
					PEV_COMANDO evento = WdfMemoryGetBuffer(WdfCollectionGetItem(colaComandos, 0), NULL);
					if ((evento != NULL) && (evento->Tipo != TipoComando_Hold))
					{
						soloHolds = FALSE;
						break;
					}
				}

				posEvento++;
			}

			vacia = soloHolds;
		}

		if (vacia)
		{
			WdfWaitLockRelease(devExt->HID.WaitLockEventos);
			if (Request != NULL)
			{
				NTSTATUS status = WdfRequestForwardToIoQueue(Request, devExt->HID.ColaRequestSinUsar);
				if (!NT_SUCCESS(status))
				{
					WdfRequestComplete(Request, status);
				}
			}
			break;
		}
		else
		{
			if (Request == NULL)
			{
				forzada = TRUE;
				WdfIoQueueRetrieveNextRequest(devExt->HID.ColaRequestSinUsar, &Request);
			}
			if (Request == NULL)
			{
				WdfWaitLockRelease(devExt->HID.WaitLockEventos);
				break;
			}
			else
			{
				UCHAR procesado = ProcesarEventos(Device);
				WdfWaitLockRelease(devExt->HID.WaitLockEventos);
				switch (procesado)
				{
					case 1:
						CompletarRequestDirectX1(Device, Request);
						break;
					case 2:
						CompletarRequestDirectX2(Device, Request);
						break;
					case 3:
						CompletarRequestRaton(Device, Request);
						break;
					case 4:
						CompletarRequestTeclado(Device, Request);
						break;
					default:
					{
						NTSTATUS status = (forzada) ? WdfRequestRequeue(Request) : WdfRequestForwardToIoQueue(Request, devExt->HID.ColaRequestSinUsar);
						if (!NT_SUCCESS(status))
						{
							WdfRequestComplete(Request, status);
						}
					}
					break;
				}
				Request = NULL;
			}
		}
	}
}

VOID CompletarRequestDirectX1(WDFDEVICE device, WDFREQUEST request)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(HID_REPORT1) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		HID_REPORT1 report;
		*((PUCHAR)buffer) = 1;
		WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
			report.Ejes[0] = devExt->Estado.DirectX.Ejes[0];
			report.Ejes[1] = devExt->Estado.DirectX.Ejes[1];
			report.Ejes[2] = devExt->Estado.DirectX.Ejes[3];
			report.Ejes[3] = devExt->Estado.DirectX.Ejes[4];
			report.Ejes[4] = devExt->Estado.DirectX.Ejes[5];
			report.Ejes[5] = devExt->Estado.DirectX.Ejes[6];
			RtlCopyMemory(report.Botones, devExt->Estado.DirectX.Botones, sizeof(report.Botones));
			RtlCopyMemory(report.Setas, devExt->Estado.DirectX.Setas, sizeof(report.Setas));
			RtlCopyMemory((PUCHAR)buffer + 1, &report, sizeof(HID_REPORT1));
		WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
		WdfRequestSetInformation(request, sizeof(HID_REPORT1) + 1);
	}
	WdfRequestComplete(request, status);
}

VOID CompletarRequestDirectX2(WDFDEVICE device, WDFREQUEST request)
{
	PAGED_CODE();

	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(HID_REPORT2) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		HID_REPORT2 report;
		*((PUCHAR)buffer) = 2;
		WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
			report.MiniStick = devExt->Estado.DirectX.MiniStick;
			report.Ejes[0] = devExt->Estado.DirectX.Ejes[2];
			report.Ejes[1] = devExt->Estado.DirectX.Ejes[7];
			report.Ejes[2] = devExt->Estado.DirectX.Ejes[8];
			RtlCopyMemory((PUCHAR)buffer + 1, &report, sizeof(HID_REPORT2));
		WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
		WdfRequestSetInformation(request, sizeof(HID_REPORT2) + 1);
	}
	WdfRequestComplete(request, status);
}

VOID CompletarRequestTeclado(WDFDEVICE device, WDFREQUEST request)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->Estado.Teclado) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 4;
		WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
		RtlCopyMemory((PUCHAR)buffer + 1, devExt->Estado.Teclado, sizeof(devExt->Estado.Teclado));
		WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
		WdfRequestSetInformation(request, sizeof(devExt->Estado.Teclado) + 1);
	}
	WdfRequestComplete(request, status);
}

VOID CompletarRequestRaton(WDFDEVICE device, WDFREQUEST request)
{
	PAGED_CODE();

	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;
	BOOLEAN			ratonOn = FALSE;

	WdfTimerStop(devExt->RatonTimer, FALSE);

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->Estado.Raton) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 3;
		WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
		{
			RtlCopyMemory((PUCHAR)buffer + 1, devExt->Estado.Raton, sizeof(devExt->Estado.Raton));
			ratonOn = ((devExt->Estado.Raton[1] != 0) || (devExt->Estado.Raton[2] != 0));
		}
		WdfWaitLockRelease(devExt->Estado.WaitLockEstado);
		WdfRequestSetInformation(request, sizeof(devExt->Estado.Raton) + 1);
	}
	WdfRequestComplete(request, status);

	if (ratonOn)
	{
		WdfWaitLockAcquire(GetDeviceContext(device)->Perfil.WaitLockMapas, NULL);
		{
			//devExt->RatonActivado = TRUE;
			WdfTimerStart(devExt->RatonTimer, WDF_REL_TIMEOUT_IN_MS(GetDeviceContext(device)->Perfil.TickRaton));
		}
		WdfWaitLockRelease(GetDeviceContext(device)->Perfil.WaitLockMapas);
	}
	//else
	//{
	//	devExt->RatonActivado = FALSE;
	//}
}

