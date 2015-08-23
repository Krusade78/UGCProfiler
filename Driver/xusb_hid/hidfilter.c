/*++

Copyright (c) 2007 Alfredo Costalago
Module Name:

    hidfilter.c

Abstract: Filtro para - Human Interface Device (HID) USB driver

Environment:

    Kernel mode

--*/

#include <ntddk.h>
#include <wdf.h>
#include <usbdi.h>
#include <wdfusb.h>
#include "control.h"
#include "initguid.h"

#define _HIDFILTER_
#include "hidfilter.h"
#undef _HIDFILTER_


#ifdef ALLOC_PRAGMA
    #pragma alloc_text(INIT, DriverEntry)
    #pragma alloc_text(PAGE, HF_AddDevice)
    #pragma alloc_text( PAGE, IniciarInterfazControl)
#endif

DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\XUsb_HidF") ;
DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\XUSBInterface");

NTSTATUS DriverEntry
    (
    IN PDRIVER_OBJECT  DriverObject,
    IN PUNICODE_STRING RegistryPath
    )
{
	NTSTATUS			status = STATUS_SUCCESS;
    WDF_DRIVER_CONFIG   config;
	UNREFERENCED_PARAMETER(RegistryPath);

	WDF_DRIVER_CONFIG_INIT(&config, HF_AddDevice);
    status = WdfDriverCreate(DriverObject, RegistryPath, WDF_NO_OBJECT_ATTRIBUTES, &config, WDF_NO_HANDLE);

    return status;
}

NTSTATUS
HF_AddDevice(
    IN WDFDRIVER       Driver,
    IN PWDFDEVICE_INIT DeviceInit
    )
{
	NTSTATUS                        status;
    WDFDEVICE                       device;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDF_PNPPOWER_EVENT_CALLBACKS    pnpPowerCallbacks;
	WDF_IO_QUEUE_CONFIG				ioQConfig;

    UNREFERENCED_PARAMETER(Driver);
	UNREFERENCED_PARAMETER(ioQConfig);

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
		pnpPowerCallbacks.EvtDevicePrepareHardware	= EvtDevicePrepareHardware;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_EXTENSION);
		attributes.EvtCleanupCallback = EvtCleanupCallback;
    status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status)) return status;

	GetDeviceExtension(device)->fecha = 0;
	GetDeviceExtension(device)->UsbDevice = NULL;

	status = IniciarInterfazControl(Driver, GetDeviceExtension(device));

	return status;
}

NTSTATUS IniciarInterfazControl(IN WDFDRIVER Driver, IN PDEVICE_EXTENSION devExt)
{
	NTSTATUS                        status;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDF_IO_QUEUE_CONFIG				ioQConfig;
	PWDFDEVICE_INIT					devInit;
	
	PAGED_CODE();

	devInit = WdfControlDeviceInitAllocate(Driver, &SDDL_DEVOBJ_SYS_ALL_ADM_RWX_WORLD_RW_RES_R);
	if(devInit == NULL) return 1;

	status = WdfDeviceInitAssignName(devInit, &MyDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	WdfDeviceInitSetIoType(devInit, WdfDeviceIoBuffered);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, CONTROL_EXTENSION);
		attributes.ExecutionLevel = WdfExecutionLevelPassive;
		//attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	status = WdfDeviceCreate(&devInit, &attributes, &devExt->ControlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
		ioQConfig.EvtIoDeviceControl			= HF_Control;
	status = WdfIoQueueCreate(devExt->ControlDevice, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(devExt->ControlDevice);
		devExt->ControlDevice = NULL;
		return status;
	}

	status = WdfDeviceCreateSymbolicLink(devExt->ControlDevice, &dosDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(devExt->ControlDevice);
		devExt->ControlDevice = NULL;
		return status;
	}

	GetControlExtension(devExt->ControlDevice)->devExt = devExt;

	WdfControlFinishInitializing(devExt->ControlDevice);

	return STATUS_SUCCESS;
}

NTSTATUS
EvtDevicePrepareHardware(
    IN WDFDEVICE    Device,
    IN WDFCMRESLIST ResourceList,
    IN WDFCMRESLIST ResourceListTranslated
    )
{
    NTSTATUS status = STATUS_SUCCESS;

    UNREFERENCED_PARAMETER(ResourceList);
    UNREFERENCED_PARAMETER(ResourceListTranslated);

	if (GetDeviceExtension(Device)->UsbDevice == NULL)
        status = WdfUsbTargetDeviceCreate(Device, WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceExtension(Device)->UsbDevice);

    return status;
}

VOID
  EvtCleanupCallback (
    IN WDFOBJECT  Object
    )
{
	if(GetDeviceExtension(Object)->ControlDevice != NULL)
		WdfObjectDelete(GetDeviceExtension(Object)->ControlDevice);
}