/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

x52_ioctl_usuario.c

Abstract:

Archivo de lectura de io control desde las aplicaciones de usuario.

Environment:



--*/
#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "X52_write.h"
#include "CalibradoHID.h"
#include "mapa.h"
#define _PRIVATE_
#include "UserModeIO_read.h"
#undef _PRIVATE_

DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\XUsb_HidF");
DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\XUSBInterface");

//PASSIVE_LEVEL
NTSTATUS IniciarIoCtlAplicacion(_In_ WDFDEVICE device)
{
	NTSTATUS                        status;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDF_IO_QUEUE_CONFIG				ioQConfig;
	PWDFDEVICE_INIT					devInit;

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

	WdfDeviceInitSetIoType(devInit, WdfDeviceIoBuffered);

	status = WdfDeviceCreate(&devInit, WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->ControlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	status = WdfDeviceCreateSymbolicLink(&GetDeviceContext(device)->ControlDevice, &dosDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(&GetDeviceContext(device)->ControlDevice);
		&GetDeviceContext(device)->ControlDevice = NULL;
		return status;
	}

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
	ioQConfig.EvtIoInternalDeviceControl = EvtIOCtlAplicacion;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;
	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	status = WdfIoQueueCreate(GetDeviceContext(device)->ControlDevice, &ioQConfig, &attributes, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(&GetDeviceContext(device)->ControlDevice);
		&GetDeviceContext(device)->ControlDevice = NULL;
		return status;
	}

	WdfControlFinishInitializing(&GetDeviceContext(device)->ControlDevice);

	return STATUS_SUCCESS;
}

//PASSIVE_LEVEL
VOID EvtIOCtlAplicacion(
	_In_  WDFQUEUE Queue,
	_In_  WDFREQUEST Request,
	_In_  size_t OutputBufferLength,
	_In_  size_t InputBufferLength,
	_In_  ULONG IoControlCode
)
{
	WDFDEVICE	device;
	NTSTATUS	status;
	BOOLEAN		ret;
	PUCHAR		SystemBuffer;
	WDF_REQUEST_SEND_OPTIONS options;

	UNREFERENCED_PARAMETER(OutputBufferLength);

	status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfRequestSetInformation(Request, 0);
		WdfRequestComplete(Request, status);
		return;
	}
	switch (IoControlCode)
	{
		case IOCTL_MFD_LUZ:
		case IOCTL_GLOBAL_LUZ:
		case IOCTL_INFO_LUZ:
		case IOCTL_PINKIE:
		case IOCTL_HORA:
		case IOCTL_HORA24:
			if (InputBufferLength != 1)
			{
				WdfRequestSetInformation(Request, 1);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
		case IOCTL_FECHA:
		{
			if (InputBufferLength != 2)
			{
				WdfRequestSetInformation(Request, 2);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
		}
		default:
			break;
	}

	switch (IoControlCode)
	{
	case IOCTL_RAW:
		GetDeviceContext(WdfIoQueueGetDevice(Queue))->HID.ModoRaw = SystemBuffer[0];
		WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	case IOCTL_USR_CALIBRADO:
		status = EscribirCalibrado(WdfIoQueueGetDevice(Queue), Request);
		return;
	case IOCTL_USR_MAPA:
		status = HF_IoEscribirMapa(WdfIoQueueGetDevice(Queue));
	case IOCTL_USR_COMANDOS:
		status = HF_IoEscribirComandos(WdfIoQueueGetDevice(Queue));
	//---------------------------------------------------------------------
	case IOCTL_MFD_LUZ:
	{
		status = Luz_MFD(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_GLOBAL_LUZ:
	{
		status = Luz_Global(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_INFO_LUZ:
	{
		status = Luz_Info(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_PINKIE:
	{
		status = Set_Pinkie(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_TEXTO:
	{
		status = Set_Texto(WdfIoQueueGetDevice(Queue), SystemBuffer, InputBufferLength);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_HORA:
	{
		status = Set_Hora(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_HORA24:
	{
		status = Set_Hora24(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_FECHA:
	{
		status = Set_Fecha(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 2);
		WdfRequestComplete(Request, status);
		return;
	}
	default:
		break;
	}

	WDF_REQUEST_SEND_OPTIONS_INIT(&options, WDF_REQUEST_SEND_OPTION_SEND_AND_FORGET);
	ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), &options);
	if (ret == FALSE)
	{
		status = WdfRequestGetStatus(Request);
		WdfRequestComplete(Request, status);
	}
}