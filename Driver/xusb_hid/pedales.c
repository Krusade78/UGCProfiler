#define _PRIVATE_
#include "pedales.h"
#undef _PRIVATE
#include "extensions.h"

#include <wdm.h>
#include <hidclass.h>
#include <wdmguid.h>
#include <Ntstrsafe.h>

#define HARDWARE_ID_PEDALES  L"\\??\\HID#Vid_044f&Pid_b653"

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, IniciarNotificacionPnP)
#pragma alloc_text(PAGE, PnPCallback)
#pragma alloc_text(PAGE, IniciarIoTarget)
#endif

//PASSIVE_LEVEL
NTSTATUS IniciarNotificacionPnP(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	return IoRegisterPlugPlayNotification(
										EventCategoryDeviceInterfaceChange,
										PNPNOTIFY_DEVICE_INTERFACE_INCLUDE_EXISTING_INTERFACES,
										(LPGUID)&GUID_DEVINTERFACE_HID,
										WdfDriverWdmGetDriverObject(WdfDeviceGetDriver(device)),
										PnPCallback,
										GetDeviceExtension(device),
										&GetDeviceExtension(device)->PnPNotifyHandle);
}

//PASSIVE_LEVEL
NTSTATUS PnPCallback(_In_ PVOID notification, _Inout_opt_ PVOID context)
{
	PDEVICE_INTERFACE_CHANGE_NOTIFICATION diNotify = (PDEVICE_INTERFACE_CHANGE_NOTIFICATION)notification;

	PAGED_CODE();

	if (IsEqualGUID(diNotify->Event, GUID_DEVICE_INTERFACE_ARRIVAL))
	{
		UNICODE_STRING strId;
		RtlUnicodeStringInit(&strId, HARDWARE_ID_PEDALES);
		if (RtlCompareUnicodeString(diNotify->SymbolicLinkName, &strId, TRUE) == 0)
		{
			return IniciarIoTarget(diNotify->SymbolicLinkName, ((PDEVICE_EXTENSION)context)->Self);
		}
	}

	return STATUS_SUCCESS;
}

//PASSIVE_LEVEL
NTSTATUS IniciarIoTarget(_In_ PUNICODE_STRING strId, _In_ WDFDEVICE device)
{
	NTSTATUS status;
	WDF_OBJECT_ATTRIBUTES attributes;
	WDFIOTARGET IoTarget;
	WDF_IO_TARGET_OPEN_PARAMS  openParams;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	status = WdfIoTargetCreate(device, &attributes, &IoTarget);
	if (NT_SUCCESS(status))
	{
		WDF_IO_TARGET_OPEN_PARAMS_INIT_OPEN_BY_NAME(&openParams, strId, STANDARD_RIGHTS_READ);
		openParams.ShareAccess = FILE_SHARE_READ;
		openParams.EvtIoTargetRemoveComplete = EvIoTargetRemove;

		status = WdfIoTargetOpen(IoTarget, &openParams);
		if (NT_SUCCESS(status) && ((openParams.FileInformation == FILE_SUPERSEDED) || (openParams.FileInformation == FILE_OPENED)))
		{
			if (WdfIoTargetGetState(IoTarget) == WdfIoTargetStarted)
			{
				PDPCTARGETS_CONTEXT ctx = WdfObjectGet_DPCTARGETS_CONTEXT(GetDeviceExtension(device)->DpcTargets[0]);
				ctx->target = IoTarget;
				ctx->tipo = 1;
				if (!WdfDpcEnqueue(GetDeviceExtension(Device)->DpcTargets[0]))
					WdfObjectDelete(IoTarget);
			}
			else
			{
				WdfObjectDelete(IoTarget);
			}

		}
		else
		{
			WdfObjectDelete(IoTarget);
		}
	}
}