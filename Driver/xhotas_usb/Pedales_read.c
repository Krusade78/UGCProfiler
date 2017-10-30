/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

pedales_read.c

Abstract:

Archivo para el control de los pedales de Thrusmaster.

Environment:

Kernel-mode Driver Framework 2

--*/

#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include <hidclass.h>
#include <Wdmguid.h>
#include <ntstrsafe.h>
#include "context.h"
#include "x52_read.h"
#define _PRIVATE_
#include "pedales_read.h"
#undef _PRIVATE_

#define HARDWARE_ID_PEDALES  L"\\??\\HID#Vid_044f&Pid_b653"
#define TAM_REPORTPEDALES 8

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, CerrarPedales)
#pragma alloc_text (PAGE, CerrarIoTargetPassive)
#pragma alloc_text (PAGE, CerrarIoTargetWI)
#pragma alloc_text (PAGE, IniciarPedales)
#pragma alloc_text (PAGE, IniciarIoTargetPassive)
#pragma alloc_text (PAGE, IniciarIoTargetWI)
#pragma alloc_text (PAGE, PnPCallbackPedales)
#endif

#pragma region "Cerrar"
//PASSIVE_LEVEL
VOID CerrarPedales(_In_ WDFDEVICE device)
{
	if (GetDeviceContext(device)->Pedales.PnPNotifyHandle != NULL)
	{
		PVOID p = GetDeviceContext(device)->Pedales.PnPNotifyHandle;
		GetDeviceContext(device)->Pedales.PnPNotifyHandle = NULL;
		IoUnregisterPlugPlayNotification(p);
	}
	if (GetDeviceContext(device)->Pedales.WaitLockIoTarget != NULL)
	{
		CerrarIoTargetPassive(device);
		GetDeviceContext(device)->Pedales.WaitLockIoTarget = NULL;
	}
}

#pragma region "Cerrar IO Target"
//PASSIVE_LEVEL
VOID CerrarIoTargetPassive(_In_ WDFDEVICE device)
{
	WDFIOTARGET ioTarget = NULL;

	WdfWaitLockAcquire(GetDeviceContext(device)->Pedales.WaitLockIoTarget, NULL);
	{
		ioTarget = GetDeviceContext(device)->Pedales.IoTarget;
		GetDeviceContext(device)->Pedales.IoTarget = NULL;
	}
	WdfWaitLockRelease(GetDeviceContext(device)->Pedales.WaitLockIoTarget);
	if (ioTarget != NULL)
	{
		WdfIoTargetClose(ioTarget);
		WdfObjectDelete(ioTarget);
	}
}

//PASSIVE_LEVEL
VOID CerrarIoTargetWI(_In_ WDFWORKITEM workItem)
{
	CerrarIoTargetPassive((WDFDEVICE)WdfWorkItemGetParentObject(workItem));
	WdfObjectDelete(workItem);
}

// <=DISPATCH_LEVEL
NTSTATUS CerrarIoTarget(_In_ WDFDEVICE device)
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

	return status;
}
#pragma endregion
#pragma endregion

#pragma region "Iniciar"
//PASSIVE_LEVEL
NTSTATUS IniciarPedales(_In_ WDFDEVICE device)
{
	NTSTATUS status;

	PAGED_CODE();

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Pedales.SpinLockPosicion);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Pedales.WaitLockIoTarget);
	if (!NT_SUCCESS(status)) return status;

	GetDeviceContext(device)->Pedales.IoTarget = NULL;
	GetDeviceContext(device)->Pedales.UltimaPosicion = 511;

	return STATUS_SUCCESS;
}

//PASSIVE_LEVEL
//El writeBufferMemHandle se borra al borrar newRequest
NTSTATUS IniciarReports(WDFIOTARGET ioTarget)
{
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFREQUEST				newRequest;
	WDFMEMORY				writeBufferMemHandle;
	NTSTATUS				status;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = ioTarget;

	status = WdfRequestCreate(&attributes, ioTarget, &newRequest);
	if (!NT_SUCCESS(status))
		return status;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = newRequest;
	status = WdfMemoryCreate(&attributes, NonPagedPool, 0, TAM_REPORTPEDALES, &writeBufferMemHandle, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(newRequest);
		return status;
	}

	status = WdfIoTargetFormatRequestForRead(ioTarget, newRequest, writeBufferMemHandle, NULL, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(newRequest);
		return status;
	}

	WdfRequestSetCompletionRoutine(newRequest, CompletionPedales, NULL);

	if (WdfRequestSend(newRequest, ioTarget, NULL) == FALSE)
	{
		WdfObjectDelete(newRequest);
		status = STATUS_UNSUCCESSFUL;
	}

	return status;
}

