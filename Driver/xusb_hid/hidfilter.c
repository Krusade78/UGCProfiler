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
#include "ioctl_user.h"


#ifdef ALLOC_PRAGMA
    #pragma alloc_text(INIT, DriverEntry)
    #pragma alloc_text(PAGE, AddDevice)
#endif

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
	WDF_IO_QUEUE_CONFIG				ioQConfig;

    UNREFERENCED_PARAMETER(Driver);

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	//WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
	//	pnpPowerCallbacks.EvtDevicePrepareHardware	= EvtDevicePrepareHardware;
	//WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_EXTENSION);
		attributes.EvtCleanupCallback = CleanupCallback;
    status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status))
		return status;

	RtlZeroMemory(GetDeviceExtension(device), sizeof(DEVICE_EXTENSION));
	GetDeviceExtension(device)->Self = device;
	GetDeviceExtension(device)->Pedales.Activado = TRUE;

	status = IniciarX52(device);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarPedales(device);
	if (!NT_SUCCESS(status))
		return status;

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
	ioQConfig.EvtIoInternalDeviceControl = HF_X52IOCtl;
	status = WdfIoQueueCreate(device, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarIOUsrControl(device);

	return status;
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
	CerrarX52((WDFDEVICE)Object);
	if(GetDeviceExtension(Object)->UsrIOControlDevice != NULL)
		WdfObjectDelete(GetDeviceExtension(Object)->UsrIOControlDevice);
}