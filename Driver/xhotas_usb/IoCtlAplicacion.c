/*++
Copyright (c) 2021 Alfredo Costalago

Module Name:

IoCtlAplicacion.c

Abstract:

Archivo de lectura de io control desde las aplicaciones de usuario.

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "EscribirUSBX52.h"
#define _PRIVATE_
#include "IoCtlAplicacion.h"
#undef _PRIVATE_
#undef _PUBLIC_

DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\X52_XHOTAS_Control");
DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\X52_XHOTASControl");

#ifdef ALLOC_PRAGMA
    #pragma alloc_text( PAGE, IniciarIoCtlAplicacion)
	#pragma alloc_text( PAGE, CerrarIoCtlAplicacion)
	#pragma alloc_text( PAGE, EvtIOCtlAplicacion)
#endif /* ALLOC_PRAGMA */

NTSTATUS IniciarIoCtlAplicacion(_In_ WDFDEVICE device)
{
	NTSTATUS                 status;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_IO_QUEUE_CONFIG		ioQConfig;
	PWDFDEVICE_INIT			devInit;
	WDFDEVICE				ctlDevice;

	PAGED_CODE();

	devInit = WdfControlDeviceInitAllocate(WdfDeviceGetDriver(device), &SDDL_DEVOBJ_SYS_ALL_ADM_RWX_WORLD_RW_RES_R);
	if (devInit == NULL)
		return STATUS_UNSUCCESSFUL;

	status = WdfDeviceInitAssignName(devInit, &MyDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	WdfDeviceInitSetExclusive(devInit, TRUE);
	WdfDeviceInitSetIoType(devInit, WdfDeviceIoBuffered);


	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, CONTROL_CONTEXT);
	status = WdfDeviceCreate(&devInit, &attributes, &ctlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}
	GetControlContext(ctlDevice)->Padre = device;

	status = WdfDeviceCreateSymbolicLink(ctlDevice, &dosDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(ctlDevice);
		return status;
	}

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
	ioQConfig.EvtIoDeviceControl = EvtIOCtlAplicacion;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	status = WdfIoQueueCreate(ctlDevice, &ioQConfig, &attributes, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(ctlDevice);
		return status;
	}

	WdfControlFinishInitializing(ctlDevice);
	GetDeviceContext(device)->ControlDevice = ctlDevice;

	return STATUS_SUCCESS;
}

VOID CerrarIoCtlAplicacion(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	if (GetDeviceContext(device)->ControlDevice != NULL)
	{
		WdfObjectDelete(GetDeviceContext(device)->ControlDevice);
		GetDeviceContext(device)->ControlDevice = NULL;
	}
}

VOID EvtIOCtlAplicacion(
	_In_  WDFQUEUE Queue,
	_In_  WDFREQUEST Request,
	_In_  size_t OutputBufferLength,
	_In_  size_t InputBufferLength,
	_In_  ULONG IoControlCode
)
{
	NTSTATUS	status;
	PUCHAR		SystemBuffer = NULL;
	WDFDEVICE	device = GetControlContext(WdfIoQueueGetDevice(Queue))->Padre;

	UNREFERENCED_PARAMETER(OutputBufferLength);

	PAGED_CODE();

	if (InputBufferLength > 0)
	{
		status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
		if (!NT_SUCCESS(status))
		{
			WdfRequestSetInformation(Request, 0);
			WdfRequestComplete(Request, status);
			return;
		}
	}
	if (SystemBuffer == NULL)
	{
		WdfRequestSetInformation(Request, 0);
		WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
		return;
	}
	if (IoControlCode == IOCTL_X52)
	{
		if (InputBufferLength != 3)
		{
			WdfRequestSetInformation(Request, 3);
			WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
			return;
		}
	}

	switch (IoControlCode)
	{
	//------------------- X52_write.c -----------------------------------------
	case IOCTL_TEXTO:
	{
		status = Set_Texto(device, SystemBuffer, InputBufferLength);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_X52:
	{
		status = EnviarOrden(device, SystemBuffer, 1);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 3);
		WdfRequestComplete(Request, status);
		break;
	}
	default:
		WdfRequestSetInformation(Request, 0);
		WdfRequestComplete(Request, STATUS_NOT_SUPPORTED);
		break;
	}
}