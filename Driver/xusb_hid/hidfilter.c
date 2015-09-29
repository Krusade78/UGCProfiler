/*++

Copyright (c) 2007 Alfredo Costalago
Module Name:

    hidfilter.c

Abstract: Filtro para - Human Interface Device (HID) USB driver

Environment:

    Kernel mode

--*/



//#include <usbdi.h>
//#include <usbdlib.h>
//#include <wdfusb.h>
//#include <initguid.h>

#define _PRIVATE_
#include "hidfilter.h"
#undef _PRIVATE_
#include "extensions.h"
#include "ioctl_x52.h"
#include "pedales.h"
//#include "control.h"


#ifdef ALLOC_PRAGMA
    #pragma alloc_text(INIT, DriverEntry)
    #pragma alloc_text(PAGE, AddDevice)
    #pragma alloc_text(PAGE, IniciarInterfazControl)
#endif

//DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\XUsb_HidF") ;
//DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\XUSBInterface");

NTSTATUS DriverEntry
    (
    IN PDRIVER_OBJECT  DriverObject,
    IN PUNICODE_STRING RegistryPath
    )
{
	NTSTATUS			status = STATUS_SUCCESS;
    WDF_DRIVER_CONFIG   config;

	WDF_DRIVER_CONFIG_INIT(&config, AddDevice);
    status = WdfDriverCreate(DriverObject, RegistryPath, WDF_NO_OBJECT_ATTRIBUTES, &config, WDF_NO_HANDLE);

    return status;
}

NTSTATUS AddDevice
	(
    IN WDFDRIVER       Driver,
    IN PWDFDEVICE_INIT DeviceInit
    )
{
	NTSTATUS                        status;
    WDFDEVICE                       device;
	WDF_OBJECT_ATTRIBUTES			attributes;
	//WDF_PNPPOWER_EVENT_CALLBACKS    pnpPowerCallbacks;
	//WDF_IO_QUEUE_CONFIG				ioQConfig;

    UNREFERENCED_PARAMETER(Driver);
	//UNREFERENCED_PARAMETER(ioQConfig);

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	//WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
	//	pnpPowerCallbacks.EvtDevicePrepareHardware	= EvtDevicePrepareHardware;
	//WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_EXTENSION);
		attributes.EvtCleanupCallback = CleanupCallback;
    status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status)) return status;

	RtlZeroMemory(GetDeviceExtension(device), sizeof(DEVICE_EXTENSION));
	GetDeviceExtension(device)->Self = device;

	status = IniciarPedales(device);
	if (!NT_SUCCESS(status)) return status;

	//GetDeviceExtension(device)->fecha = 0;
	//GetDeviceExtension(device)->UsbDevice = NULL;

	status = IniciarInterfazControl(device);

	return status;
}

NTSTATUS IniciarInterfazControl(_In_ WDFDEVICE device)
{
	NTSTATUS                        status;
	//WDF_OBJECT_ATTRIBUTES			attributes;
	WDF_IO_QUEUE_CONFIG				ioQConfig;
	//PWDFDEVICE_INIT					devInit;
	
	//UNREFERENCED_PARAMETER(Driver);
	UNREFERENCED_PARAMETER(device);

	PAGED_CODE();

	//devInit = WdfControlDeviceInitAllocate(Driver, &SDDL_DEVOBJ_SYS_ALL_ADM_RWX_WORLD_RW_RES_R);
	//if(devInit == NULL) return 1;

	//status = WdfDeviceInitAssignName(devInit, &MyDeviceName);
	//if (!NT_SUCCESS(status))
	//{
	//	WdfDeviceInitFree(devInit);
	//	return status;
	//}

	//WdfDeviceInitSetIoType(devInit, WdfDeviceIoBuffered);

	//WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, CONTROL_EXTENSION);
	//	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	//	//attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	//status = WdfDeviceCreate(&devInit, &attributes, &devExt->ControlDevice);
	//if (!NT_SUCCESS(status))
	//{
	//	WdfDeviceInitFree(devInit);
	//	return status;
	//}

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
		ioQConfig.EvtIoInternalDeviceControl = HF_X52IOCtl;
	status = WdfIoQueueCreate(device/*devExt->ControlDevice*/, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
	{
		//WdfObjectDelete(devExt->ControlDevice);
		//devExt->ControlDevice = NULL;
		return status;
	}

	//status = WdfDeviceCreateSymbolicLink(devExt->ControlDevice, &dosDeviceName);
	//if (!NT_SUCCESS(status))
	//{
	//	WdfObjectDelete(devExt->ControlDevice);
	//	devExt->ControlDevice = NULL;
	//	return status;
	//}

	//GetControlExtension(devExt->ControlDevice)->devExt = devExt;

	//WdfControlFinishInitializing(devExt->ControlDevice);

	return STATUS_SUCCESS;
}

//NTSTATUS
//EvtDevicePrepareHardware(
//    IN WDFDEVICE    Device,
//    IN WDFCMRESLIST ResourceList,
//    IN WDFCMRESLIST ResourceListTranslated
//    )
//{
//	PDEVICE_CONTEXT pDeviceContext;
//    NTSTATUS status = STATUS_SUCCESS;
//
//    UNREFERENCED_PARAMETER(ResourceList);
//    UNREFERENCED_PARAMETER(ResourceListTranslated);
//
//	pDeviceContext = DeviceGetContext(Device);
//
//	if (GetDeviceExtension(Device)->UsbDevice == NULL)
//	{
//		WDF_USB_DEVICE_CREATE_CONFIG createParams;
//		WDF_USB_DEVICE_CREATE_CONFIG_INIT(&createParams, USBD_CLIENT_CONTRACT_VERSION_602);
//
//		//status = WdfUsbTargetDeviceCreate(Device, WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceExtension(Device)->UsbDevice);
//		status = WdfUsbTargetDeviceCreateWithParameters(Device,	&createParams,	WDF_NO_OBJECT_ATTRIBUTES,&GetDeviceExtension(Device)->UsbDevice);
//	}
//
//
//    return status;
//}

VOID CleanupCallback (_In_ WDFOBJECT  Object)
{
	CerrarPedales((WDFDEVICE)Object);
//	if(GetDeviceExtension(Object)->ControlDevice != NULL)
//		WdfObjectDelete(GetDeviceExtension(Object)->ControlDevice);
}