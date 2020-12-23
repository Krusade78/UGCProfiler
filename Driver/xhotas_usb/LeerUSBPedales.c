/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

pedales_read.c

Abstract:

Archivo para el control de los pedales.

--*/


#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "PnPPedales.h"
#include "ProcesarUSBs.h"
#include "ProcesarHID.h"
#define _PRIVATE_
#include "LeerUSBPedales.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, IniciarReportsUSBPedales)
#pragma alloc_text (PAGE, ProcesarEntradaPedalesWI)
#endif

/// <summary>
/// [PASSIVE_LEVEL] El writeBufferMemHandle se borra al borrar newRequest
/// </summary>
NTSTATUS IniciarReportsUSBPedales(WDFIOTARGET ioTarget)
{
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFREQUEST				newRequest;
	WDFMEMORY				writeBufferMemHandle;
	NTSTATUS				status;

	PAGED_CODE();

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


#pragma region "Callbacks"
/// <summary>
/// [<=DISPATCH_LEVEL] El writeBufferMemHandle se borra al borrar newRequest
/// </summary>
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

/// <summary>
/// <=DISPATCH_LEVEL
/// </summary>
VOID ProcesarEntradaPedales(_In_ WDFDEVICE device, _In_ PVOID buffer)
{
	NTSTATUS				status;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_WORKITEM_CONFIG		workitemConfig;
	WDFWORKITEM				workItem;

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, WI_CONTEXT);
	attributes.ParentObject = device;
	WDF_WORKITEM_CONFIG_INIT(&workitemConfig, ProcesarEntradaPedalesWI);
	status = WdfWorkItemCreate(&workitemConfig, &attributes, &workItem);
	if (NT_SUCCESS(status))
	{
		RtlCopyMemory(GetWIContext(workItem)->Buffer, (PUCHAR)buffer, TAM_REPORTPEDALES);
		WdfWorkItemEnqueue(workItem);
	}
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
VOID ProcesarEntradaPedalesWI(_In_ WDFWORKITEM workItem)
{
	PAGED_CODE();

	WDFDEVICE		device = (WDFDEVICE)WdfWorkItemGetParentObject(workItem);
	PDEVICE_CONTEXT	devExt = GetDeviceContext(device);
	UCHAR			buffer[4] = { 0, 0, 0, 0 };

	RtlCopyMemory(buffer, GetWIContext(workItem)->Buffer, TAM_REPORTPEDALES);
	WdfObjectDelete(workItem);

	WdfWaitLockAcquire(devExt->Pedales.WaitLockPosicion, NULL);
	{
		devExt->Pedales.UltimaPosicionRz = (UINT16)((((PUCHAR)buffer)[2] >> 6) + (((PUCHAR)buffer)[3] << 2));
		devExt->Pedales.UltimaPosicionFrenoI = (UCHAR)(((PUCHAR)buffer)[1] & 0x7F);
		devExt->Pedales.UltimaPosicionFrenoD = (UCHAR)((((PUCHAR)buffer)[1] >> 7) + ((((PUCHAR)buffer)[2] & 0x3f) << 1));
		if (devExt->Pedales.Activado)
		{
			ProcesarEntradaUSB(device, NULL, TRUE);
		}
	}
	WdfWaitLockRelease(devExt->Pedales.WaitLockPosicion);
	if (devExt->Pedales.Activado)
	{
		ProcesarRequestHIDForzada(device);
	}
}
