/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

x52_read.c

Abstract:

Archivo de lectura del X52 por USB.

Environment:

User-mode Driver Framework 2

--*/
#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include <usb.h>
#include <usbioctl.h>
#include <hidport.h>
#include "context.h"
#include "X52_write.h"
#include "ProcesarHID.h"
#include "RequestHID_read.h"
#define _PRIVATE_
#include "x52_read.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, IniciarX52)
#endif

//PASSIVE
NTSTATUS IniciarX52(_In_ WDFDEVICE device)
{
	NTSTATUS status;
	WDF_OBJECT_ATTRIBUTES	attributes;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;

	status = WdfSpinLockCreate(&attributes, &GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
	if (!NT_SUCCESS(status)) return status;
	status = WdfSpinLockCreate(&attributes, &GetDeviceContext(device)->EntradaX52.SpinLockRequest);
	if (!NT_SUCCESS(status)) return status;
	status = WdfCollectionCreate(&attributes, &GetDeviceContext(device)->EntradaX52.ListaRequest);
	
	return status;
}


#pragma region "Callbacks"
//DISPATCH
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
	BOOLEAN						ret;

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
			if (purb->UrbBulkOrInterruptTransfer.TransferBufferLength >= (29 + 1))
			{
				PDEVICE_CONTEXT	devExt = GetDeviceContext(WdfIoQueueGetDevice(Queue));
				BOOLEAN			pasarAUSB = TRUE;

				WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
				{
					if (devExt->EntradaX52.RequestEnUSB)
					{
						status = WdfCollectionAdd(devExt->EntradaX52.ListaRequest, Request);
						WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
						if (!NT_SUCCESS(status))
						{
							WdfRequestComplete(Request, status);
							return;
						}
						else
						{
							ProcesarRequest(WdfIoQueueGetDevice(Queue));
							pasarAUSB = FALSE;
						}
					}
					else
					{
						WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
					}
				}

				if (pasarAUSB) //pasar request hacia abajo
				{
					devExt->EntradaX52.RequestEnUSB = TRUE;
					WdfRequestFormatRequestUsingCurrentType(Request);
					WdfRequestSetCompletionRoutine(Request, EvtCompletionX52Data, NULL);
					purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
					ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL);
					{
						if (!ret)
						{
							devExt->EntradaX52.RequestEnUSB = FALSE;
							status = WdfRequestGetStatus(Request);
							WdfRequestComplete(Request, status);
						}
					}
				}

				return;
			}
			else
			{
				purb->UrbBulkOrInterruptTransfer.TransferBufferLength = 29 + 1;
				purb->UrbHeader.Status = USBD_STATUS_BUFFER_TOO_SMALL;
				WdfRequestComplete(Request, status);
				return;
			}
		}
		case URB_FUNCTION_GET_DESCRIPTOR_FROM_DEVICE:
		{
			if (purb->UrbControlDescriptorRequest.DescriptorType == USB_CONFIGURATION_DESCRIPTOR_TYPE)
			{
				WdfRequestFormatRequestUsingCurrentType(Request);
				WdfRequestSetCompletionRoutine(Request, EvtCompletionConfigDescriptor, NULL);
				ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL);

				if (ret == FALSE) {
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
	ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), &options);
	if (ret == FALSE)
	{
		status = WdfRequestGetStatus(Request);
		WdfRequestComplete(Request, status);
	}
}

//DISPATCH
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


//DISPATCH
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
	NTSTATUS	status;
	//BOOLEAN		cancelada = TRUE;
	WDF_REQUEST_PARAMETERS params;
	PURB purb;

	WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
	{
		GetDeviceContext(device)->EntradaX52.RequestEnUSB = FALSE;
		//{
		//	GetDeviceContext(device)->EntradaX52.Request = NULL;
		//	cancelada = FALSE;
		//}
	}
	WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockRequest);

	WDF_REQUEST_PARAMETERS_INIT(&params);
	WdfRequestGetParameters(Request, &params);
	purb = (PURB)params.Parameters.Others.Arg1;

	status = WdfRequestGetStatus(Request);


	//BOOLEAN repetirUltimo = FALSE;
	//if ((status == STATUS_CANCELLED) && cancelada)
	//	//La llamada viene por la cancelación de Request. Si !cancelada es una cancelación desde fuera del driver
	//	// y no se toca nada
	//{
	//	if (purb->UrbHeader.Status == USBD_STATUS_CANCELED)
	//	{
	//		purb->UrbHeader.Status = USBD_STATUS_SUCCESS;
	//		status = STATUS_SUCCESS;
	//		repetirUltimo = TRUE;
	//	}
	//}
	if (NT_SUCCESS(status))
		//La llamada no viene de WdfRequestCancelSentRequest. Aunque ya esté fuera de la lista de request todavía no
		// se ha activado la cancelación y se pueden usar los datos
	{
		if (purb->UrbHeader.Status == USBD_STATUS_SUCCESS)
		{
			ProcesarInputX52(device, purb->UrbBulkOrInterruptTransfer.TransferBuffer, FALSE); // repetirUltimo);
			WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
			{
				status = WdfCollectionAdd(GetDeviceContext(device)->EntradaX52.ListaRequest, Request);
			}
			WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
			if (NT_SUCCESS(status))
			{
				ProcesarRequest(device);
				return;
			}
		}
	}

    WdfRequestSetInformation(Request, sizeof(HID_INPUT_DATA) + 1);
	WdfRequestComplete(Request, status);
}
#pragma endregion

//DISPATCH
VOID LeerX52ConPedales(WDFDEVICE device)
{
	//// Cancela una de las request enviadas hacia abajo para que se vuelva a hacer la petición
	//// con la posición de los pedales actualizada
	//WDFREQUEST request = NULL;
	//WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
	//{
	//	if (devExt->EntradaX52.Request != NULL)
	//	{
	//		request = devExt->EntradaX52.Request;
	//		devExt->EntradaX52.Request = NULL;
	//		WdfObjectReference(request);
	//	}
	//}
	//WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
	//if (request != NULL)
	//{
	//	WdfRequestCancelSentRequest(request);
	//	WdfObjectDereference(request);
	//}
	HIDX52_INPUT_DATA input;
	RtlZeroMemory(&input, sizeof(HIDX52_INPUT_DATA));
	ProcesarInputX52(device, &input, TRUE);
	ProcesarRequest(device);
}
