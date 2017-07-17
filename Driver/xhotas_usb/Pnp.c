#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include <hidclass.h>
#include "Context.h"
#include "Pedales_read.h"
#include "acciones.h"
#include "PnP.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoInit)
#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoCleanup)
#pragma alloc_text (PAGE, EvtDeviceD0Entry)
#pragma alloc_text (PAGE, EvtDeviceD0Exit)
#endif

//PASSIVE_LEVEL
NTSTATUS EvtDeviceSelfManagedIoInit(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	return IoRegisterPlugPlayNotification(
		EventCategoryDeviceInterfaceChange,
		PNPNOTIFY_DEVICE_INTERFACE_INCLUDE_EXISTING_INTERFACES,
		(PGUID)&GUID_DEVINTERFACE_HID,
		WdfDriverWdmGetDriverObject(WdfDeviceGetDriver(device)),
		PnPCallbackPedales,
	    GetDeviceContext(device),
		&GetDeviceContext(device)->Pedales.PnPNotifyHandle);
}

//PASSIVE_LEVEL
VOID EvtDeviceSelfManagedIoCleanup(_In_ WDFDEVICE device)
{
	PAGED_CODE();
	if (GetDeviceContext(device)->Pedales.PnPNotifyHandle != NULL)
	{
		PVOID p = GetDeviceContext(device)->Pedales.PnPNotifyHandle;
		GetDeviceContext(device)->Pedales.PnPNotifyHandle = NULL;
		IoUnregisterPlugPlayNotification(p);
	}
}

//PASSIVE_LEVEL
NTSTATUS EvtDeviceD0Entry(
	_In_ WDFDEVICE device,
	_In_ WDF_POWER_DEVICE_STATE PreviousState
)
{
	UNREFERENCED_PARAMETER(device);
	UNREFERENCED_PARAMETER(PreviousState);

	PAGED_CODE();

	//GetDeviceExtension(Device)->D0Apagado = FALSE;
	return STATUS_SUCCESS;
}

//PASSIVE_LEVEL
NTSTATUS EvtDeviceD0Exit(
	_In_  WDFDEVICE device,
	_In_  WDF_POWER_DEVICE_STATE TargetState
)
{
	UNREFERENCED_PARAMETER(device);
	UNREFERENCED_PARAMETER(TargetState);

	PAGED_CODE();

	//WdfWaitLockAcquire(GetDeviceExtension(Device)->WaitLockCierre, NULL);
	//GetDeviceExtension(Device)->D0Apagado = TRUE;
	//WdfWaitLockRelease(GetDeviceExtension(Device)->WaitLockCierre);
	LimpiarAcciones(device);

	RtlZeroMemory(GetDeviceContext(device)->HID.stTeclado, sizeof(GetDeviceContext(device)->HID.stTeclado));
	RtlZeroMemory(GetDeviceContext(device)->HID.stRaton, sizeof(GetDeviceContext(device)->HID.stRaton));
	RtlZeroMemory(GetDeviceContext(device)->HID.stBotones, sizeof(GetDeviceContext(device)->HID.stBotones));
	RtlZeroMemory(GetDeviceContext(device)->HID.stSetas, sizeof(GetDeviceContext(device)->HID.stSetas));

	GetDeviceContext(device)->HID.RatonActivado = FALSE;
	WdfTimerStop(GetDeviceContext(device)->HID.RatonTimer, TRUE);

	return STATUS_SUCCESS;
}
