/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

pedales_read.c

Abstract:

Archivo para el control de los pedales de Thrusmaster.

Environment:

User-mode Driver Framework 2

--*/

#define INITGUID

#include <windows.h>
#include <wdf.h>
#include <cfgmgr32.h>
#include <hidclass.h>
#include "context.h"
#include "x52_read.h"
#define _PRIVATE_
#include "pedales_read.h"
#undef _PRIVATE_

#pragma region "Cerrar"
//PASSIVE_LEVEL
VOID CerrarPedales(_In_ WDFDEVICE device)
{
	if (GetDeviceContext(device)->Pedales.PnPNotifyHandle != NULL)
	{
		CM_Unregister_Notification((HCMNOTIFICATION)GetDeviceContext(device)->Pedales.PnPNotifyHandle);
		GetDeviceContext(device)->Pedales.PnPNotifyHandle = NULL;
	}
	if (GetDeviceContext(device)->Pedales.WaitLockIoTarget != NULL)
	{
		CerrarIoTargetPassive(device);
		WdfObjectDelete(GetDeviceContext(device)->Pedales.WaitLockIoTarget);
		GetDeviceContext(device)->Pedales.WaitLockIoTarget = NULL;
	}
	if (GetDeviceContext(device)->Pedales.SpinLockPosicion != NULL)
	{
		WdfObjectDelete(GetDeviceContext(device)->Pedales.SpinLockPosicion);
		GetDeviceContext(device)->Pedales.SpinLockPosicion = NULL;
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
	DWORD cmRet;
	ULONG tam = 0;
	CM_NOTIFY_FILTER cmFilter;

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Pedales.SpinLockPosicion);
	if (!NT_SUCCESS(status)) return status;
	status = WdfWaitLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->Pedales.WaitLockIoTarget);
	if (!NT_SUCCESS(status)) return status;

	GetDeviceContext(device)->Pedales.IoTarget = NULL;
	RtlZeroMemory(GetDeviceContext(device)->Pedales.SymbolicLink, 100 * sizeof(WCHAR));
	GetDeviceContext(device)->Pedales.Activado = FALSE;
	GetDeviceContext(device)->Pedales.PedalSel = 0;
	GetDeviceContext(device)->Pedales.Posicion = 512;

	RtlZeroMemory(&cmFilter, sizeof(cmFilter));
	cmFilter.cbSize = sizeof(cmFilter);
	cmFilter.FilterType = CM_NOTIFY_FILTER_TYPE_DEVICEINTERFACE;
	cmFilter.u.DeviceInterface.ClassGuid = GUID_DEVINTERFACE_HID;
	cmRet = CM_Register_Notification(
		&cmFilter,						// PCM_NOTIFY_FILTER pFilter,
		(PVOID)GetDeviceContext(device),// PVOID pContext,
		PnPCallback,					// PCM_NOTIFY_CALLBACK pCallback,
		(PHCMNOTIFICATION)&GetDeviceContext(device)->Pedales.PnPNotifyHandle	// PHCMNOTIFICATION pNotifyContext
	);

	if (cmRet != CR_SUCCESS)
		return STATUS_UNSUCCESSFUL;

	cmRet = CM_Get_Device_Interface_List_Size(&tam, &cmFilter.u.DeviceInterface.ClassGuid, NULL, CM_GET_DEVICE_INTERFACE_LIST_PRESENT);
	if ((cmRet != CR_SUCCESS))
		return STATUS_UNSUCCESSFUL;

	if (tam > 1)
	{
		WDF_OBJECT_ATTRIBUTES  attributes;
		WDFMEMORY	hMem;
		PVOID		pMem;
		PWSTR		sInterfaz;

		WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		status = WdfMemoryCreate(&attributes, PagedPool, 0, tam * sizeof(WCHAR), &hMem, &pMem);
		if (!NT_SUCCESS(status))
			return status;

		RtlZeroMemory(pMem, tam * sizeof(WCHAR));

		cmRet = CM_Get_Device_Interface_List(&cmFilter.u.DeviceInterface.ClassGuid, NULL, pMem, tam, CM_GET_DEVICE_INTERFACE_LIST_PRESENT);
		if ((cmRet != CR_SUCCESS))
		{
			WdfObjectDelete(hMem);
			return STATUS_UNSUCCESSFUL;
		}

		for (sInterfaz = pMem; *sInterfaz; sInterfaz += wcslen(sInterfaz) + 1)
		{
			if (IniciarIoTarget(sInterfaz, device))
				break;
		}
		WdfObjectDelete(hMem);
	}

	return STATUS_SUCCESS;
}