#pragma region "Iniciar IO target"
//PASSIVE_LEVEL
NTSTATUS IniciarIoTargetPassive(_In_ WDFDEVICE device)
{
	NTSTATUS status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES attributes;
	WDFIOTARGET ioTarget;
	WDF_IO_TARGET_OPEN_PARAMS  openParams;

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->Pedales.WaitLockIoTarget, NULL);
	{
		if (GetDeviceContext(device)->Pedales.IoTarget == NULL)
		{
			WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = device;

			status = WdfIoTargetCreate(device, &attributes, &ioTarget);
			if (NT_SUCCESS(status))
			{
				UNICODE_STRING strId;
					strId.Length = (USHORT)(wcslen(GetDeviceContext(device)->Pedales.SymbolicLink) * sizeof(WCHAR));
					strId.MaximumLength = strId.Length + sizeof(WCHAR);
					strId.Buffer = GetDeviceContext(device)->Pedales.SymbolicLink;
				WDF_IO_TARGET_OPEN_PARAMS_INIT_OPEN_BY_NAME(&openParams, &strId, STANDARD_RIGHTS_READ);
					openParams.ShareAccess = FILE_SHARE_READ | FILE_SHARE_WRITE;
					openParams.EvtIoTargetRemoveComplete = EvIoTargetRemoveComplete;
					openParams.EvtIoTargetRemoveCanceled = EvIoTargetRemoveCanceled;

				GetDeviceContext(device)->Pedales.IoTarget = ioTarget;

				status = WdfIoTargetOpen(ioTarget, &openParams);
				if (NT_SUCCESS(status))
				{
					if (WdfIoTargetGetState(ioTarget) == WdfIoTargetStarted)
					{
						status = IniciarReports(ioTarget);
						if (!NT_SUCCESS(status))
						{
							WdfWaitLockRelease(GetDeviceContext(device)->Pedales.WaitLockIoTarget);
							CerrarIoTargetPassive(device);
							return status;
						}
					}
					else
					{
						WdfWaitLockRelease(GetDeviceContext(device)->Pedales.WaitLockIoTarget);
						CerrarIoTargetPassive(device);
						return STATUS_UNSUCCESSFUL;
					}

				}
				else
				{
					WdfWaitLockRelease(GetDeviceContext(device)->Pedales.WaitLockIoTarget);
					CerrarIoTargetPassive(device);
					return status;
				}
			}
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->Pedales.WaitLockIoTarget);

	return status;
}

//PASSIVE_LEVEL
VOID IniciarIoTargetWI(_In_ WDFWORKITEM workItem)
{
	PAGED_CODE();

	IniciarIoTargetPassive((WDFDEVICE)WdfWorkItemGetParentObject(workItem));
	WdfObjectDelete(workItem);
}
#pragma endregion
#pragma endregion

#pragma region "Callbacks"
//PASSIVE_LEVEL
NTSTATUS PnPCallbackPedales(_In_ PVOID notification, _Inout_opt_ PVOID context)
{
	DECLARE_CONST_UNICODE_STRING(strId, HARDWARE_ID_PEDALES);

	NTSTATUS status = STATUS_SUCCESS;
	PDEVICE_INTERFACE_CHANGE_NOTIFICATION diNotify = (PDEVICE_INTERFACE_CHANGE_NOTIFICATION)notification;

	PAGED_CODE();

	if (IsEqualGUID((LPGUID)&diNotify->Event, (LPGUID)&GUID_DEVICE_INTERFACE_ARRIVAL))
	{
		if (diNotify->SymbolicLinkName->Length >= strId.Length)
		{
			UNICODE_STRING strLink;
			PWSTR wstrLink[sizeof(HARDWARE_ID_PEDALES)];
			RtlZeroMemory(wstrLink, sizeof(HARDWARE_ID_PEDALES));
			RtlCopyMemory(wstrLink, diNotify->SymbolicLinkName->Buffer, strId.Length);

			status = RtlUnicodeStringInit(&strLink, (NTSTRSAFE_PCWSTR)&wstrLink);
			if (!NT_SUCCESS(status))
				return status;
			else
			{
				if (RtlCompareUnicodeString(&strLink, &strId, TRUE) == 0)
				{
					WDF_OBJECT_ATTRIBUTES	attributes;
					WDF_WORKITEM_CONFIG		workitemConfig;
					WDFWORKITEM				workItem;
					size_t					tam = (diNotify->SymbolicLinkName->MaximumLength > 200) ? 199 : diNotify->SymbolicLinkName->MaximumLength;

					RtlZeroMemory(GetDeviceContext((WDFDEVICE)context)->Pedales.SymbolicLink, 200 * sizeof(WCHAR));
					RtlCopyMemory(GetDeviceContext((WDFDEVICE)context)->Pedales.SymbolicLink, diNotify->SymbolicLinkName->Buffer, tam * sizeof(WCHAR));

					WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
					attributes.ParentObject = context;

					WDF_WORKITEM_CONFIG_INIT(&workitemConfig, IniciarIoTargetWI);
					status = WdfWorkItemCreate(&workitemConfig, &attributes, &workItem);
					if (!NT_SUCCESS(status))
						return status;

					WdfWorkItemEnqueue(workItem);
				}
			}
		}
	}

	return status;
}

