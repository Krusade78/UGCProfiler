#define _PRIVATE_
#include "pedales.h"
#undef _PRIVATE
#include "extensions.h"

#include <wdm.h>
#include <initguid.h>
#include <hidclass.h>
#include <wdmguid.h>
#include <Ntstrsafe.h>

#define HARDWARE_ID_PEDALES  L"\\??\\HID#Vid_044f&Pid_b653"
#define REPORTPEDALES_TAM	 8


#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, IniciarPedales)
#pragma alloc_text(PAGE, CerrarPedales)
#pragma alloc_text(PAGE, PnPCallback)
#pragma alloc_text(PAGE, IniciarIoTarget)
#pragma alloc_text(PAGE, CerrarIoTarget)
#pragma alloc_text(PAGE, CerrarIoTargetPassive)
#pragma alloc_text(PAGE, EvIoTargetRemoveComplete)
#pragma alloc_text(PAGE, EvIoTargetRemoveCanceled)
#pragma alloc_text(PAGE, IniciarReports)
#pragma alloc_text(PAGE, CompletionPedales)
#pragma alloc_text(PAGE, ProcesarEntradaPedales)
#endif

//PASSIVE_LEVEL
NTSTATUS IniciarPedales(_In_ WDFDEVICE device)
{
	NTSTATUS status;

	PAGED_CODE();

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceExtension(device)->Pedales.SpinLockPosicion);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceExtension(device)->Pedales.WaitLockIoTarget);
	if (!NT_SUCCESS(status)) return status;

	return IoRegisterPlugPlayNotification(
										EventCategoryDeviceInterfaceChange,
										PNPNOTIFY_DEVICE_INTERFACE_INCLUDE_EXISTING_INTERFACES,
										(PGUID)&GUID_DEVINTERFACE_HID,
										WdfDriverWdmGetDriverObject(WdfDeviceGetDriver(device)),
										PnPCallback,
										GetDeviceExtension(device),
										&GetDeviceExtension(device)->PnPNotifyHandle);
}

//PASSIVE_LEVEL
void CerrarPedales(_In_ WDFDEVICE device)
{
	if (GetDeviceExtension(device)->PnPNotifyHandle != NULL)
	{
		IoUnregisterPlugPlayNotificationEx(GetDeviceExtension(device)->PnPNotifyHandle);
		GetDeviceExtension(device)->PnPNotifyHandle = NULL;
	}
	if (GetDeviceExtension(device)->Pedales.WaitLockIoTarget != NULL)
	{
		WdfWaitLockAcquire(GetDeviceExtension(device)->Pedales.WaitLockIoTarget, NULL);
		{
			if (GetDeviceExtension(device)->Pedales.IoTarget != NULL)
				CerrarIoTarget(device);
		}
		WdfWaitLockRelease(GetDeviceExtension(device)->Pedales.WaitLockIoTarget);
		WdfObjectDelete(GetDeviceExtension(device)->Pedales.WaitLockIoTarget);
		GetDeviceExtension(device)->Pedales.WaitLockIoTarget = NULL;
	}
	if (GetDeviceExtension(device)->Pedales.SpinLockPosicion != NULL)
	{
		WdfObjectDelete(GetDeviceExtension(device)->Pedales.SpinLockPosicion);
		GetDeviceExtension(device)->Pedales.SpinLockPosicion = NULL;
	}
}