#pragma region "Iniciar IO target"
//PASSIVE_LEVEL
NTSTATUS IniciarIoTargetPassive(_In_ WDFDEVICE device)
{
	NTSTATUS status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES attributes;
	WDFIOTARGET ioTarget;
	WDF_IO_TARGET_OPEN_PARAMS  openParams;

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
				strId.Length = (USHORT)wcslen(GetDeviceContext(device)->Pedales.SymbolicLink);
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
	IniciarIoTargetPassive((WDFDEVICE)WdfWorkItemGetParentObject(workItem));
	WdfObjectDelete(workItem);
}

BOOLEAN IniciarIoTarget(_In_ PWSTR nombre, _In_ WDFDEVICE device)
{
	DECLARE_CONST_UNICODE_STRING(strId, HARDWARE_ID_PEDALES);

	if (wcsnlen_s(nombre, 32) >= strId.Length)
	{
		WCHAR wstrLink[28 * sizeof(WCHAR)];
		UNICODE_STRING strLink;
		strLink.Buffer = wstrLink;
		strLink.Length = strId.Length;
		strLink.MaximumLength = strId.MaximumLength;
		RtlZeroMemory(strLink.Buffer, strLink.MaximumLength);
		RtlCopyMemory(strLink.Buffer, nombre, strId.Length);
		if (RtlCompareUnicodeString(&strLink, &strId, TRUE) == 0)
		{
			NTSTATUS				status = STATUS_SUCCESS;
			WDF_OBJECT_ATTRIBUTES	attributes;
			WDF_WORKITEM_CONFIG		workitemConfig;
			WDFWORKITEM				workItem;
			DWORD					tam = (DWORD)wcsnlen_s(nombre, 100);

			if (0 != RtlCopyMemory(GetDeviceContext(device)->Pedales.SymbolicLink, nombre, tam * sizeof(WCHAR)))
				return FALSE;

			WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = device;

			WDF_WORKITEM_CONFIG_INIT(&workitemConfig, IniciarIoTargetWI);
			status = WdfWorkItemCreate(&workitemConfig, &attributes, &workItem);
			if (NT_SUCCESS(status))
			{
				WdfWorkItemEnqueue(workItem);
				return TRUE;
			}
		}
	}

	return FALSE;
}
#pragma endregion

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
#pragma endregion

#pragma region "Callbacks"
DWORD PnPCallback(
	_In_ HCMNOTIFICATION       hNotify,
	_In_opt_ PVOID             Context,
	_In_ CM_NOTIFY_ACTION      Action,
	_In_reads_bytes_(EventDataSize) PCM_NOTIFY_EVENT_DATA EventData,
	_In_ DWORD                 EventDataSize
)
{
	UNREFERENCED_PARAMETER(hNotify);
	UNREFERENCED_PARAMETER(EventDataSize);

	WDFDEVICE device = (WDFDEVICE)Context;

	if (Action == CM_NOTIFY_ACTION_DEVICEINTERFACEARRIVAL)
		IniciarIoTarget(EventData->u.DeviceInterface.SymbolicLink, device);

	return 0;
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

	if (devExt->Pedales.Activado)
		LeerX52ConPedales(devExt);
}