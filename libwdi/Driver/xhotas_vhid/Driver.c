/*++
Copyright (c) 2021 Alfredo Costalago

Module Name:

driver.c

Abstract:

This file contains the driver entry points and callbacks.

--*/
#include <ntddk.h>
#include <wdf.h>
#include <hidport.h>
#include <vhf.h>
#include "driver.h"
#include "ColaHID.h"
#include "ColaVHF.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, EvtAddDevice)
#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoCleanup)
#endif


NTSTATUS DriverEntry(_In_ PDRIVER_OBJECT DriverObject, _In_ PUNICODE_STRING RegistryPath)
{
	WDF_DRIVER_CONFIG config;
	NTSTATUS status;

	WDF_DRIVER_CONFIG_INIT(&config, EvtAddDevice);
	status = WdfDriverCreate(DriverObject, RegistryPath, WDF_NO_OBJECT_ATTRIBUTES, &config, WDF_NO_HANDLE);

	return status;
}

NTSTATUS EvtAddDevice(_In_ WDFDRIVER Driver,_Inout_ PWDFDEVICE_INIT DeviceInit)
{
	NTSTATUS                        status;
	WDFDEVICE                       device;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDF_PNPPOWER_EVENT_CALLBACKS    pnpPowerCallbacks;

	UNREFERENCED_PARAMETER(Driver);

	DECLARE_CONST_UNICODE_STRING(dosDeviceName, DOS_DEVICE_NAME);

	PAGED_CODE();

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
		pnpPowerCallbacks.EvtDeviceSelfManagedIoCleanup = EvtDeviceSelfManagedIoCleanup;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_CONTEXT);
	status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status))
		return status;

	RtlZeroMemory(GetDeviceContext(device), sizeof(DEVICE_CONTEXT));

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;
	status = WdfSpinLockCreate(&attributes, &GetDeviceContext(device)->LockHandle);
	if (!NT_SUCCESS(status))
	{
		return status;
	}

	status = CrearColaHID(device, &GetDeviceContext(device)->ColaHID);
	if (!NT_SUCCESS(status))
	{
		return status;
	}

	return WdfDeviceCreateSymbolicLink(device, &dosDeviceName);
}

VOID EvtDeviceSelfManagedIoCleanup(WDFDEVICE WdfDevice)
{
	PAGED_CODE();

	VhfLimpiar(WdfDevice);
}