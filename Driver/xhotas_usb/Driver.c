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
#include "PnP.h"
#include "context.h"
#include "x52_read.h"
#include "UserModeIO_read.h"
#include "Pedales_read.h"
#include "Teclado_read.h"
#include "Raton_read.h"
#define _PRIVATE_
#include "driver.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, EvtAddDevice)
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

	UNREFERENCED_PARAMETER(Driver);

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
		pnpPowerCallbacks.EvtDeviceSelfManagedIoInit = EvtDeviceSelfManagedIoInit;
		pnpPowerCallbacks.EvtDeviceSelfManagedIoCleanup = EvtDeviceSelfManagedIoCleanup;
		pnpPowerCallbacks.EvtDeviceD0Entry = EvtDeviceD0Entry;
		pnpPowerCallbacks.EvtDeviceD0Exit = EvtDeviceD0Exit;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_CONTEXT);
		attributes.EvtCleanupCallback = EvtCleanupCallback;
	status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status))
		return status;

	RtlZeroMemory(GetDeviceContext(device), sizeof(DEVICE_CONTEXT));
	GetDeviceContext(device)->Device = device;

	status = IniciarX52(device);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarPedales(device);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Programacion.slCalibrado);
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

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchParallel);
		ioQConfig.EvtIoInternalDeviceControl = EvtX52InternalIOCtl;
	status = WdfIoQueueCreate(device, &ioQConfig, &attributes, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
		return status;

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchManual);
	status = WdfIoQueueCreate(device, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, &cola);
	if (!NT_SUCCESS(status))
		return status;
	status = WdfIoQueueReadyNotify(cola, EvtTecladoListo, NULL);
	if (!NT_SUCCESS(status))
		return status;
	GetDeviceContext(device)->ColaTeclado = cola;

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchManual);
	status = WdfIoQueueCreate(device, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, &cola);
	if (!NT_SUCCESS(status))
		return status;
	status = WdfIoQueueReadyNotify(cola, EvtRatonListo, NULL);
	if (!NT_SUCCESS(status))
		return status;
	GetDeviceContext(device)->ColaRaton = cola;

	status = IniciarIoCtlAplicacion(device);

	return status;
}

VOID EvtCleanupCallback(_In_ WDFOBJECT  Object)
{
	PAGED_CODE();

	CerrarPedales((WDFDEVICE)Object);
	CerrarX52((WDFDEVICE)Object);
}