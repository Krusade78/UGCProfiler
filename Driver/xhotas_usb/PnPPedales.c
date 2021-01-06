/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

pedales_read.c

Abstract:

Archivo para el control de los pedales de Thrusmaster.

--*/

#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include <hidclass.h>
#include <Wdmguid.h>
#include <ntstrsafe.h>
#include "context.h"
#define _PUBLIC_
#include "LeerUSBPedales.h"
#define _PRIVATE_
#include "PnPPedales.h"
#undef _PRIVATE_
#undef _PUBLIC_

#define HARDWARE_ID_PEDALES  L"\\??\\HID#VID_06A3&PID_0763"

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, CerrarPedales)
#pragma alloc_text (PAGE, PnPCallbackPedales)
#pragma alloc_text (PAGE, IniciarIoTarget)
#pragma alloc_text (PAGE, CerrarIoTargetPassive)
#pragma alloc_text (PAGE, CerrarIoTargetWI)
#pragma alloc_text (PAGE, EvIoTargetRemoveComplete)
#pragma alloc_text (PAGE, EvIoTargetRemoveCanceled)
#endif

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
VOID CerrarPedales(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	if (GetDeviceContext(device)->PedalesPnP.PnPNotifyHandle != NULL)
	{
		PVOID p = GetDeviceContext(device)->PedalesPnP.PnPNotifyHandle;
		GetDeviceContext(device)->PedalesPnP.PnPNotifyHandle = NULL;
		IoUnregisterPlugPlayNotification(p);
	}
	if (GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget != NULL)
	{
		CerrarIoTargetPassive(device);
	}
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="context">-> Device</param>
NTSTATUS PnPCallbackPedales(_In_ PVOID notification, _Inout_opt_ PVOID context)
{
	DECLARE_CONST_UNICODE_STRING(strId, HARDWARE_ID_PEDALES);

	NTSTATUS status = STATUS_SUCCESS;
	PDEVICE_INTERFACE_CHANGE_NOTIFICATION diNotify = (PDEVICE_INTERFACE_CHANGE_NOTIFICATION)notification;

	PAGED_CODE();

	if ((context != NULL) && IsEqualGUID((LPGUID)&diNotify->Event, (LPGUID)&GUID_DEVICE_INTERFACE_ARRIVAL))
	{
		if (diNotify->SymbolicLinkName->Length >= strId.Length)
		{
			UNICODE_STRING strLink;
			PWSTR wstrLink[sizeof(HARDWARE_ID_PEDALES)];
			RtlZeroMemory(wstrLink, sizeof(HARDWARE_ID_PEDALES));
			RtlCopyMemory(wstrLink, diNotify->SymbolicLinkName->Buffer, sizeof(HARDWARE_ID_PEDALES) - sizeof(WCHAR));

			status = RtlUnicodeStringInit(&strLink, (NTSTRSAFE_PCWSTR)&wstrLink);
			if (!NT_SUCCESS(status))
				return status;
			else
			{
				if (RtlCompareUnicodeString(&strLink, &strId, TRUE) == 0)
				{
					size_t tam = (diNotify->SymbolicLinkName->MaximumLength > 200) ? 199 : diNotify->SymbolicLinkName->MaximumLength;

					RtlZeroMemory(GetDeviceContext((WDFDEVICE)context)->PedalesPnP.SymbolicLink, 200 * sizeof(WCHAR));
					RtlCopyMemory(GetDeviceContext((WDFDEVICE)context)->PedalesPnP.SymbolicLink, diNotify->SymbolicLinkName->Buffer, tam * sizeof(WCHAR));

					status = IniciarIoTarget((WDFDEVICE)context);
				}
			}
		}
	}

	return status;
}

#pragma region "Iniciar IO target"
/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
NTSTATUS IniciarIoTarget(_In_ WDFDEVICE device)
{
	NTSTATUS status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES attributes;
	WDFIOTARGET ioTarget;
	WDF_IO_TARGET_OPEN_PARAMS  openParams;

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget, NULL);
	{
		if (GetDeviceContext(device)->PedalesPnP.IoTarget == NULL)
		{
			WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = device;

			status = WdfIoTargetCreate(device, &attributes, &ioTarget);
			if (NT_SUCCESS(status))
			{
				UNICODE_STRING strId;
				strId.Length = (USHORT)(wcslen(GetDeviceContext(device)->PedalesPnP.SymbolicLink) * sizeof(WCHAR));
				strId.MaximumLength = strId.Length + sizeof(WCHAR);
				strId.Buffer = GetDeviceContext(device)->PedalesPnP.SymbolicLink;
				WDF_IO_TARGET_OPEN_PARAMS_INIT_OPEN_BY_NAME(&openParams, &strId, STANDARD_RIGHTS_READ);
				openParams.ShareAccess = FILE_SHARE_READ | FILE_SHARE_WRITE;
				openParams.EvtIoTargetRemoveComplete = EvIoTargetRemoveComplete;
				openParams.EvtIoTargetRemoveCanceled = EvIoTargetRemoveCanceled;

				GetDeviceContext(device)->PedalesPnP.IoTarget = ioTarget;

				status = WdfIoTargetOpen(ioTarget, &openParams);
				if (NT_SUCCESS(status))
				{
					if (WdfIoTargetGetState(ioTarget) == WdfIoTargetStarted)
					{
						status = IniciarReportsUSBPedales(ioTarget);
						if (!NT_SUCCESS(status))
						{
							WdfWaitLockRelease(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget);
							CerrarIoTargetPassive(device);
							return status;
						}
					}
					else
					{
						WdfWaitLockRelease(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget);
						CerrarIoTargetPassive(device);
						return STATUS_UNSUCCESSFUL;
					}

				}
				else
				{
					WdfWaitLockRelease(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget);
					CerrarIoTargetPassive(device);
					return status;
				}
			}
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget);

	return status;
}
#pragma endregion

#pragma region "Cerrar IO Target"
/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="device"></param>
VOID CerrarIoTargetPassive(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	WDFIOTARGET ioTarget = NULL;

	WdfWaitLockAcquire(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget, NULL);
	{
		ioTarget = GetDeviceContext(device)->PedalesPnP.IoTarget;
		GetDeviceContext(device)->PedalesPnP.IoTarget = NULL;
	}
	WdfWaitLockRelease(GetDeviceContext(device)->PedalesPnP.WaitLockIoTarget);

	if (ioTarget != NULL)
	{
		WdfIoTargetClose(ioTarget);
		WdfObjectDelete(ioTarget);
	}
}

/// <summary>
/// <=DISPATCH_LEVEL
/// </summary>
VOID CerrarIoTarget(_In_ WDFDEVICE device)
{
	NTSTATUS				status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_WORKITEM_CONFIG		workitemConfig;
	WDFWORKITEM				workItem;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	WDF_WORKITEM_CONFIG_INIT(&workitemConfig, CerrarIoTargetWI);
	status = WdfWorkItemCreate(&workitemConfig, &attributes, &workItem);
	if (NT_SUCCESS(status))
		WdfWorkItemEnqueue(workItem);

	//return status;
}
/// <summary>
/// PASSIVE_LEVEL
/// </summary>
VOID CerrarIoTargetWI(_In_ WDFWORKITEM workItem)
{
	PAGED_CODE();

	CerrarIoTargetPassive((WDFDEVICE)WdfWorkItemGetParentObject(workItem));
	WdfObjectDelete(workItem);
}
#pragma endregion

#pragma region "PnP Callbacks IoTarget"
/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="ioTarget"></param>
VOID EvIoTargetRemoveComplete(_In_ WDFIOTARGET ioTarget)
{
	PAGED_CODE();
	CerrarIoTargetPassive(WdfIoTargetGetDevice(ioTarget));
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
/// <param name="ioTarget"></param>
VOID EvIoTargetRemoveCanceled(_In_ WDFIOTARGET ioTarget)
{
	PAGED_CODE();

	WDF_IO_TARGET_OPEN_PARAMS openParams;
	NTSTATUS status;

	WDF_IO_TARGET_OPEN_PARAMS_INIT_REOPEN(&openParams);
	status = WdfIoTargetOpen(ioTarget, &openParams);
	if (NT_SUCCESS(status))
	{
		if (WdfIoTargetGetState(ioTarget) == WdfIoTargetStarted)
		{
			status = IniciarReportsUSBPedales(ioTarget);
			if (!NT_SUCCESS(status))
				CerrarIoTargetPassive(WdfIoTargetGetDevice(ioTarget));
		}
		else
			CerrarIoTargetPassive(WdfIoTargetGetDevice(ioTarget));
	}
	else
		CerrarIoTargetPassive(WdfIoTargetGetDevice(ioTarget));
}
#pragma endregion