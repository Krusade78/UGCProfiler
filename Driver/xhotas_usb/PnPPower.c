#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include <hidclass.h>
#include "Context.h"
#define _PUBLIC_
#include "PnPPedales.h"
#include "EventosProcesar.h"
#include "EscribirUSBX52.h"
#define _PRIVATE_
#include "PnPPower.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoInit)
#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoCleanup)
#pragma alloc_text (PAGE, EvtDeviceD0Entry)
#pragma alloc_text (PAGE, EvtDeviceD0Exit)
#endif

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
NTSTATUS EvtDeviceSelfManagedIoInit(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	return IoRegisterPlugPlayNotification(
		EventCategoryDeviceInterfaceChange,
		PNPNOTIFY_DEVICE_INTERFACE_INCLUDE_EXISTING_INTERFACES,
		(PGUID)&GUID_DEVINTERFACE_HID,
		WdfDriverWdmGetDriverObject(WdfDeviceGetDriver(device)),
		PnPCallbackPedales,
	    device,
		&GetDeviceContext(device)->PedalesPnP.PnPNotifyHandle);
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
VOID EvtDeviceSelfManagedIoCleanup(_In_ WDFDEVICE device)
{
	PAGED_CODE();
	CerrarPedales(device);
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
NTSTATUS EvtDeviceD0Entry(
	_In_  WDFDEVICE device,
	_In_  WDF_POWER_DEVICE_STATE TargetState
)
{
	UNREFERENCED_PARAMETER(TargetState);

	PAGED_CODE();

	if (!GetDeviceContext(device)->MenuMFD.Activado)
	{
		Luz_Global(device, &GetDeviceContext(device)->MenuMFD.LuzGlobal);
		Luz_MFD(device, &GetDeviceContext(device)->MenuMFD.LuzMFD);
	}
	WdfTimerStart(GetDeviceContext(device)->MenuMFD.TimerHora, WDF_REL_TIMEOUT_IN_MS(4000));

	return STATUS_SUCCESS;
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
NTSTATUS EvtDeviceD0Exit(
	_In_  WDFDEVICE device,
	_In_  WDF_POWER_DEVICE_STATE TargetState
)
{
	UNREFERENCED_PARAMETER(TargetState);

	PAGED_CODE();

	LimpiarEventos(device);

	//GetDeviceContext(device)->HID.RatonActivado = FALSE;
	WdfTimerStop(GetDeviceContext(device)->HID.RatonTimer, TRUE);
	WdfTimerStop(GetDeviceContext(device)->MenuMFD.TimerHora, TRUE);

	return STATUS_SUCCESS;
}


