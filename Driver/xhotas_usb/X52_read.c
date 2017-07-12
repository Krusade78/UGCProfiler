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
#define _PRIVATE_
#include "x52_read.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, IniciarX52)
#pragma alloc_text (PAGE, CerrarX52)
#endif

//PASSIVE
NTSTATUS IniciarX52(_In_ WDFDEVICE device)
{
	NTSTATUS status;

	PAGED_CODE();

	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
	if (!NT_SUCCESS(status)) return status;
	status = WdfSpinLockCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->EntradaX52.SpinLockRequest);
	if (!NT_SUCCESS(status)) return status;
	status = WdfCollectionCreate(WDF_NO_OBJECT_ATTRIBUTES, &GetDeviceContext(device)->EntradaX52.ListaRequest);
	
	return status;
}


//PASSIVE
VOID CerrarX52(_In_ WDFDEVICE device)
{
	PAGED_CODE();

	if (GetDeviceContext(device)->EntradaX52.ListaRequest != NULL)
	{
		WdfObjectDelete(GetDeviceContext(device)->EntradaX52.ListaRequest);
		GetDeviceContext(device)->EntradaX52.ListaRequest = NULL;
	}
	if (GetDeviceContext(device)->EntradaX52.SpinLockPosicion != NULL)
	{
		WdfObjectDelete(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
		GetDeviceContext(device)->EntradaX52.SpinLockPosicion = NULL;
	}
	if (GetDeviceContext(device)->EntradaX52.SpinLockRequest != NULL)
	{
		WdfObjectDelete(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
		GetDeviceContext(device)->EntradaX52.SpinLockRequest = NULL;
	}
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
			if (purb->UrbBulkOrInterruptTransfer.TransferBufferLength >= (29 + 3 + sizeof(HID_INPUT_DATA) + 1)) //keyboard
			{			
				PDEVICE_CONTEXT devExt = GetDeviceContext(WdfIoQueueGetDevice(Queue));
				WDFREQUEST requestEnCola = NULL;

				//Keyboard
				{
					status = WdfIoQueueRetrieveNextRequest(devExt->ColaTeclado, &requestEnCola);
					if (((status == STATUS_NO_MORE_ENTRIES) || NT_SUCCESS(status)))
					{
						if (requestEnCola == NULL)
						{
							requestEnCola = Request;
						}

						status = WdfRequestForwardToIoQueue(requestEnCola, devExt->ColaTeclado);
						if (NT_SUCCESS(status) && (requestEnCola == Request))
							return;
					}
					if (!NT_SUCCESS(status))
					{
						WdfRequestComplete(Request, status);
						return;
					}
				}

				//Mouse
				{
					status = WdfIoQueueRetrieveNextRequest(devExt->ColaRaton, &requestEnCola);
					if (((status == STATUS_NO_MORE_ENTRIES) || NT_SUCCESS(status)))
					{
						if (requestEnCola == NULL)
						{
							requestEnCola = Request;
						}

						status = WdfRequestForwardToIoQueue(requestEnCola, devExt->ColaRaton);
						if (NT_SUCCESS(status) && (requestEnCola == Request))
							return;
					}
					if (!NT_SUCCESS(status))
					{
						WdfRequestComplete(Request, status);
						return;
					}
				}

				//Joystick
				{
					WdfRequestFormatRequestUsingCurrentType(Request);
					WdfRequestSetCompletionRoutine(Request, EvtCompletionX52Data, NULL);
					purb->UrbBulkOrInterruptTransfer.TransferBufferLength = 0x0f;
					ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL);
					WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
					{
						if (ret == FALSE)
						{
							status = WdfRequestGetStatus(Request);
							WdfRequestComplete(Request, status);
						}
						else
							WdfCollectionAdd(devExt->EntradaX52.ListaRequest, Request);
					}
					WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
				}

				return;
			}
			else
			{
				purb->UrbBulkOrInterruptTransfer.TransferBufferLength = 29 + 3 + sizeof(HID_INPUT_DATA) + 1;
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
	BOOLEAN		cancelada = TRUE;
	WDF_REQUEST_PARAMETERS params;
	PURB purb;

	WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockRequest);
	{
		for (ULONG i = 0; i < WdfCollectionGetCount(GetDeviceContext(device)->EntradaX52.ListaRequest); i++)
		{
			WDFREQUEST request = (WDFREQUEST)WdfCollectionGetItem(GetDeviceContext(device)->EntradaX52.ListaRequest, i);
			if (request == Request)
			{
				WdfCollectionRemoveItem(GetDeviceContext(device)->EntradaX52.ListaRequest, i);
				cancelada = FALSE;
				break;
			}
		}
	}
	WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockRequest);

	WDF_REQUEST_PARAMETERS_INIT(&params);
	WdfRequestGetParameters(Request, &params);
	purb = (PURB)params.Parameters.Others.Arg1;

	status = WdfRequestGetStatus(Request);

	if ((status == STATUS_CANCELLED) && cancelada)
	//La llamada viene por la cancelación de Request. Si !cancelada es una cancelación desde fuera del driver
	// y no se toca nada
	{
		if (purb->UrbHeader.Status == USBD_STATUS_CANCELED)
		{
			INT16 posPedales = 512;
			if (GetDeviceContext(device)->Pedales.Activado)
			{
				WdfSpinLockAcquire(GetDeviceContext(device)->Pedales.SpinLockPosicion);
					posPedales = GetDeviceContext(device)->Pedales.UltimaPosicion;
				WdfSpinLockRelease(GetDeviceContext(device)->Pedales.SpinLockPosicion);
			}
			WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
			{
				if (GetDeviceContext(device)->Pedales.Activado)
				{
					GetDeviceContext(device)->EntradaX52.UltimaPosicion.Ejes[5] = posPedales >> 8;
					GetDeviceContext(device)->EntradaX52.UltimaPosicion.Ejes[4] = posPedales & 0xff;
				}
				RtlCopyMemory((PVOID)((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer + 1), &GetDeviceContext(device)->EntradaX52.UltimaPosicion, sizeof(HID_INPUT_DATA));
			}
			WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);

			purb->UrbHeader.Status = USBD_STATUS_SUCCESS;
			*((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer) = 0x01; //Resport id
			purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
			status = STATUS_SUCCESS;
		}
	}
	else if (NT_SUCCESS(status))
	//La llamada no viene de WdfRequestCancelSentRequest. Aunque ya esté fuera de la lista de request todavía no
	// se ha activado la cancelación y se pueden usar los datos
	{
		if (purb->UrbHeader.Status == USBD_STATUS_SUCCESS)
		{
			ProcesarInputX52(device, purb->UrbBulkOrInterruptTransfer.TransferBuffer);
			*((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer) = 0x01; //Resport id
			purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
		}
	}

    WdfRequestSetInformation(Request, sizeof(HID_INPUT_DATA) + 1);
	WdfRequestComplete(Request, status);
}
#pragma endregion

//DISPATCH
VOID LeerX52ConPedales(PDEVICE_CONTEXT devExt)
{
	// Cancela una de las request enviadas hacia abajo para que se vuelva a hacer la petición
	// con la posición de los pedales actualizada
	WDFREQUEST request = NULL;
	WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
	{
		if (WdfCollectionGetCount(devExt->EntradaX52.ListaRequest) > 0)
		{
			request = (WDFREQUEST)WdfCollectionGetFirstItem(devExt->EntradaX52.ListaRequest);
			WdfObjectReference(request);
			WdfCollectionRemoveItem(devExt->EntradaX52.ListaRequest, 0);
		}
	}
	WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
	if (request != NULL)
	{
		WdfRequestCancelSentRequest(request);
		WdfObjectDereference(request);
	}
}
