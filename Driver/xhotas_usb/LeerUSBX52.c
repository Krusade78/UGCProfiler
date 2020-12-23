/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

x52_read.c

Abstract:

Archivo de lectura del X52 por USB.

--*/
#include <ntddk.h>
#include <wdf.h>
#include <usb.h>
#include <usbioctl.h>
#include <hidport.h>
#include "context.h"
#define _PUBLIC_
#include "ProcesarUSBs.h"
#include "ProcesarHID.h"
#undef _PUBLIC_
#define _PRIVATE_
#include "LeerUSBX52.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, ProcesarEntradaX52WI)
#endif

#pragma region "Callbacks"
/// <summary>
/// DISPATCH
/// </summary>
VOID EvtX52InternalIOCtl(
	_In_  WDFQUEUE Queue,
	_In_  WDFREQUEST Request,
	_In_  size_t OutputBufferLength,
	_In_  size_t InputBufferLength,
	_In_  ULONG IoControlCode
)
{
	NTSTATUS					status = STATUS_SUCCESS;
	WDF_REQUEST_SEND_OPTIONS	options;

	UNREFERENCED_PARAMETER(OutputBufferLength);
	UNREFERENCED_PARAMETER(InputBufferLength);

	if (IoControlCode == IOCTL_INTERNAL_USB_SUBMIT_URB)
	{
		WDF_REQUEST_PARAMETERS  params;
		PURB purb;

		WDF_REQUEST_PARAMETERS_INIT(&params);
		WdfRequestGetParameters(Request, &params);
		purb = (PURB)params.Parameters.Others.Arg1;
		switch (purb->UrbHeader.Function)
		{
		case URB_FUNCTION_BULK_OR_INTERRUPT_TRANSFER:
		{
			if (purb->UrbBulkOrInterruptTransfer.TransferBufferLength == (29 + 1))
			{
				PDEVICE_CONTEXT	devExt = GetDeviceContext(WdfIoQueueGetDevice(Queue));

				if (InterlockedCompareExchange16(&devExt->EntradaX52.RequestEnUSB, 1, 0)) //si hay request en USB
				{
					status = WdfRequestForwardToIoQueue(Request, devExt->EntradaX52.ColaRequest);
					if (!NT_SUCCESS(status))
					{
						WdfRequestComplete(Request, status);
					}
					return;
				}
				else //pasar request hacia abajo
				{
					WdfRequestFormatRequestUsingCurrentType(Request);
					WdfRequestSetCompletionRoutine(Request, EvtCompletionX52Data, NULL);
					if (!WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL))
					{
						InterlockedDecrement16(&devExt->EntradaX52.RequestEnUSB);
						status = WdfRequestGetStatus(Request);
						WdfRequestComplete(Request, status);
					}
				}

				return;
			}
			else
			{
				WdfRequestCompleteWithInformation(Request, STATUS_INVALID_BUFFER_SIZE, 29 + 1);
				return;
			}
		}
		case URB_FUNCTION_GET_DESCRIPTOR_FROM_DEVICE:
		{
			if (purb->UrbControlDescriptorRequest.DescriptorType == USB_CONFIGURATION_DESCRIPTOR_TYPE)
			{
				WdfRequestFormatRequestUsingCurrentType(Request);
				WdfRequestSetCompletionRoutine(Request, EvtCompletionConfigDescriptor, NULL);
				if (!WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL))
				{
					status = WdfRequestGetStatus(Request);
					WdfRequestComplete(Request, status);
				}
				return;
			}
			break;
		}
		case URB_FUNCTION_GET_DESCRIPTOR_FROM_INTERFACE:
		{
			if (purb->UrbControlDescriptorRequest.TransferBufferLength >= sizeof(reportDescriptor))
			{
				PVOID p = purb->UrbControlDescriptorRequest.TransferBuffer;
				memcpy(p, reportDescriptor, sizeof(reportDescriptor));
				purb->UrbControlDescriptorRequest.TransferBufferLength = sizeof(reportDescriptor);
				purb->UrbHeader.Status = USBD_STATUS_SUCCESS;
			}
			else
			{
				purb->UrbControlDescriptorRequest.TransferBufferLength = sizeof(reportDescriptor);
				purb->UrbHeader.Status = USBD_STATUS_BUFFER_TOO_SMALL;
			}

			WdfRequestComplete(Request, status);
			return;
		}
		}
	}

	WDF_REQUEST_SEND_OPTIONS_INIT(&options, WDF_REQUEST_SEND_OPTION_SEND_AND_FORGET);
	if (!WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), &options))
	{
		status = WdfRequestGetStatus(Request);
		WdfRequestComplete(Request, status);
	}
}

