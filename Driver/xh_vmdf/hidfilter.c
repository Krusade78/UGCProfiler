/*++

Copyright (c) 2005-2007 Alfredo Costalago
Module Name:

    hidfilter.c

Abstract: Filtro para - Human Interface Device (HID) Gameport driver

Environment:

    Kernel mode

--*/

#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "control.h"
#include "ioctl.h"
#include "IoTargets.h"
#include "reports.h"
#include "mapa.h"
#include <initguid.h>


#define _HIDFILTER_
#include "hidfilter.h"
#undef _HIDFILTER_

DECLARE_CONST_UNICODE_STRING(MyDeviceName, L"\\Device\\XH_VHidF") ;
DECLARE_CONST_UNICODE_STRING(dosDeviceName, L"\\??\\XHOTASHidInterface");

#ifdef ALLOC_PRAGMA
    #pragma alloc_text( INIT, DriverEntry )
    #pragma alloc_text( PAGE, HF_AddDevice)
    #pragma alloc_text( PAGE, IniciarExtensiones)
    #pragma alloc_text( PAGE, IniciarInterfazControl)
	#pragma alloc_text( PAGE, HF_EvtDeviceD0Exit)
	#pragma alloc_text( PAGE, HF_EvtReleaseHardware)
#endif /* ALLOC_PRAGMA */


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
	WDF_PNPPOWER_EVENT_CALLBACKS	pnpPowerCallbacks;
	WDF_IO_QUEUE_CONFIG				ioQConfig;

	PAGED_CODE();

	WdfFdoInitSetFilter(DeviceInit);

	WDF_PNPPOWER_EVENT_CALLBACKS_INIT(&pnpPowerCallbacks);
		pnpPowerCallbacks.EvtDeviceD0Entry			= HF_EvtDeviceD0Entry;
		pnpPowerCallbacks.EvtDeviceD0Exit			= HF_EvtDeviceD0Exit;
		pnpPowerCallbacks.EvtDeviceReleaseHardware	= HF_EvtReleaseHardware;
    WdfDeviceInitSetPnpPowerEventCallbacks(DeviceInit, &pnpPowerCallbacks);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, DEVICE_EXTENSION);
		attributes.SynchronizationScope = WdfSynchronizationScopeDevice;
		attributes.EvtCleanupCallback = EvtCleanupCallback;
    status = WdfDeviceCreate(&DeviceInit, &attributes, &device);
	if (!NT_SUCCESS(status)) return status;

	WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&ioQConfig, WdfIoQueueDispatchSequential);
		ioQConfig.EvtIoDefault					= HF_DefaultIoDeviceControl;
		ioQConfig.EvtIoInternalDeviceControl	= HF_InternIoCtl;
	status = WdfIoQueueCreate(device, &ioQConfig, WDF_NO_OBJECT_ATTRIBUTES, WDF_NO_HANDLE);
	if (!NT_SUCCESS(status)) return status;

	status = IniciarExtensiones(device);
	if (!NT_SUCCESS(status)) return status;

	return IniciarInterfazControl(Driver, GetDeviceExtension(device));
}

