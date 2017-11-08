/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

teclado_read.c

Abstract:

Pasar datos del teclado al HID.

--*/
#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#include "AccionesProcesar.h"
#include "AccionesGenerar.h"
#define _PRIVATE_
#include "RequestHID_read.h"
#undef _PRIVATE_

VOID EvtRequestHID(_In_ WDFQUEUE Queue, _In_ WDFREQUEST Request)
{
	ProcesarRequest(WdfIoQueueGetDevice(Queue), Request);
}

VOID ForzarProcesarRequest(WDFDEVICE device)
{
	ProcesarRequest(device, NULL);
}

VOID ProcesarRequest(_In_ WDFDEVICE Device, _In_ WDFREQUEST Request)
{
	PDEVICE_CONTEXT	devExt = GetDeviceContext(Device);
	BOOLEAN			forzada = FALSE;

	while (TRUE)
	{
		BOOLEAN vacia;
		WdfSpinLockAcquire(devExt->HID.SpinLockAcciones);

		vacia = ColaEstaVacia(&devExt->HID.ColaAcciones);
		if (!vacia)
		{
			BOOLEAN soloHolds = TRUE;
			PNODO	nsiguiente = devExt->HID.ColaAcciones.principio;
			while (nsiguiente != NULL)
			{
				PCOLA cola = (PCOLA)nsiguiente->Datos;
				if ((((PUCHAR)cola->principio->Datos)[0]) != TipoComando_Hold) //tipo
				{
					soloHolds = FALSE;
					break;
				}
				nsiguiente = nsiguiente->siguiente;
			}
			vacia = soloHolds;
		}

		if (vacia)
		{
			WdfSpinLockRelease(devExt->HID.SpinLockAcciones);
			if (Request != NULL)
			{
				NTSTATUS status = WdfRequestForwardToIoQueue(Request, devExt->EntradaX52.ColaRequestSinUsar);
				if (!NT_SUCCESS(status))
				{
					WdfRequestComplete(Request, status);
				}
			}
			return;
		}
		else
		{
			if (Request == NULL)
			{
				forzada = TRUE;
				WdfIoQueueRetrieveNextRequest(devExt->EntradaX52.ColaRequestSinUsar, &Request);
			}
			if (Request == NULL)
			{
				WdfSpinLockRelease(devExt->HID.SpinLockAcciones);
				return;
			}
			else
			{
				UCHAR procesado = ProcesarAcciones(Device);
				WdfSpinLockRelease(devExt->HID.SpinLockAcciones);

				switch (procesado)
				{
					case 1:
						CompletarRequestDirectX(Device, Request);
						break;
					case 2:
						CompletarRequestRaton(Device, Request);
						break;
					case 3:
						CompletarRequestTeclado(Device, Request);
						break;
					default:
					{
						NTSTATUS status = (forzada) ? WdfRequestRequeue(Request) : WdfRequestForwardToIoQueue(Request, devExt->EntradaX52.ColaRequestSinUsar);
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

VOID CompletarRequestDirectX(WDFDEVICE device, WDFREQUEST request)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(HID_INPUT_DATA) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 1;
		RtlCopyMemory((PUCHAR)buffer + 1, &devExt->stDirectX, sizeof(HID_INPUT_DATA));
		WdfRequestSetInformation(request, sizeof(HID_INPUT_DATA) + 1);
	}
	WdfRequestComplete(request, status);
}

VOID CompletarRequestTeclado(WDFDEVICE device, WDFREQUEST request)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->stTeclado) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 3;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt->stTeclado, sizeof(devExt->stTeclado));
		WdfRequestSetInformation(request, sizeof(devExt->stTeclado) + 1);
	}
	WdfRequestComplete(request, status);
}

#pragma region "Ratón"
VOID CompletarRequestRaton(WDFDEVICE device, WDFREQUEST request)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	NTSTATUS		status;
	PVOID			buffer;

	WdfTimerStop(devExt->RatonTimer, FALSE);

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->stRaton) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 2;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt->stRaton, sizeof(devExt->stRaton));
		WdfRequestSetInformation(request, sizeof(devExt->stRaton) + 1);
	}
	WdfRequestComplete(request, status);

	if ((devExt->stRaton[1] == 0) && (devExt->stRaton[2] == 0))
	{
		devExt->RatonActivado = FALSE;
	}
	else
	{
		devExt->RatonActivado = TRUE;
		WdfTimerStart(devExt->RatonTimer, WDF_REL_TIMEOUT_IN_MS(GetDeviceContext(device)->Programacion.TickRaton));
	}
}

VOID EvtTickRaton(_In_ WDFTIMER Timer)
{
	WDFDEVICE	device = WdfTimerGetParentObject(Timer);
	UCHAR		accion[2] = { 0, 0 };

	if (!GetDeviceContext(device)->HID.RatonActivado)
		return;

	WdfSpinLockAcquire(GetDeviceContext(device)->HID.SpinLockAcciones);
	{
		if (GetDeviceContext(device)->HID.stRaton[1] != 0)
		{
			if (GetDeviceContext(device)->HID.stRaton[1] < 0)
			{
				accion[0] = TipoComando_RatonIzq | 64;
			}
			else
			{
				accion[0] = TipoComando_RatonDer| 64;
			}
			accion[1] = GetDeviceContext(device)->HID.stRaton[1];
		}
	}
	WdfSpinLockRelease(GetDeviceContext(device)->HID.SpinLockAcciones);
	if (accion[0] != 0)
	{
		AccionarRaton(device, accion);
	}

	accion[0] = 0;
	WdfSpinLockAcquire(GetDeviceContext(device)->HID.SpinLockAcciones);
	{
		if (GetDeviceContext(device)->HID.stRaton[2] != 0)
		{
			if (GetDeviceContext(device)->HID.stRaton[2] < 0)
			{
				accion[0] = TipoComando_RatonArr | 64;
			}
			else
			{
				accion[0] = TipoComando_RatonAba | 64;
			}
			accion[1] = GetDeviceContext(device)->HID.stRaton[2];
		}
	}
	WdfSpinLockRelease(GetDeviceContext(device)->HID.SpinLockAcciones);
	if (accion[0] != 0)
	{
		AccionarRaton(device, accion);
	}

	ForzarProcesarRequest(device);
}
#pragma endregion