//PASSIVE_LEVEL
NTSTATUS PnPCallback(_In_ PVOID notification, _Inout_opt_ PVOID context)
{
	NTSTATUS status = STATUS_SUCCESS;
	PDEVICE_INTERFACE_CHANGE_NOTIFICATION diNotify = (PDEVICE_INTERFACE_CHANGE_NOTIFICATION)notification;

	PAGED_CODE();

	if (IsEqualGUID((LPGUID)&diNotify->Event, (LPGUID)&GUID_DEVICE_INTERFACE_ARRIVAL))
	{
		UNICODE_STRING strId;
		status = RtlUnicodeStringInit(&strId, HARDWARE_ID_PEDALES);
		if (!NT_SUCCESS(status))
			return status;
		if (diNotify->SymbolicLinkName->Length >= strId.Length)
		{
			UNICODE_STRING strLink;
			PWSTR wstrLink[sizeof(HARDWARE_ID_PEDALES)];
			RtlCopyMemory(wstrLink, HARDWARE_ID_PEDALES, sizeof(HARDWARE_ID_PEDALES));
			status = RtlUnicodeStringInit(&strLink, (NTSTRSAFE_PCWSTR)&wstrLink);
			if (!NT_SUCCESS(status))
				return status;
			status = RtlUnicodeStringCbCopyN(&strLink, diNotify->SymbolicLinkName, strId.Length);
			if (NT_SUCCESS(status))
			{
				if (RtlCompareUnicodeString(&strLink, &strId, TRUE) == 0)
					return IniciarIoTarget(diNotify->SymbolicLinkName, ((PDEVICE_EXTENSION)context)->Self);
			}
		}
	}

	return status;
}

//PASSIVE_LEVEL
NTSTATUS IniciarIoTarget(_In_ PUNICODE_STRING strId, _In_ WDFDEVICE device)
{
	NTSTATUS status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES attributes;
	WDFIOTARGET ioTarget;
	WDF_IO_TARGET_OPEN_PARAMS  openParams;

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceExtension(device)->Pedales.WaitLockIoTarget, NULL);
	{
		if (GetDeviceExtension(device)->Pedales.IoTarget != NULL)
			return status;

		WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;

		status = WdfIoTargetCreate(device, &attributes, &ioTarget);
		if (NT_SUCCESS(status))
		{
			WDF_IO_TARGET_OPEN_PARAMS_INIT_OPEN_BY_NAME(&openParams, strId, STANDARD_RIGHTS_READ);
			openParams.ShareAccess = FILE_SHARE_READ;
			openParams.EvtIoTargetRemoveComplete = EvIoTargetRemoveComplete;
			openParams.EvtIoTargetRemoveCanceled = EvIoTargetRemoveCanceled;

			GetDeviceExtension(device)->Pedales.IoTarget = ioTarget;

			status = WdfIoTargetOpen(ioTarget, &openParams);
			if (NT_SUCCESS(status) && ((openParams.FileInformation == FILE_SUPERSEDED) || (openParams.FileInformation == FILE_OPENED)))
			{
				if (WdfIoTargetGetState(ioTarget) == WdfIoTargetStarted)
				{
					status = IniciarReports(ioTarget);
				}
				else
				{
					CerrarIoTarget(device);
					status = STATUS_UNSUCCESSFUL;
				}

			}
			else
			{
				CerrarIoTarget(device);
			}
		}
	}
	WdfWaitLockRelease(GetDeviceExtension(device)->Pedales.WaitLockIoTarget);

	return status;
}

// <=DISPATCH_LEVEL
NTSTATUS CerrarIoTarget(_In_ WDFDEVICE device)
{
	NTSTATUS				status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_WORKITEM_CONFIG		workitemConfig;
	WDFWORKITEM				workItem;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;

	WDF_WORKITEM_CONFIG_INIT(&workitemConfig, CerrarIoTargetPassive);
	status = WdfWorkItemCreate(&workitemConfig,	&attributes, &workItem);
	if (NT_SUCCESS(status))
		WdfWorkItemEnqueue(workItem);

	return status;
}
//PASSIVE_LEVEL
VOID CerrarIoTargetPassive(_In_ WDFWORKITEM workItem)
{
	WDFDEVICE	device = (WDFDEVICE)WdfWorkItemGetParentObject(workItem);
	WDFIOTARGET ioTarget = NULL;
	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceExtension(device)->Pedales.WaitLockIoTarget, NULL);
	{
		ioTarget = GetDeviceExtension(device)->Pedales.IoTarget;
		GetDeviceExtension(device)->Pedales.IoTarget = NULL;
	}
	WdfWaitLockRelease(GetDeviceExtension((WDFDEVICE)WdfIoTargetGetDevice(ioTarget))->Pedales.WaitLockIoTarget);
	if (ioTarget != NULL)
	{
		WdfIoTargetClose(ioTarget);
		WdfObjectDelete(ioTarget);
	}

	WdfObjectDelete(workItem);
}