/// <summary>
/// <=DISPATCH
/// </summary>
void EvtCompletionConfigDescriptor(
	_In_ WDFREQUEST                     Request,
	_In_ WDFIOTARGET                    Target,
	_In_ PWDF_REQUEST_COMPLETION_PARAMS Params,
	_In_ WDFCONTEXT                     Context
)
{
	UNREFERENCED_PARAMETER(Target);
	UNREFERENCED_PARAMETER(Context);
	UNREFERENCED_PARAMETER(Params);

	NTSTATUS status = WdfRequestGetStatus(Request);

	if (status == STATUS_SUCCESS)
	{
		WDF_REQUEST_PARAMETERS rparams;
		PUSB_CONFIGURATION_DESCRIPTOR pconfig;
		PUSB_INTERFACE_DESCRIPTOR pinterface;
		PHID_DESCRIPTOR phid;
		PURB purb;

		WDF_REQUEST_PARAMETERS_INIT(&rparams);
		WdfRequestGetParameters(Request, &rparams);
		purb = (PURB)rparams.Parameters.Others.Arg1;

		if ((purb->UrbHeader.Status == USBD_STATUS_SUCCESS) && (purb->UrbHeader.Function == URB_FUNCTION_CONTROL_TRANSFER))
		{
			if (purb->UrbControlTransfer.TransferBufferLength == 34)
			{		
				pconfig = (PUSB_CONFIGURATION_DESCRIPTOR)purb->UrbControlTransfer.TransferBuffer;
				pinterface = (PUSB_INTERFACE_DESCRIPTOR)((BYTE*)pconfig + pconfig->bLength);
				phid = (PHID_DESCRIPTOR)((BYTE*)pinterface + pinterface->bLength);
				phid->DescriptorList[0].wReportLength = sizeof(reportDescriptor);
			}
		}
	}
	WdfRequestComplete(Request, status);
}


/// <summary>
/// <=DISPATCH
/// </summary>
void EvtCompletionX52Data(
	_In_ WDFREQUEST                     Request,
	_In_ WDFIOTARGET                    Target,
	_In_ PWDF_REQUEST_COMPLETION_PARAMS Params,
	_In_ WDFCONTEXT                     Context
)
{
	UNREFERENCED_PARAMETER(Context);
	UNREFERENCED_PARAMETER(Params);

	WDFDEVICE	device = WdfIoTargetGetDevice(Target);
	NTSTATUS	status = WdfRequestGetStatus(Request);
	WDF_REQUEST_PARAMETERS params;
	PURB purb;

	InterlockedDecrement16(&GetDeviceContext(device)->EntradaX52.RequestEnUSB);

	WDF_REQUEST_PARAMETERS_INIT(&params);
	WdfRequestGetParameters(Request, &params);
	purb = (PURB)params.Parameters.Others.Arg1;

	if (NT_SUCCESS(status))
	{
		ProcesarEntradaX52(device, purb->UrbBulkOrInterruptTransfer.TransferBuffer);
		purb->UrbBulkOrInterruptTransfer.TransferBufferLength = 29 + 1;
		status = WdfRequestForwardToIoQueue(Request, GetDeviceContext(device)->EntradaX52.ColaRequest);
		if (NT_SUCCESS(status))
		{
			return;
		}
	}

	WdfRequestComplete(Request, status);
}
#pragma endregion

/// <summary>
/// <=DISPATCH_LEVEL
/// </summary>
VOID ProcesarEntradaX52(_In_ WDFDEVICE device, _In_ PVOID buffer)
{
	NTSTATUS				status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDF_WORKITEM_CONFIG		workitemConfig;
	WDFWORKITEM				workItem;

	WDF_OBJECT_ATTRIBUTES_INIT_CONTEXT_TYPE(&attributes, WI_CONTEXT);
	attributes.ParentObject = device;
	WDF_WORKITEM_CONFIG_INIT(&workitemConfig, ProcesarEntradaX52WI);
	status = WdfWorkItemCreate(&workitemConfig, &attributes, &workItem);
	if (NT_SUCCESS(status))
	{
		RtlCopyMemory(&GetWIContext(workItem)->Buffer, (PHIDX52_INPUT_DATA)buffer, sizeof(HIDX52_INPUT_DATA));
		WdfWorkItemEnqueue(workItem);
	}
}

/// <summary>
/// PASSIVE_LEVEL
/// </summary>
VOID ProcesarEntradaX52WI(_In_ WDFWORKITEM workItem)
{
	PAGED_CODE();

	WDFDEVICE			device = (WDFDEVICE)WdfWorkItemGetParentObject(workItem);
	HIDX52_INPUT_DATA	buffer;

	RtlCopyMemory(&buffer, &GetWIContext(workItem)->Buffer, sizeof(HIDX52_INPUT_DATA));
	WdfObjectDelete(workItem);

	ProcesarEntradaUSB(device, &buffer, FALSE);
	ProcesarRequestHIDForzada(device);
}