NTSTATUS IniciarExtensiones(IN WDFDEVICE device)
{
	NTSTATUS                status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_WORKITEM_CONFIG		workitemConfig;
	WDF_TIMER_CONFIG		timerConfig;
	WDF_DPC_CONFIG			dpcConfig;
	PDEVICE_EXTENSION		devExt = GetDeviceExtension(device);

	PAGED_CODE();

	RtlZeroMemory(devExt, sizeof(DEVICE_EXTENSION));

	status = WdfWaitLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->WaitLockCierre);
	if (!NT_SUCCESS(status)) return status;

	//status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->SpinLockTargets);
	//if (!NT_SUCCESS(status)) return status;

	WDF_TIMER_CONFIG_INIT(&timerConfig, TimerTickRaton);
		timerConfig.AutomaticSerialization = TRUE;
    WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
    status = WdfTimerCreate(&timerConfig, &attributes, &devExt->TimerRaton);
	if (!NT_SUCCESS(status)) return status;

	WDF_TIMER_CONFIG_INIT_PERIODIC(&timerConfig, TimerTickTargets, 5000);
		timerConfig.AutomaticSerialization = TRUE;
    WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
    status = WdfTimerCreate(&timerConfig, &attributes, &devExt->TimerIoTargets);
	if (!NT_SUCCESS(status)) return status;

	WDF_WORKITEM_CONFIG_INIT(&workitemConfig, IniciarIoTargets);
		workitemConfig.AutomaticSerialization = FALSE;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
	WDF_OBJECT_ATTRIBUTES_SET_CONTEXT_TYPE(&attributes, WI_CONTEXT);
	status = WdfWorkItemCreate(&workitemConfig, &attributes, &devExt->WorkItemTargets);
	if (!NT_SUCCESS(status))

	devExt->SpinLockDeltaHid = 0; //La creacion del workitem bloquea la siguiente asignacion, o sea, esta
	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->SpinLockDeltaHid);
	if (!NT_SUCCESS(status)) return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->SpinLockAcciones);
	if (!NT_SUCCESS(status)) return status;

	WDF_DPC_CONFIG_INIT(&dpcConfig, EvDpcAbrirCerrarTargets);
		dpcConfig.AutomaticSerialization = TRUE;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
	WDF_OBJECT_ATTRIBUTES_SET_CONTEXT_TYPE(&attributes, DPCTARGETS_CONTEXT);
	status = WdfDpcCreate(&dpcConfig, &attributes, &devExt->DpcTargets[0]);
	if (!NT_SUCCESS(status)) return status;
	status = WdfDpcCreate(&dpcConfig, &attributes, &devExt->DpcTargets[1]);
	if (!NT_SUCCESS(status)) return status;
	status = WdfDpcCreate(&dpcConfig, &attributes, &devExt->DpcTargets[2]);
	if (!NT_SUCCESS(status)) return status;
	status = WdfDpcCreate(&dpcConfig, &attributes, &devExt->DpcTargets[3]);
	if (!NT_SUCCESS(status)) return status;
	status = WdfDpcCreate(&dpcConfig, &attributes, &devExt->DpcTargets[4]);
	if (!NT_SUCCESS(status)) return status;

	WDF_DPC_CONFIG_INIT(&dpcConfig, EvDpc);
		dpcConfig.AutomaticSerialization = TRUE;
	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
	status = WdfDpcCreate(&dpcConfig, &attributes, &devExt->DpcRequest);
	if (!NT_SUCCESS(status)) return status;

	devExt->stModos = devExt->stAux = 0xff;
	devExt->TickRaton = 70;

	//ITF
	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->itfExt.slCalibrado);
	if (!NT_SUCCESS(status)) return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->itfExt.slMapas);
	if (!NT_SUCCESS(status)) return status;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &devExt->itfExt.slComandos);
	if (!NT_SUCCESS(status)) return status;

	devExt->PedalesActivados = TRUE;

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
	if(devInit == NULL)
		return STATUS_INSUFFICIENT_RESOURCES;

	status = WdfDeviceInitAssignName(devInit, &MyDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	WdfDeviceInitSetExclusive(devInit, TRUE);
	WdfDeviceInitSetIoType(devInit, WdfDeviceIoBuffered);

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, CONTROL_EXTENSION);
	status = WdfDeviceCreate(&devInit, &attributes, &devExt->ControlDevice);
	if (!NT_SUCCESS(status))
	{
		WdfDeviceInitFree(devInit);
		return status;
	}

	status = WdfDeviceCreateSymbolicLink(devExt->ControlDevice, &dosDeviceName);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(devExt->ControlDevice);
		devExt->ControlDevice = NULL;
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

	GetControlExtension(devExt->ControlDevice)->devExt = devExt;

	WdfControlFinishInitializing(devExt->ControlDevice);

	return STATUS_SUCCESS;
}


VOID HF_DefaultIoDeviceControl(
  __in  WDFQUEUE Queue,
  __in  WDFREQUEST Request
)
{
	UNREFERENCED_PARAMETER(Queue);
	WdfRequestComplete(Request, STATUS_NOT_IMPLEMENTED);
}

