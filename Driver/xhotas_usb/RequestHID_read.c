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

VOID EvtRequestHIDLista(_In_ WDFQUEUE Queue, _In_ WDFCONTEXT Context)
{
	UNREFERENCED_PARAMETER(Context);

	ProcesarRequest(WdfIoQueueGetDevice(Queue));
}

VOID ProcesarRequest(WDFDEVICE device)
{
	PDEVICE_CONTEXT devExt = GetDeviceContext(device);

	while (TRUE)
	{
		BOOLEAN vacia;
		WdfSpinLockAcquire(devExt->HID.SpinLockAcciones);
		{
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
		}
		WdfSpinLockRelease(devExt->HID.SpinLockAcciones);
		if (!vacia)
		{
			WDFREQUEST	request = NULL;
			NTSTATUS	status = WdfIoQueueRetrieveNextRequest(devExt->ColaRequest, &request);

			if (NT_SUCCESS(status))
			{
				ProcesarAcciones(device, request);
			}
			else if (status == STATUS_NO_MORE_ENTRIES)
			{
				break;
			}
			else if (request != NULL)
			{
				WdfRequestComplete(request, STATUS_UNSUCCESSFUL);
			}
		}
		else
			break;
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
}
#pragma endregion