//PASSIVE_LEVEL
VOID EvIoTargetRemoveComplete(_In_ WDFIOTARGET ioTarget)
{
	PAGED_CODE();
	CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
}

//PASSIVE_LEVEL
VOID EvIoTargetRemoveCanceled(_In_ WDFIOTARGET ioTarget)
{
	WDF_IO_TARGET_OPEN_PARAMS openParams;
	NTSTATUS status;

	PAGED_CODE();
	
	WDF_IO_TARGET_OPEN_PARAMS_INIT_REOPEN(&openParams);
	status = WdfIoTargetOpen(ioTarget, &openParams);
	if (NT_SUCCESS(status) && ((openParams.FileInformation == FILE_SUPERSEDED) || (openParams.FileInformation == FILE_OPENED)))
	{
		if (WdfIoTargetGetState(ioTarget) == WdfIoTargetStarted)
			IniciarReports(ioTarget);
		else
			CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
	}
	else
		CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
}

//PASSIVE_LEVEL
NTSTATUS IniciarReports(WDFIOTARGET ioTarget)
{
	WDFDEVICE				device = WdfIoTargetGetDevice(ioTarget);
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFREQUEST				newRequest = NULL;
	WDFMEMORY				writeBufferMemHandle;
	NTSTATUS				status;
	PVOID					buffer;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = ioTarget;

	status = WdfRequestCreate(&attributes, ioTarget, &newRequest);
	if (!NT_SUCCESS(status))
	{
		CerrarIoTarget(device);
		return status;
	}

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = newRequest;
	status = WdfMemoryCreate(&attributes, NonPagedPool, 0, REPORTPEDALES_TAM, &writeBufferMemHandle, &buffer);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(newRequest);
		CerrarIoTarget(device);
		return status;
	}

	status = WdfIoTargetFormatRequestForRead(ioTarget, newRequest, writeBufferMemHandle, NULL, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(writeBufferMemHandle);
		WdfObjectDelete(newRequest);
		CerrarIoTarget(device);
		return status;
	}

	WdfRequestSetCompletionRoutine(newRequest, CompletionPedales, NULL);

	if (WdfRequestSend(newRequest, ioTarget, NULL) == FALSE)
	{
		WdfObjectDelete(writeBufferMemHandle);
		WdfObjectDelete(newRequest);
		CerrarIoTarget(device);
		status = STATUS_UNSUCCESSFUL;
	}

	return status;
}

// <=DISPATCH_LEVEL
void CompletionPedales(
	_In_ WDFREQUEST request,
	_In_ WDFIOTARGET ioTarget,
	_In_ PWDF_REQUEST_COMPLETION_PARAMS params,
	_In_ WDFCONTEXT context
	)
{
	UNREFERENCED_PARAMETER(context);

	PAGED_CODE();

	NTSTATUS  status;

	status = params->IoStatus.Status;
	if (NT_SUCCESS(status))
	{
		WDF_REQUEST_REUSE_PARAMS  rparams;
		WDFMEMORY mem = params->Parameters.Read.Buffer;

		if (params->Parameters.Read.Length == REPORTPEDALES_TAM)
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

	CerrarIoTarget(WdfIoTargetGetDevice(ioTarget));
}

void ProcesarEntradaPedales(_In_ WDFDEVICE device, _In_ PVOID buffer)
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(device);
	UCHAR				izq = 0xff - ((PUCHAR)buffer)[6];
	UCHAR				der = 0xff - ((PUCHAR)buffer)[5];
	UINT16				eje = 0xff;

	PAGED_CODE();

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
		eje = 0x0100 + der;
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

	WdfSpinLockAcquire(devExt->Pedales.SpinLockPosicion);
	{
		devExt->Pedales.Posicion = eje;
	}
	WdfSpinLockRelease(devExt->Pedales.SpinLockPosicion);
}