NTSTATUS
  HF_EvtDeviceD0Entry(
    IN WDFDEVICE  Device,
    IN WDF_POWER_DEVICE_STATE  PreviousState
    )
{
	UNREFERENCED_PARAMETER(PreviousState);

	//WdfTimerStart(GetDeviceExtension(Device)->TimerRaton, WDF_REL_TIMEOUT_IN_MS(GetDeviceExtension(Device)->TickRaton));
	GetDeviceExtension(Device)->D0Apagado = FALSE;
	WdfTimerStart(GetDeviceExtension(Device)->TimerIoTargets, WDF_REL_TIMEOUT_IN_MS(5000));
	return STATUS_SUCCESS;
}

NTSTATUS
  HF_EvtDeviceD0Exit(
    IN WDFDEVICE  Device,
    IN WDF_POWER_DEVICE_STATE  PreviousState
    )
{
	UNREFERENCED_PARAMETER(PreviousState);

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceExtension(Device)->WaitLockCierre, NULL);
	GetDeviceExtension(Device)->D0Apagado = TRUE;
	WdfWaitLockRelease(GetDeviceExtension(Device)->WaitLockCierre);
	CerrarIoTargets(Device);

	WdfSpinLockAcquire(GetDeviceExtension(Device)->SpinLockAcciones);
		if(!ColaEstaVacia(&GetDeviceExtension(Device)->ColaAccionesHOTAS))
		{
			PNODO siguiente = GetDeviceExtension(Device)->ColaAccionesHOTAS.principio;
			while(siguiente != NULL)
			{
				ColaBorrar((PCOLA)siguiente->Datos); siguiente->Datos = NULL;
				siguiente = siguiente->link;
				ColaBorrarNodo(&GetDeviceExtension(Device)->ColaAccionesHOTAS, GetDeviceExtension(Device)->ColaAccionesHOTAS.principio);
			}
		}
		if(!ColaEstaVacia(&GetDeviceExtension(Device)->ColaAccionesRaton))
		{
			PNODO siguiente = GetDeviceExtension(Device)->ColaAccionesRaton.principio;
			while(siguiente != NULL)
			{
				ColaBorrar((PCOLA)siguiente->Datos); siguiente->Datos = NULL;
				siguiente = siguiente->link;
				ColaBorrarNodo(&GetDeviceExtension(Device)->ColaAccionesRaton, GetDeviceExtension(Device)->ColaAccionesRaton.principio);
			}
		}
		if(!ColaEstaVacia(&GetDeviceExtension(Device)->ColaAccionesComando))
		{
			PNODO siguiente = GetDeviceExtension(Device)->ColaAccionesComando.principio;
			while(siguiente != NULL)
			{
				ColaBorrar((PCOLA)siguiente->Datos); siguiente->Datos = NULL;
				siguiente = siguiente->link;
				ColaBorrarNodo(&GetDeviceExtension(Device)->ColaAccionesComando, GetDeviceExtension(Device)->ColaAccionesComando.principio);
			}
		}
	WdfSpinLockRelease(GetDeviceExtension(Device)->SpinLockAcciones);

	RtlZeroMemory(GetDeviceExtension(Device)->stTeclado, sizeof(GetDeviceExtension(Device)->stTeclado));
	RtlZeroMemory(GetDeviceExtension(Device)->stRaton, sizeof(GetDeviceExtension(Device)->stRaton));
	RtlZeroMemory(GetDeviceExtension(Device)->stHOTAS.Botones, sizeof(GetDeviceExtension(Device)->stHOTAS.Botones));
	RtlZeroMemory(GetDeviceExtension(Device)->stHOTAS.Setas, sizeof(GetDeviceExtension(Device)->stHOTAS.Setas));

	GetDeviceExtension(Device)->TimerRatonOn = FALSE;
	WdfTimerStop(GetDeviceExtension(Device)->TimerRaton, TRUE);

	return STATUS_SUCCESS;
}

NTSTATUS
  HF_EvtReleaseHardware (
    IN WDFDEVICE  Device,
    IN WDFCMRESLIST  ResourcesTranslated
    )
{
	UNREFERENCED_PARAMETER(ResourcesTranslated);

	PAGED_CODE();

	LimpiarMemoria(GetDeviceExtension(Device));

	return STATUS_SUCCESS;
}

VOID
  EvtCleanupCallback (
    IN WDFOBJECT  Object
    )
{
	if(GetDeviceExtension(Object)->ControlDevice != NULL)
		WdfObjectDelete(GetDeviceExtension(Object)->ControlDevice);
}