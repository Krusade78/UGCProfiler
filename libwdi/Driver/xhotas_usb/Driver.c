/*++
Copyright (c) 2021 Alfredo Costalago

Module Name:

driver.c

Abstract:

This file contains the driver entry points and callbacks.

--*/
#include <ntddk.h>
#include <wdf.h>
#include <usbdi.h>
#include <usbdlib.h>
#include <wdfusb.h>
#include "context.h"
#define _PUBLIC_
#include "IoCtlAplicacion.h"
#include "EscribirUSBX52.h"
#undef _PUBLIC_
#define _PRIVATE_
#include "driver.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, EvtAddDevice)
#pragma alloc_text (PAGE, IniciarContextX52)
#pragma alloc_text (PAGE, EvtDevicePrepareHardware)
#pragma alloc_text (PAGE, EvtDeviceSurpriseRemoval)
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

	UNREFERENCED_PARAMETER(Driver);

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
		pnpPowerCallbacks.EvtDevicePrepareHardware = EvtDevicePrepareHardware;
		pnpPowerCallbacks.EvtDeviceSurpriseRemoval = EvtDeviceSurpriseRemoval;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_CONTEXT);
		attributes.EvtCleanupCallback = EvtCleanupCallback;
	status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status))
		return status;

	RtlZeroMemory(GetDeviceContext(device), sizeof(DEVICE_CONTEXT));

	status = IniciarContextX52(device);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarIoCtlAplicacion(device);

	return status;
}

NTSTATUS IniciarContextX52(_In_ WDFDEVICE device)
{
	NTSTATUS status;
	WDF_OBJECT_ATTRIBUTES	attributes;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	status = WdfCollectionCreate(&attributes, &GetDeviceContext(device)->SalidaX52.Ordenes);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->SalidaX52.WaitLockOrdenes);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->SalidaX52.WaitLockX52);

	return status;
}

/// <summary>
/// PASSIVE
/// </summary>
NTSTATUS EvtDevicePrepareHardware(
    IN WDFDEVICE    Device,
    IN WDFCMRESLIST ResourceList,
    IN WDFCMRESLIST ResourceListTranslated
    )
{
    NTSTATUS status = STATUS_SUCCESS;

    UNREFERENCED_PARAMETER(ResourceList);
    UNREFERENCED_PARAMETER(ResourceListTranslated);

	PAGED_CODE();

	if (GetDeviceContext(Device)->UsbDevice == NULL)
	{
		WDF_USB_DEVICE_CREATE_CONFIG createParams;
		WDF_USB_DEVICE_CREATE_CONFIG_INIT(&createParams, USBD_CLIENT_CONTRACT_VERSION_602);
		status = WdfUsbTargetDeviceCreateWithParameters(Device,	&createParams,	WDF_NO_OBJECT_ATTRIBUTES,&GetDeviceContext(Device)->UsbDevice);
	}

    return status;
}

/// <summary>
/// PASSIVE
/// </summary>
/// <param name="WDFDEVICE"></param>
VOID EvtDeviceSurpriseRemoval(WDFDEVICE device)
{
	PAGED_CODE();
	EvtCleanupCallback(device);
}

/// <summary>
/// PASSIVE
/// </summary>
/// <param name="WDFDEVICE"></param>
VOID EvtCleanupCallback(WDFOBJECT  Object)
{
	WDFDEVICE device = (WDFDEVICE)Object;

	PAGED_CODE();

	CerrarIoCtlAplicacion(device);
	LimpiarSalidaX52(device);
}