/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

driver.c

Abstract:

This file contains the driver entry points and callbacks.

Environment:

Kernel-mode Driver Framework

--*/
#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include <usbdi.h>
#include <usbdlib.h>
#include <wdfusb.h>
#include "PnP.h"
#include "context.h"
#include "x52_read.h"
#include "UserModeIO_read.h"
#include "Pedales_read.h"
#include "RequestHID_read.h"
#include "mapa.h"
#define _PRIVATE_
#include "driver.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, EvtAddDevice)
#pragma alloc_text (PAGE, IniciarContext)
#pragma alloc_text (PAGE, EvtCleanupCallback)
#endif


NTSTATUS DriverEntry(
	_In_ PDRIVER_OBJECT  DriverObject,
	_In_ PUNICODE_STRING RegistryPath
)
{
	WDF_DRIVER_CONFIG config;
	NTSTATUS status;

	WDF_DRIVER_CONFIG_INIT(&config, EvtAddDevice);
	status = WdfDriverCreate(DriverObject, RegistryPath, WDF_NO_OBJECT_ATTRIBUTES, &config, WDF_NO_HANDLE);

	return status;
}

NTSTATUS EvtAddDevice(
	_In_    WDFDRIVER       Driver,
	_Inout_ PWDFDEVICE_INIT DeviceInit
)
{
	NTSTATUS                        status;
	WDFDEVICE                       device;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDF_PNPPOWER_EVENT_CALLBACKS    pnpPowerCallbacks;
	WDF_IO_QUEUE_CONFIG				ioQConfig;
	WDFQUEUE						cola;
	PDEVICE_CONTEXT					dc;

	UNREFERENCED_PARAMETER(Driver);

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
		pnpPowerCallbacks.EvtDevicePrepareHardware = EvtDevicePrepareHardware;
		pnpPowerCallbacks.EvtDeviceSelfManagedIoInit = EvtDeviceSelfManagedIoInit;
		pnpPowerCallbacks.EvtDeviceSelfManagedIoCleanup = EvtDeviceSelfManagedIoCleanup;
		//pnpPowerCallbacks.EvtDeviceD0Entry = EvtDeviceD0Entry;
		pnpPowerCallbacks.EvtDeviceD0Exit = EvtDeviceD0Exit;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_CONTEXT);
		attributes.EvtCleanupCallback = EvtCleanupCallback;
	status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status))
		return status;

	RtlZeroMemory(GetDeviceContext(device), sizeof(DEVICE_CONTEXT));
	dc =  GetDeviceContext(device);

	status = IniciarContext(device);
	if (!NT_SUCCESS(status))
		return status;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchParallel);
		ioQConfig.EvtIoInternalDeviceControl = EvtX52InternalIOCtl;
	status = WdfIoQueueCreate(device, &ioQConfig, &attributes, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
		return status;

	WDF_IO_QUEUE_CONFIG_INIT(&ioQConfig, WdfIoQueueDispatchSequential);
	ioQConfig.EvtIoDefault = EvtRequestHID;
	status = WdfIoQueueCreate(device, &ioQConfig, &attributes, &cola);
	if (!NT_SUCCESS(status))
		return status;
	GetDeviceContext(device)->EntradaX52.ColaRequest = cola;

	WDF_IO_QUEUE_CONFIG_INIT(&ioQConfig, WdfIoQueueDispatchManual);
	status = WdfIoQueueCreate(device, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, &cola);
	if (!NT_SUCCESS(status))
		return status;
	GetDeviceContext(device)->EntradaX52.ColaRequestSinUsar = cola;

	status = IniciarIoCtlAplicacion(device);

	return status;
}

//PASSIVE
NTSTATUS IniciarContext(WDFDEVICE device)
{
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_TIMER_CONFIG		timerConfig;
	NTSTATUS				status;

	PAGED_CODE();

	status = IniciarX52(device);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarPedales(device);
	if (!NT_SUCCESS(status))
		return status;

	WDF_TIMER_CONFIG_INIT(&timerConfig, EvtTickRaton);
	timerConfig.AutomaticSerialization = TRUE;
	timerConfig.TolerableDelay = 0;
	timerConfig.UseHighResolutionTimer = WdfTrue;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;
	status = WdfTimerCreate(&timerConfig, &attributes, &GetDeviceContext(device)->HID.RatonTimer);
	if (!NT_SUCCESS(status))
		return status;

	WDF_TIMER_CONFIG_INIT(&timerConfig, EvtTickMenu);
	timerConfig.TolerableDelay = TolerableDelayUnlimited;
	status = WdfTimerCreate(&timerConfig, &attributes, &GetDeviceContext(device)->HID.MenuTimer);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfCollectionCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->HID.ListaTimersDelay);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Programacion.slCalibrado);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Programacion.slMapas);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Programacion.slComandos);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->HID.SpinLockDeltaHid);
	if (!NT_SUCCESS(status))
		return status;
	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->HID.SpinLockAcciones);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfWaitLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->SalidaX52.WaitLockX52);

	return status;
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

	if (GetDeviceContext(Device)->UsbDevice == NULL)
	{
		WDF_USB_DEVICE_CREATE_CONFIG createParams;
		WDF_USB_DEVICE_CREATE_CONFIG_INIT(&createParams, USBD_CLIENT_CONTRACT_VERSION_602);
		status = WdfUsbTargetDeviceCreateWithParameters(Device,	&createParams,	WDF_NO_OBJECT_ATTRIBUTES,&GetDeviceContext(Device)->UsbDevice);
	}


    return status;
}

VOID EvtCleanupCallback(WDFOBJECT  Object)
{
	WDFDEVICE device = (WDFDEVICE)Object;

	PAGED_CODE();

	CerrarPedales(device);

	if (GetDeviceContext(device)->ControlDevice != NULL)
	{
		WdfObjectDelete(GetDeviceContext(device)->ControlDevice);
		GetDeviceContext(device)->ControlDevice = NULL;
	}

	LimpiarMapa(device);
}