//PASSIVE_LEVEL
VOID EvIoTargetRemoveComplete(_In_ WDFIOTARGET ioTarget)
{
	CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
}

//PASSIVE_LEVEL
VOID EvIoTargetRemoveCanceled(_In_ WDFIOTARGET ioTarget)
{
	WDF_IO_TARGET_OPEN_PARAMS openParams;
	NTSTATUS status;

	WDF_IO_TARGET_OPEN_PARAMS_INIT_REOPEN(&openParams);
	status = WdfIoTargetOpen(ioTarget, &openParams);
	if (NT_SUCCESS(status))
	{
		if (WdfIoTargetGetState(ioTarget) == WdfIoTargetStarted)
			IniciarReports(ioTarget);
		else
			CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
	}
	else
		CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
}

// <=DISPATCH_LEVEL
//El writeBufferMemHandle se borra al borrar newRequest
VOID CompletionPedales(
	_In_ WDFREQUEST request,
	_In_ WDFIOTARGET ioTarget,
	_In_ PWDF_REQUEST_COMPLETION_PARAMS params,
	_In_ WDFCONTEXT context
)
{
	UNREFERENCED_PARAMETER(context);

	NTSTATUS  status;

	status = params->IoStatus.Status;
	if (NT_SUCCESS(status))
	{
		WDF_REQUEST_REUSE_PARAMS  rparams;
		WDFMEMORY mem = params->Parameters.Read.Buffer;

		if (params->Parameters.Read.Length == TAM_REPORTPEDALES)
			ProcesarEntradaPedales(WdfIoTargetGetDevice(ioTarget), WdfMemoryGetBuffer(mem, NULL));

		WDF_REQUEST_REUSE_PARAMS_INIT(&rparams, WDF_REQUEST_REUSE_NO_FLAGS, STATUS_SUCCESS);
		status = WdfRequestReuse(request, &rparams);
		if (NT_SUCCESS(status))
		{
			WdfRequestSetCompletionRoutine(request, CompletionPedales, NULL);
			status = WdfIoTargetFormatRequestForRead(ioTarget, request, mem, NULL, NULL);
			if (NT_SUCCESS(status))
				if (WdfRequestSend(request, ioTarget, NULL) != FALSE)
					return;
		}

	}

	WdfObjectDelete(request);
	CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
}
#pragma endregion

VOID ProcesarEntradaPedales(_In_ WDFDEVICE device, _In_ PVOID buffer)
{
	PDEVICE_CONTEXT	devExt = GetDeviceContext(device);
	UCHAR			izq = 0xff - ((PUCHAR)buffer)[6];
	UCHAR			der = 0xff - ((PUCHAR)buffer)[5];
	UINT16			eje = 0xff;

	if ((izq < 80) && (der < 80))
	{
		devExt->Pedales.PedalSel = 0;
	}
	switch (devExt->Pedales.PedalSel)
	{
	case 1:
		eje = 0xff - izq;
		break;
	case 2:
		eje = 0x0101 + der;
		break;
	default:
		if (izq > der) devExt->Pedales.PedalSel = 1;
		else if (der >izq) devExt->Pedales.PedalSel = 2;
	}

	if ((izq < 2) && (der < 2)) // zona nula
		eje = 512;
	else if ((izq < 80) && (der < 80)) // zona con los dos pedales
		eje = (der >= izq) ? (0x100 + der - izq) * 2 : (0xff - izq + der) * 2;
	else // sólo un pedal
		eje *= 2;
	if (eje == 1024) eje = 1023;

	WdfSpinLockAcquire(devExt->Pedales.SpinLockPosicion);
	{
		devExt->Pedales.UltimaPosicion = eje;
	}
	WdfSpinLockRelease(devExt->Pedales.SpinLockPosicion);

	if (devExt->Pedales.Activado)
		LeerX52ConPedales(device);
}