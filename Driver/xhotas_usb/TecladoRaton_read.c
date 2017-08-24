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
#include "acciones.h"
#include "TecladoRaton_read.h"

VOID EvtTecladoRatonListo(_In_ WDFQUEUE Queue, _In_ WDFCONTEXT Context)
{
	UNREFERENCED_PARAMETER(Context);
	UNREFERENCED_PARAMETER(Queue);
	//ProcesarAcciones(WdfIoQueueGetDevice(Queue), TRUE);
}

#pragma region "Teclado"
BOOLEAN ProcesarEventoTeclado(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	WDFREQUEST		request;
	BOOLEAN			soltar;
	NTSTATUS		status;
	PVOID			buffer;

	soltar = ((tipo & 32) == 32) ? TRUE : FALSE;
	tipo &= 0x1f;

	if (!soltar)
		devExt->stTeclado[dato / 8] |= 1 << (dato % 8);
	else
		devExt->stTeclado[dato / 8] &= ~(1 << (dato % 8));

	status = WdfIoQueueRetrieveNextRequest(GetDeviceContext(device)->ColaRequest, &request);
	if (((status == STATUS_NO_MORE_ENTRIES) || NT_SUCCESS(status)))
	{
		if (request == NULL)
			return FALSE;
	}
	if (!NT_SUCCESS(status))
		return FALSE;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->stTeclado) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 3;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt->stTeclado, sizeof(devExt->stTeclado));
		WdfRequestSetInformation(request, sizeof(devExt->stTeclado) + 1);
	}
	WdfRequestComplete(request, status);
	return TRUE;
}
#pragma endregion

#pragma region "Ratón"
BOOLEAN ProcesarEventoRaton(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	WDFREQUEST		request;
	BOOLEAN			soltar;
	NTSTATUS		status;
	PVOID			buffer;

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

	status = WdfIoQueueRetrieveNextRequest(GetDeviceContext(device)->ColaRequest, &request);
	if (((status == STATUS_NO_MORE_ENTRIES) || NT_SUCCESS(status)))
	{
		if (request == NULL)
			return FALSE;
	}
	if (!NT_SUCCESS(status))
		return FALSE;

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
	return TRUE;
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
		AccionarRaton(device, accion, TRUE);
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
		AccionarRaton(device, accion, TRUE);
	}
}
#pragma endregion