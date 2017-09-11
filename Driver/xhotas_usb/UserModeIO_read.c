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

#ifdef ALLOC_PRAGMA
    #pragma alloc_text( PAGE, IniciarIoCtlAplicacion)
	#pragma alloc_text( PAGE, EvtIOCtlAplicacion)
#endif /* ALLOC_PRAGMA */

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

	WdfDeviceInitSetExclusive(devInit, TRUE);
	WdfDeviceInitSetIoType(devInit, WdfDeviceIoBuffered);


	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, CONTROL_CONTEXT);
	status = WdfDeviceCreate(&devInit, &attributes, &GetDeviceContext(device)->ControlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}
	GetControlContext(GetDeviceContext(device)->ControlDevice)->padre = device;

	status = WdfDeviceCreateSymbolicLink(GetDeviceContext(device)->ControlDevice, &dosDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(GetDeviceContext(device)->ControlDevice);
		GetDeviceContext(device)->ControlDevice = NULL;
		return status;
	}

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
	ioQConfig.EvtIoDeviceControl = EvtIOCtlAplicacion;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	status = WdfIoQueueCreate(GetDeviceContext(device)->ControlDevice, &ioQConfig, &attributes, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(GetDeviceContext(device)->ControlDevice);
		GetDeviceContext(device)->ControlDevice = NULL;
		return status;
	}

	WdfControlFinishInitializing(GetDeviceContext(device)->ControlDevice);

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
	NTSTATUS	status;
	PUCHAR		SystemBuffer = NULL;
	WDFDEVICE	device = GetControlContext(WdfIoQueueGetDevice(Queue))->padre;

	UNREFERENCED_PARAMETER(OutputBufferLength);

	PAGED_CODE();

	if (IoControlCode == IOCTL_GET_MENU)
	{
		status = WdfRequestRetrieveOutputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
		if (!NT_SUCCESS(status))
		{
			WdfRequestSetInformation(Request, 1);
			WdfRequestComplete(Request, status);
			return;
		}
	}
	else if (IoControlCode == IOCTL_DESACTIVAR_MENU)
	{ }
	else
	{
		status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
		if (!NT_SUCCESS(status))
		{
			WdfRequestSetInformation(Request, 0);
			WdfRequestComplete(Request, status);
			return;
		}
	}
	switch (IoControlCode)
	{
		case IOCTL_GET_MENU:
			if (OutputBufferLength != 1)
			{
				WdfRequestSetInformation(Request, 1);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
			break;
		case IOCTL_DESACTIVAR_MENU:
			if (InputBufferLength != 0)
			{
				WdfRequestSetInformation(Request, 0);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
			break;
		case IOCTL_USR_RAW:
		case IOCTL_MFD_LUZ:
		case IOCTL_GLOBAL_LUZ:
		case IOCTL_INFO_LUZ:
		case IOCTL_PEDALES:
			if (InputBufferLength != 1)
			{
				WdfRequestSetInformation(Request, 1);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
			break;
		case IOCTL_FECHA:
		{
			if (InputBufferLength != 2)
			{
				WdfRequestSetInformation(Request, 2);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
			break;
		}
		case IOCTL_HORA:
		case IOCTL_HORA24:
		{
			if (InputBufferLength != 3)
			{
				WdfRequestSetInformation(Request, 2);
				WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
				return;
			}
			break;
		}
		default:
			break;
	}

	switch (IoControlCode)
	{
	case IOCTL_USR_RAW:
	{
		GetDeviceContext(device)->HID.ModoRaw = SystemBuffer[0];
		WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, STATUS_SUCCESS);
	}
		break;
	//------------------- CalibradoHID.c ---------------------------------
	case IOCTL_USR_CALIBRADO:
		status = EscribirCalibrado(device, Request);
		WdfRequestComplete(Request, status);
		break;
	//-------------------- Mapa.c -----------------------------------
	case IOCTL_USR_MAPA:
		status = HF_IoEscribirMapa(device, Request);
		WdfRequestComplete(Request, status);
		break;
	case IOCTL_USR_COMANDOS:
		status = HF_IoEscribirComandos(device, Request);
		WdfRequestComplete(Request, status);
		break;
	//-------------------- Menu -----------------------------------
	case IOCTL_GET_MENU:
	{
		SystemBuffer[0] = GetDeviceContext(device)->HID.MenuActivado;
		WdfRequestCompleteWithInformation(Request, STATUS_SUCCESS, 1);
		break;
	}
	case IOCTL_DESACTIVAR_MENU:
	{
		UCHAR dato = 0;
		status = Luz_Info(device, &dato);
		GetDeviceContext(device)->HID.MenuActivado = FALSE;
		WdfRequestComplete(Request, status);
		break;
	}
	//-------------------- Pedales -----------------------------------
	case IOCTL_PEDALES:
	{
		GetDeviceContext(device)->Pedales.Activado = SystemBuffer[0];
		WdfRequestComplete(Request, STATUS_SUCCESS);
		break;
	}
	//------------------- X52_write.c -----------------------------------------
	case IOCTL_MFD_LUZ:
	{
		status = Luz_MFD(device, SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_GLOBAL_LUZ:
	{
		status = Luz_Global(device, SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_INFO_LUZ:
	{
		status = Luz_Info(device, SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_TEXTO:
	{
		status = Set_Texto(device, SystemBuffer, InputBufferLength);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_HORA:
	{
		status = Set_Hora(device, SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_HORA24:
	{
		status = Set_Hora24(device, SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		break;
	}
	case IOCTL_FECHA:
	{
		status = Set_Fecha(device, SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 2);
		WdfRequestComplete(Request, status);
		break;
	}
	default:
		WdfRequestSetInformation(Request, 0);
		WdfRequestComplete(Request, STATUS_NOT_SUPPORTED);
		break;
	}
}

VOID EvtTickMenu(_In_ WDFTIMER Timer)
{
	UCHAR dato = 1;
	GetDeviceContext(WdfTimerGetParentObject(Timer))->HID.MenuActivado = TRUE;
	GetDeviceContext(WdfTimerGetParentObject(Timer))->HID.MenuTimerEsperando = FALSE;
	Luz_Info(WdfTimerGetParentObject(Timer), &dato);
}