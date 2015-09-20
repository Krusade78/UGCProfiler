/*++

Copyright (c) 2015 Alfredo Costalago
Module Name:

ioctl_x52.c

Abstract: Filtro para - Human Interface Device (HID) USB driver

Environment:

Kernel mode

--*/

#define _PRIVATE_
#include "ioctl_x52.h"
#undef _PRIVATE_
#include <usb.h>
#include <usbioctl.h>
#include <hidport.h>
#include "extensions.h"

void HF_X52IOCtl(
		__in  WDFQUEUE Queue,
		__in  WDFREQUEST Request,
		__in  size_t OutputBufferLength,
		__in  size_t InputBufferLength,
		__in  ULONG IoControlCode
		)
{
	NTSTATUS			status = STATUS_SUCCESS;
	WDF_REQUEST_SEND_OPTIONS options;
	BOOLEAN ret;

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
					WdfRequestFormatRequestUsingCurrentType(Request);
					WdfRequestSetCompletionRoutine(Request, CompletionX52Data, NULL);
					purb->UrbBulkOrInterruptTransfer.TransferBufferLength = 0x0e;
					ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), NULL);

					if (ret == FALSE) {
						status = WdfRequestGetStatus(Request);
						WdfRequestComplete(Request, status);
					}
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
					WdfRequestSetCompletionRoutine(Request, CompletionConfigDescriptor, NULL);
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
				if (purb->UrbControlDescriptorRequest.TransferBufferLength >= sizeof(ReportDescriptor))
				{
					PVOID p = purb->UrbControlDescriptorRequest.TransferBuffer;
					memcpy(p, ReportDescriptor, sizeof(ReportDescriptor));
					purb->UrbControlDescriptorRequest.TransferBufferLength = sizeof(ReportDescriptor);
					purb->UrbHeader.Status = USBD_STATUS_SUCCESS;
				}
				else
				{
					purb->UrbControlDescriptorRequest.TransferBufferLength = sizeof(ReportDescriptor);
					purb->UrbHeader.Status = USBD_STATUS_BUFFER_TOO_SMALL;
				}

				WdfRequestComplete(Request, status);
				return;
			}
		}
	}


	WDF_REQUEST_SEND_OPTIONS_INIT(&options, WDF_REQUEST_SEND_OPTION_SEND_AND_FORGET);
	ret = WdfRequestSend(Request, WdfDeviceGetIoTarget(WdfIoQueueGetDevice(Queue)), &options);

	if (ret == FALSE) {
		status = WdfRequestGetStatus(Request);
		WdfRequestComplete(Request, status);
	}
}

void CompletionConfigDescriptor(
		__in WDFREQUEST                     Request,
		__in WDFIOTARGET                    Target,
		__in PWDF_REQUEST_COMPLETION_PARAMS Params,
		__in WDFCONTEXT                     Context
		)
{
	UNREFERENCED_PARAMETER(Target);
	UNREFERENCED_PARAMETER(Context);
	UNREFERENCED_PARAMETER(Params);

	NTSTATUS status = WdfRequestGetStatus(Request);

	if (status == STATUS_SUCCESS)
	{
		WDF_REQUEST_PARAMETERS params;
		PUSB_CONFIGURATION_DESCRIPTOR pconfig;
		PUSB_INTERFACE_DESCRIPTOR pinterface;
		PHID_DESCRIPTOR phid;

		WDF_REQUEST_PARAMETERS_INIT(&params);
		WdfRequestGetParameters(Request, &params);

		if (((PURB)params.Parameters.Others.Arg1)->UrbHeader.Status == USBD_STATUS_SUCCESS)
		{
			if (((PURB)params.Parameters.Others.Arg1)->UrbControlDescriptorRequest.TransferBufferLength != sizeof(USB_CONFIGURATION_DESCRIPTOR))
			{
				pconfig = (PUSB_CONFIGURATION_DESCRIPTOR)((PURB)params.Parameters.Others.Arg1)->UrbControlDescriptorRequest.TransferBuffer;
				pinterface = (PUSB_INTERFACE_DESCRIPTOR)((BYTE*)pconfig + pconfig->bLength);
				phid = (PHID_DESCRIPTOR)((BYTE*)pinterface + pinterface->bLength);
				phid->DescriptorList[0].wReportLength = sizeof(ReportDescriptor);
			}
		}
	}
	WdfRequestComplete(Request, status);
}

void CompletionX52Data(
		__in WDFREQUEST                     Request,
		__in WDFIOTARGET                    Target,
		__in PWDF_REQUEST_COMPLETION_PARAMS Params,
		__in WDFCONTEXT                     Context
		)
{
	UNREFERENCED_PARAMETER(Target);
	UNREFERENCED_PARAMETER(Context);
	UNREFERENCED_PARAMETER(Params);

	NTSTATUS status = WdfRequestGetStatus(Request);

	if (status == STATUS_SUCCESS)
	{
		WDF_REQUEST_PARAMETERS params;
		PURB purb;

		WDF_REQUEST_PARAMETERS_INIT(&params);
		WdfRequestGetParameters(Request, &params);
		purb = (PURB)params.Parameters.Others.Arg1;

		if (purb->UrbHeader.Status == USBD_STATUS_SUCCESS)
		{
			ConvertirX52(purb->UrbBulkOrInterruptTransfer.TransferBuffer);
			*((PUCHAR)purb->UrbBulkOrInterruptTransfer.TransferBuffer) = 0x01; //Resport id
			purb->UrbBulkOrInterruptTransfer.TransferBufferLength = sizeof(HID_INPUT_DATA) + 1;
		}
	}
	WdfRequestComplete(Request, status);
}

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

void ConvertirX52(PVOID inputData)
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
	hidData.Ejes[6] = hidGameData->Ejes[0]; //Z
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

	RtlCopyMemory((PVOID)((PUCHAR)inputData + 1), &hidData, sizeof(HID_INPUT_DATA));
	//if (devExt->PedalesActivados)
	//{
	//	HID_INPUT_DATA ultimoHID;
	//	WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
	//	RtlCopyMemory(&ultimoHID, &devExt->DeltaHidData, sizeof(HID_INPUT_DATA));
	//	WdfSpinLockRelease(devExt->SpinLockDeltaHid);
	//	hidData.Ejes[16] = hidData.Ejes[4];
	//	hidData.Ejes[17] = hidData.Ejes[5];
	//	hidData.Ejes[14] = ultimoHID.Ejes[14];
	//	hidData.Ejes[15] = ultimoHID.Ejes[15];
	//	hidData.Ejes[4] = ultimoHID.Ejes[4];
	//	hidData.Ejes[5] = ultimoHID.Ejes[5];
	//}
}