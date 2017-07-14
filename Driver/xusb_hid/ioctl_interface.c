/*++

Copyright (c) 2015 Alfredo Costalago
Module Name:

    ioctl_user.c

Abstract: User mode ioctls

Environment:

    Kernel mode

--*/
#define _PRIVATE_
#include "ioctl_interface.h"
#undef _PRIVATE_
#include "extensions.h"
#include "salida_x52.h"
#include <initguid.h>


#ifdef ALLOC_PRAGMA
	#pragma alloc_text(PAGE, IniciarIOUsrControl)
	#pragma alloc_text(PAGE, IniciarInterface)
    #pragma alloc_text(PAGE, IoControl)
#endif

DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\XUsb_HidF");
DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\XUSBInterface");
// {55074FCB-1C35-44A4-B336-1C4324FD8058}
DEFINE_GUID(GUID_XUSB_INTERFACE, 0x55074fcb, 0x1c35, 0x44a4, 0xb3, 0x36, 0x1c, 0x43, 0x24, 0xfd, 0x80, 0x58);

#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0107, METHOD_BUFFERED, FILE_WRITE_ACCESS)

//PASSIVE_LEVEL
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
		ioQConfig.EvtIoInternalDeviceControl = IoControl;
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

	status = IniciarInterface(device, GetDeviceExtension(device)->UsrIOControlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(GetDeviceExtension(device)->UsrIOControlDevice);
		GetDeviceExtension(device)->UsrIOControlDevice = NULL;
		return status;
	}

	return STATUS_SUCCESS;
}

//PASSIVE_LEVEL
NTSTATUS IniciarInterface(WDFDEVICE device, WDFDEVICE deviceControl)
{
	CONTROL_INTERFACE			interface;
	PINTERFACE					interfaceHeader;
	WDF_QUERY_INTERFACE_CONFIG	queryInterfaceConfig;

	PAGED_CODE();

	interfaceHeader = &interface.InterfaceHeader;

	interfaceHeader->Size = sizeof(CONTROL_INTERFACE);
	interfaceHeader->Version = 1;
	interfaceHeader->Context = (PVOID)device;
	interfaceHeader->InterfaceReference = WdfDeviceInterfaceReferenceNoOp;
	interfaceHeader->InterfaceDereference = WdfDeviceInterfaceDereferenceNoOp;
	interface.EnviarOrdenX52 = EnviarOrdenX52;

	WDF_QUERY_INTERFACE_CONFIG_INIT(&queryInterfaceConfig, interfaceHeader, &GUID_XUSB_INTERFACE, WDF_NO_EVENT_CALLBACK);

	return WdfDeviceAddQueryInterface(deviceControl, &queryInterfaceConfig);
}

//PASSIVE_LEVEL
void IoControl(
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
	UNREFERENCED_PARAMETER(Queue);

	PAGED_CODE();

    WdfRequestSetInformation(Request, 0);
	status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
	if(!NT_SUCCESS(status))
	{
		WdfRequestComplete(Request, status);
		return;
	}
	if (IoControlCode != IOCTL_TEXTO)
	{
		if ((IoControlCode != IOCTL_FECHA) && (InputBufferLength != 1))
		{
			WdfRequestSetInformation(Request, 1);
			WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
			return;
		}
		else if ((IoControlCode == IOCTL_FECHA) && (InputBufferLength != 2))
		{
			WdfRequestSetInformation(Request, 2);
			WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
			return;
		}
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

//PASSIVE_LEVEL
void EnviarOrdenX52(_In_ PINTERFACE interfaceHeader, _In_ ULONG ctlCode, _In_ PUCHAR buffer, _In_ size_t tamBuffer)
{
	PAGED_CODE();

	switch (ctlCode)
	{
		case IOCTL_MFD_LUZ:
		{
			Luz_MFD((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		case IOCTL_GLOBAL_LUZ:
		{
			Luz_Global((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		case IOCTL_INFO_LUZ:
		{
			Luz_Info((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		case IOCTL_PINKIE:
		{
			Set_Pinkie((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		case IOCTL_TEXTO:
		{
			Set_Texto((WDFDEVICE)interfaceHeader->Context, buffer, tamBuffer);
			break;
		}
		case IOCTL_HORA:
		{
			Set_Hora((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		case IOCTL_HORA24:
		{
			Set_Hora24((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		case IOCTL_FECHA:
		{
			Set_Fecha((WDFDEVICE)interfaceHeader->Context, buffer);
			break;
		}
		default:
			break;
	}
}

