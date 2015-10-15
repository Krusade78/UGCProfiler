/*++

Copyright (c) 2015 Alfredo Costalago
Module Name:

    ioctl_user.c

Abstract: User mode ioctls

Environment:

    Kernel mode

--*/
#define _PRIVATE_
#include "ioctl_user.h"
#undef _PRIVATE_
#include "extensions.h"
#include "salida_x52.h"


#ifdef ALLOC_PRAGMA
	#pragma alloc_text(PAGE, IniciarIOUsrControl)
    #pragma alloc_text(PAGE, HF_IoControl)
#endif

DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\XUsb_HidF");
DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\XUSBInterface");

#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0107, METHOD_BUFFERED, FILE_WRITE_ACCESS)

NTSTATUS IniciarIOUsrControl(_In_ WDFDEVICE device)
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

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, IOUSR_CONTROL_EXTENSION);
	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	//attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	status = WdfDeviceCreate(&devInit, &attributes, &GetDeviceExtension(device)->UsrIOControlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	GetIOUsrControlExtension(GetDeviceExtension(device)->UsrIOControlDevice)->Padre = device;

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
		ioQConfig.EvtIoInternalDeviceControl = HF_IoControl;
	status = WdfIoQueueCreate(GetDeviceExtension(device)->UsrIOControlDevice, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(GetDeviceExtension(device)->UsrIOControlDevice);
		GetDeviceExtension(device)->UsrIOControlDevice = NULL;
		return status;
	}

	status = WdfDeviceCreateSymbolicLink(GetDeviceExtension(device)->UsrIOControlDevice, &dosDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(GetDeviceExtension(device)->UsrIOControlDevice);
		GetDeviceExtension(device)->UsrIOControlDevice = NULL;
		return status;
	}

	WdfControlFinishInitializing(GetDeviceExtension(device)->UsrIOControlDevice);

	return STATUS_SUCCESS;
}

void HF_IoControl(
	_In_  WDFQUEUE Queue,
	_In_  WDFREQUEST Request,
	_In_  size_t OutputBufferLength,
	_In_  size_t InputBufferLength,
	_In_  ULONG IoControlCode
	)
{
	NTSTATUS			status	= STATUS_SUCCESS;
	PUCHAR				SystemBuffer;	

    UNREFERENCED_PARAMETER(OutputBufferLength);
	UNREFERENCED_PARAMETER(InputBufferLength);
	UNREFERENCED_PARAMETER(Queue);

	PAGED_CODE();

    WdfRequestSetInformation(Request, 0);
	status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
	if(!NT_SUCCESS(status))
	{
		WdfRequestComplete(Request, status);
		return;
	}

	switch(IoControlCode)
	{
		case IOCTL_MFD_LUZ:
			{
				status = Luz_MFD(GetIOUsrControlExtension( WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_GLOBAL_LUZ:
			{
				status = Luz_Global(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_INFO_LUZ:
			{
				status = Luz_Info(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_PINKIE:
			{
				status = Set_Pinkie(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_TEXTO:
			{
				status = Set_Texto(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer, InputBufferLength);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
				break;
			}
		case IOCTL_HORA:
			{
				status = Set_Hora(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_HORA24:
			{
				status = Set_Hora24(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_FECHA:
			{
				status = Set_Fecha(GetIOUsrControlExtension(WdfIoQueueGetDevice(Queue))->Padre, SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 2);
				break;
			}
		default:
			status = STATUS_NOT_SUPPORTED;
	}

	WdfRequestComplete(Request, status);
}

