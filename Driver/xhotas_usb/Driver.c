/*++
Copyright (c) 2020 Alfredo Costalago

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
#include "PnPPower.h"
#include "PnPPedales.h"
#include "LeerUSBX52.h"
#include "IoCtlAplicacion.h"
#include "ProcesarHID.h"
#include "EventosProcesar.h"
#include "Perfil.h"
#include "MenuMFD.h"
#include "EscribirUSBX52.h"
#undef _PUBLIC_
#define _PRIVATE_
#include "driver.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (INIT, DriverEntry)
#pragma alloc_text (PAGE, EvtAddDevice)
#pragma alloc_text (PAGE, IniciarContext)
#pragma alloc_text (PAGE, IniciarContextX52)
#pragma alloc_text (PAGE, IniciarContextPedales)
#pragma alloc_text (PAGE, IniciarContextHID)
#pragma alloc_text (PAGE, EvtDevicePrepareHardware)
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
		pnpPowerCallbacks.EvtDevicePrepareHardware = EvtDevicePrepareHardware;
		pnpPowerCallbacks.EvtDeviceSelfManagedIoInit = EvtDeviceSelfManagedIoInit;
		pnpPowerCallbacks.EvtDeviceSelfManagedIoCleanup = EvtDeviceSelfManagedIoCleanup;
		pnpPowerCallbacks.EvtDeviceD0Exit = EvtDeviceD0Exit;
		pnpPowerCallbacks.EvtDeviceD0Entry = EvtDeviceD0Entry;
	WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_CONTEXT);
		attributes.EvtCleanupCallback = EvtCleanupCallback;
	status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status))
		return status;

	RtlZeroMemory(GetDeviceContext(device), sizeof(DEVICE_CONTEXT));

	status = IniciarContext(device);
	if (!NT_SUCCESS(status))
		return status;

	LeerConfiguracion(device);

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.SynchronizationScope = WdfSynchronizationScopeQueue;
	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchParallel);
		ioQConfig.EvtIoInternalDeviceControl = EvtX52InternalIOCtl;
	status = WdfIoQueueCreate(device, &ioQConfig, &attributes, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status))
		return status;

	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	WDF_IO_QUEUE_CONFIG_INIT(&ioQConfig, WdfIoQueueDispatchSequential);
		ioQConfig.EvtIoDefault = EvtRequestHID;
	status = WdfIoQueueCreate(device, &ioQConfig, &attributes, &cola);
	if (!NT_SUCCESS(status))
		return status;
	GetDeviceContext(device)->EntradaX52.ColaRequest = cola;

	WDF_IO_QUEUE_CONFIG_INIT(&ioQConfig, WdfIoQueueDispatchManual);
	status = WdfIoQueueCreate(device, &ioQConfig, &attributes, &cola);
	if (!NT_SUCCESS(status))
		return status;
	GetDeviceContext(device)->HID.ColaRequestSinUsar = cola;

	status = IniciarIoCtlAplicacion(device);

	return status;
}

NTSTATUS IniciarContext(WDFDEVICE device)
{
	WDF_OBJECT_ATTRIBUTES	attributes;
	NTSTATUS				status;
	WDF_TIMER_CONFIG		timerConfig;

	PAGED_CODE();

	status = IniciarContextX52(device);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarContextPedales(device);
	if (!NT_SUCCESS(status))
		return status;

	status = IniciarContextHID(device);
	if (!NT_SUCCESS(status))
		return status;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->Calibrado.WaitLockCalibrado);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->Perfil.WaitLockMapas);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->Perfil.WaitLockAcciones);
	if (!NT_SUCCESS(status)) return status;
	status = WdfCollectionCreate(&attributes, &GetDeviceContext(device)->Perfil.Acciones);
	if (!NT_SUCCESS(status)) return status;

	WDF_TIMER_CONFIG_INIT(&timerConfig, EvtTickMenu);
	timerConfig.TolerableDelay = TolerableDelayUnlimited;
	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	status = WdfTimerCreate(&timerConfig, &attributes, &GetDeviceContext(device)->MenuMFD.Timer);
	if (!NT_SUCCESS(status))
		return status;

	WDF_TIMER_CONFIG_INIT(&timerConfig, EvtTickHora);
	status = WdfTimerCreate(&timerConfig, &attributes, &GetDeviceContext(device)->MenuMFD.TimerHora);
	GetDeviceContext(device)->MenuMFD.HoraActivada = TRUE;
	GetDeviceContext(device)->MenuMFD.FechaActivada = TRUE;

	return status;
}

NTSTATUS IniciarContextX52(_In_ WDFDEVICE device)
{
	NTSTATUS status;
	WDF_OBJECT_ATTRIBUTES	attributes;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->USBaHID.WaitLockProcesar);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado);
	if (!NT_SUCCESS(status)) return status;

	status = WdfCollectionCreate(&attributes, &GetDeviceContext(device)->SalidaX52.Ordenes);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->SalidaX52.WaitLockOrdenes);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->SalidaX52.WaitLockX52);

	return status;
}

NTSTATUS IniciarContextPedales(_In_ WDFDEVICE device)
{
	NTSTATUS status;
	WDF_OBJECT_ATTRIBUTES	attributes;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->Pedales.WaitLockPosicion);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget);

	GetDeviceContext(device)->Pedales.Activado = TRUE;
	GetDeviceContext(device)->Pedales.UltimaPosicionRz = 255;

	return status;
}

NTSTATUS IniciarContextHID(_In_ WDFDEVICE device)
{
	NTSTATUS				status;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_TIMER_CONFIG		timerConfig;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	status = WdfCollectionCreate(&attributes, &GetDeviceContext(device)->HID.ColaEventos);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->HID.WaitLockRequest);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->HID.WaitLockEventos);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(&attributes, &GetDeviceContext(device)->HID.Estado.WaitLockEstado);
	if (!NT_SUCCESS(status)) return status;

	attributes.ExecutionLevel = WdfExecutionLevelPassive;
	WDF_TIMER_CONFIG_INIT(&timerConfig, EvtTickRaton);
	timerConfig.AutomaticSerialization = TRUE;
	timerConfig.TolerableDelay = 0;
	timerConfig.UseHighResolutionTimer = WdfTrue;
	status = WdfTimerCreate(&timerConfig, &attributes, &GetDeviceContext(device)->HID.RatonTimer);
	if (!NT_SUCCESS(status))
		return status;

	status = WdfCollectionCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->HID.ListaTimersDelay);

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
VOID EvtCleanupCallback(WDFOBJECT  Object)
{
	WDFDEVICE device = (WDFDEVICE)Object;

	PAGED_CODE();

	GetDeviceContext(device)->USBaHID.ModoRaw = TRUE;
	CerrarIoCtlAplicacion(device);
	CerrarPedales(device);

	LimpiarEventos(device);
	LimpiarPerfil(device);
	LimpiarEventos(device);
	LimpiarSalidaX52(device);
}