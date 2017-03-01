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
	if (!NT_SUCCESS(status)) return status;

	RtlZeroMemory(&GetDeviceContext(device)->EntradaX52.Posicion, sizeof(HID_INPUT_DATA));

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
			if (purb->UrbBulkOrInterruptTransfer.TransferBufferLength >= (sizeof(HID_INPUT_DATA) + 1))
			{
				PDEVICE_CONTEXT devExt = GetDeviceContext(WdfIoQueueGetDevice(Queue));

				WdfRequestFormatRequestUsingCurrentType(Request);
				WdfRequestSetCompletionRoutine(Request, EvtCompletionX52Data, NULL);
				purb->UrbBulkOrInterruptTransfer.TransferBufferLength = 0x0e;
				WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
				{
					WdfCollectionAdd(devExt->EntradaX52.ListaRequest, Request);
				}
				WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
				ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL);
				WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
				{
					if (ret == FALSE)
					{
                        WdfCollectionRemove(devExt->EntradaX52.ListaRequest, Request);
						status = WdfRequestGetStatus(Request);
						WdfRequestComplete(Request, status);
					}
				}
				WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);

				return;
			}
			else
			{
				purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
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
VOID EvtX52IOCtl(
	_In_  WDFQUEUE Queue,
	_In_  WDFREQUEST Request,
	_In_  size_t OutputBufferLength,
	_In_  size_t InputBufferLength,
	_In_  ULONG IoControlCode
)
{
	NTSTATUS	status;
	BOOLEAN		ret;
	PUCHAR		SystemBuffer;
	WDF_REQUEST_SEND_OPTIONS options;

	UNREFERENCED_PARAMETER(OutputBufferLength);

	status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfRequestSetInformation(Request, 0);
		WdfRequestComplete(Request, status);
		return;
	}
	if (IoControlCode != IOCTL_TEXTO)
	{
		if ((IoControlCode != IOCTL_FECHA) && (InputBufferLength != 1))
		{
			WdfRequestSetInformation(Request, 1);
			WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
			return;
		}
		else if ((IoControlCode == IOCTL_FECHA) && (InputBufferLength != 2))
		{
			WdfRequestSetInformation(Request, 2);
			WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
			return;
		}
	}

	switch (IoControlCode) //Viene del minidrive donde se env�a en PASSIVE_LEVEL
	{
	case IOCTL_MFD_LUZ:
	{
		status = Luz_MFD(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_GLOBAL_LUZ:
	{
		status = Luz_Global(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_INFO_LUZ:
	{
		status = Luz_Info(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_PINKIE:
	{
		status = Set_Pinkie(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_TEXTO:
	{
		status = Set_Texto(WdfIoQueueGetDevice(Queue), SystemBuffer, InputBufferLength);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_HORA:
	{
		status = Set_Hora(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_HORA24:
	{
		status = Set_Hora24(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_FECHA:
	{
		status = Set_Fecha(WdfIoQueueGetDevice(Queue), SystemBuffer);
		if (NT_SUCCESS(status)) WdfRequestSetInformation(Request, 2);
		WdfRequestComplete(Request, status);
		return;
	}
	case IOCTL_PEDALES:
	{
		GetDeviceContext(WdfIoQueueGetDevice(Queue))->Pedales.Activado = (BOOLEAN)*SystemBuffer;
		WdfRequestSetInformation(Request, 1);
		WdfRequestComplete(Request, STATUS_SUCCESS);
		return;
	}
	default:
		break;
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
	{
		if (purb->UrbHeader.Status == USBD_STATUS_CANCELED)
		{
			INT16 posPedales = 512;
			if (GetDeviceContext(device)->Pedales.Activado)
			{
				WdfSpinLockAcquire(GetDeviceContext(device)->Pedales.SpinLockPosicion);
					posPedales = GetDeviceContext(device)->Pedales.Posicion;
				WdfSpinLockRelease(GetDeviceContext(device)->Pedales.SpinLockPosicion);
			}
			WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
			{
				if (GetDeviceContext(device)->Pedales.Activado)
				{
					GetDeviceContext(device)->EntradaX52.Posicion.Ejes[5] = posPedales >> 8;
					GetDeviceContext(device)->EntradaX52.Posicion.Ejes[4] = posPedales & 0xff;
				}
				RtlCopyMemory((PVOID)((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer + 1), &GetDeviceContext(device)->EntradaX52.Posicion, sizeof(HID_INPUT_DATA));
			}
			WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);

			purb->UrbHeader.Status = USBD_STATUS_SUCCESS;
			*((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer) = 0x01; //Resport id
			purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
			status = STATUS_SUCCESS;
		}
	}
	else if (NT_SUCCESS(status) && !cancelada)
	{
		if (purb->UrbHeader.Status == USBD_STATUS_SUCCESS)
		{
			ConvertirInputX52(device, purb->UrbBulkOrInterruptTransfer.TransferBuffer);
			*((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer) = 0x01; //Resport id
			purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
		}
	}

	WdfRequestComplete(Request, status);
}
#pragma endregion

#pragma region "Leer datos"
//DISPATCH
UCHAR Switch4To8(UCHAR in)
{
	switch (in)
	{
	case 0: return 0;
	case 1: return 1;
	case 2: return 3;
	case 3: return 2;
	case 4: return 5;
	case 6: return 4;
	case 8: return 7;
	case 9: return 8;
	case 12: return 6;
	default: return 0;
	}
}

//DISPATCH
VOID ConvertirInputX52(WDFDEVICE device, PVOID inputData)
{
	HID_INPUT_DATA hidData;
	RtlZeroMemory(&hidData, sizeof(HID_INPUT_DATA));

	PHIDX52_INPUT_DATA hidGameData = (PHIDX52_INPUT_DATA)inputData;
	hidData.Ejes[0] = hidGameData->EjesXYR[0];
	hidData.Ejes[1] = hidGameData->EjesXYR[1] & 0x7;
	hidData.Ejes[2] = (hidGameData->EjesXYR[1] >> 3) | ((hidGameData->EjesXYR[2] & 0x7) << 5);
	hidData.Ejes[3] = (hidGameData->EjesXYR[2] >> 3) & 0x7;
	hidData.Ejes[4] = (hidGameData->EjesXYR[2] >> 6) | ((hidGameData->EjesXYR[3] & 0x3f) << 2);
	hidData.Ejes[5] = hidGameData->EjesXYR[3] >> 6;
	hidData.Ejes[6] = 255 - hidGameData->Ejes[0]; //Z
	hidData.Ejes[8] = hidGameData->Ejes[2];
	hidData.Ejes[10] = hidGameData->Ejes[1];
	hidData.Ejes[12] = hidGameData->Ejes[3];
	hidData.Botones[0] = ((hidGameData->Botones[1] >> 6) & 1) | ((hidGameData->Botones[0] >> 1) & 6) | ((hidGameData->Botones[0] << 2) & 8) | ((hidGameData->Botones[0] >> 2) & 16) | ((hidGameData->Botones[3] >> 1) & 32) | ((hidGameData->Botones[0] << 1) & 64) | ((hidGameData->Botones[0] << 3) & 128);
	hidData.Botones[1] = ((hidGameData->Botones[2] & 0x80) >> 7) | ((hidGameData->Botones[3] << 1) & 126) | (hidGameData->Botones[0] & 128);
	hidData.Botones[2] = (hidGameData->Botones[1] & 0x3f) | ((hidGameData->Botones[0] & 1) << 6) | (hidGameData->Botones[3] & 0x80);
	hidData.Botones[3] = hidGameData->Seta & 0x3;
	hidData.Setas[0] = hidGameData->Seta >> 4;
	hidData.Setas[1] = Switch4To8((hidGameData->Botones[1] >> 7) + ((hidGameData->Botones[2] << 1) & 0xf));
	hidData.Setas[2] = Switch4To8((hidGameData->Botones[2] >> 3) & 0xf);
	switch (hidGameData->Ministick & 0xf)
	{
	case 0:
		hidData.Setas[3] = 8;
		break;
	case 0xf:
		hidData.Setas[3] = 2;
		break;
	default: hidData.Setas[3] = 0;
	}
	switch (hidGameData->Ministick >> 4)
	{
	case 0:
		hidData.Setas[3] |= 1;
		break;
	case 0xf:
		hidData.Setas[3] |= 4;
		break;
	}
	hidData.Setas[3] = Switch4To8(hidData.Setas[3]);
	hidData.MiniStick = hidGameData->Ministick;

	if (GetDeviceContext(device)->Pedales.Activado)
	{
		INT16 posPedales;
		WdfSpinLockAcquire(GetDeviceContext(device)->Pedales.SpinLockPosicion);
			posPedales = GetDeviceContext(device)->Pedales.Posicion;
		WdfSpinLockRelease(GetDeviceContext(device)->Pedales.SpinLockPosicion);
		hidData.Ejes[4] = posPedales >> 8;
		hidData.Ejes[5] = posPedales & 0xff;
	}

	RtlCopyMemory((PVOID)((PUCHAR)inputData + 1), &hidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
		RtlCopyMemory(&GetDeviceContext(device)->EntradaX52.Posicion, &hidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);

}

//DISPATCH
VOID LeerX52ConPedales(PDEVICE_CONTEXT devExt)
{
	WDFREQUEST request = NULL;
	WdfSpinLockAcquire(devExt->EntradaX52.SpinLockRequest);
	{
		if (WdfCollectionGetCount(devExt->EntradaX52.ListaRequest) > 0)
		{
			request = (WDFREQUEST)WdfCollectionGetFirstItem(devExt->EntradaX52.ListaRequest);
			WdfCollectionRemoveItem(devExt->EntradaX52.ListaRequest, 0);
		}
	}
	WdfSpinLockRelease(devExt->EntradaX52.SpinLockRequest);
	if (request != NULL)
		WdfRequestCancelSentRequest(request);
}
#pragma endregion