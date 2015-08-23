/*++

Copyright (c) 2012 Alfredo Costalago
Module Name:

    hidfilter.c

Abstract: Filtro para - Human Interface Device (HID) USB driver

Environment:

    Kernel mode

--*/

#include <ntddk.h>
#include <wdf.h>
#include <usbdi.h>
#include <wdfusb.h>
#include "hidfilter.h"

#define _CONTROL_
#include "control.h"
#undef _CONTROL_

#ifdef ALLOC_PRAGMA
    #pragma alloc_text(PAGE, HF_Control)
    #pragma alloc_text(PAGE, SetFecha)
    #pragma alloc_text(PAGE, SetLinea)
    #pragma alloc_text(PAGE, EnviarOrden)
#endif

#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0107, METHOD_BUFFERED, FILE_WRITE_ACCESS)

VOID
HF_Control(
			__in  WDFQUEUE Queue,
			__in  WDFREQUEST Request,
			__in  size_t OutputBufferLength,
			__in  size_t InputBufferLength,
			__in  ULONG IoControlCode
			)
{
	NTSTATUS			status	= STATUS_SUCCESS;
	PUCHAR				SystemBuffer;		

    UNREFERENCED_PARAMETER(OutputBufferLength);

	PAGED_CODE();

    WdfRequestSetInformation(Request, 0);
	status = WdfRequestRetrieveInputBuffer(Request, 0, &SystemBuffer, NULL);
	if(!NT_SUCCESS(status))
	{
		WdfRequestComplete(Request, status);
		return;
	}

	switch(IoControlCode)
	{
		case IOCTL_MFD_LUZ:
			{
				UCHAR params[3];
				params[0] = *(SystemBuffer); params[1] = 0;
				params[2] = 0xb1;
				status = EnviarOrden(WdfIoQueueGetDevice(Queue), params);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_GLOBAL_LUZ:
			{
				UCHAR params[3];
				params[0] = *(SystemBuffer); params[1] = 0;
				params[2] = 0xb2;
				status = EnviarOrden(WdfIoQueueGetDevice(Queue),params);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_INFO_LUZ:
			{
				UCHAR params[3];
				params[0] = *(SystemBuffer )+ 0x50; params[1] = 0;
				params[2] = 0xb4;
				status = EnviarOrden(WdfIoQueueGetDevice(Queue), params);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_PINKIE:
			{
				UCHAR params[3];
				params[0] = *(SystemBuffer) + 0x50; params[1] = 0;
				params[2] = 0xfd;
				status = EnviarOrden(WdfIoQueueGetDevice(Queue), params);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_TEXTO:
			{
				UCHAR texto[17];
				RtlZeroMemory(texto, 17);
				if((InputBufferLength - 1) <= 16)
					RtlCopyMemory(texto, &(SystemBuffer)[1], InputBufferLength - 1);
				else
					RtlCopyMemory(texto, &(SystemBuffer)[1], 16);

				status = SetLinea(WdfIoQueueGetDevice(Queue), *(SystemBuffer), texto);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
				break;
			}
		case IOCTL_HORA:
			{
				UCHAR params[3];
				params[0] = (SystemBuffer)[2];
				params[1] = (SystemBuffer)[1];
				params[2] = *(SystemBuffer) + 0xbf;
				status = EnviarOrden(WdfIoQueueGetDevice(Queue), params);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_HORA24:
			{
				UCHAR params[3];
				params[0] = (SystemBuffer)[2];
				params[1] = (SystemBuffer)[1] + 0x80;
				params[2] = *(SystemBuffer) + 0xbf;
				status = EnviarOrden(WdfIoQueueGetDevice(Queue), params);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
				break;
			}
		case IOCTL_FECHA:
			{
				status = SetFecha(WdfIoQueueGetDevice(Queue), SystemBuffer);
				if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 2);
				break;
			}
		default:
			status = STATUS_NOT_SUPPORTED;
	}

	WdfRequestComplete(Request, status);
}

NTSTATUS SetFecha(IN WDFDEVICE device, IN UCHAR* datos)
{
	PDEVICE_EXTENSION itfdevExt= GetControlExtension(device)->devExt;
	UCHAR params[3];

	PAGED_CODE();

	switch(datos[0]) {
		case 1:
			params[2]=0xc4;
			params[1]=(UCHAR)(itfdevExt->fecha>>8);
			params[0]=datos[1];
			itfdevExt->fecha=*((USHORT*)params);
			break;
		case 2:
			params[2]=0xc4;
			params[1]=datos[1];
			params[0]=(UCHAR)(itfdevExt->fecha&0xff);
			itfdevExt->fecha=*((USHORT*)params);
			break;
		case 3:
			params[2]=0xc8;
			params[1]=0;
			params[0]=datos[1];
	}

	return EnviarOrden(device, params);
}

NTSTATUS SetLinea(IN WDFDEVICE DeviceObject, IN CHAR linea, IN UCHAR* texto)
{
    NTSTATUS status = STATUS_SUCCESS;
	CHAR i = 0;
	UCHAR params[3];

	PAGED_CODE();

	params[0] = 0x0; params[1] = 0;
	switch(linea)
	{
		case 1:
			params[2] = 0xd9;
			break;
		case 2:
			params[2] = 0xda;
			break;
		case 3:
			params[2] = 0xdc;
	}
	status = EnviarOrden(DeviceObject, params);

	if(NT_SUCCESS(status))
	{
		switch(linea)
		{
			case 1:
				params[2] = 0xd1;
				break;
			case 2:
				params[2] = 0xd2;
				break;
			case 3:
				params[2] = 0xd4;
		}
		for(i = 0; i < 17; i += 2)
		{
			if(texto[i] == 0)
				break;
			params[0] = texto[i];
			params[1] = texto[i + 1];
			status =EnviarOrden(DeviceObject, params);
			if(!NT_SUCCESS(status))
				break;
		}
	}

	return status;
}

NTSTATUS EnviarOrden(IN WDFDEVICE DeviceObject,	UCHAR* params)
{
    NTSTATUS                        status = STATUS_SUCCESS;
	WDF_USB_CONTROL_SETUP_PACKET    controlSetupPacket;
	WDF_REQUEST_SEND_OPTIONS		sendOptions;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDFREQUEST						newRequest = NULL;

	PAGED_CODE();

	if(GetControlExtension(DeviceObject)->devExt->UsbDevice == NULL)
		return STATUS_DEVICE_NOT_CONNECTED;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = GetControlExtension(DeviceObject)->devExt->UsbDevice;
	status = WdfRequestCreate(&attributes, WdfUsbTargetDeviceGetIoTarget(GetControlExtension(DeviceObject)->devExt->UsbDevice), &newRequest);
	if(!NT_SUCCESS(status))
		return status;

	WDF_USB_CONTROL_SETUP_PACKET_INIT_VENDOR(&controlSetupPacket,
                                BmRequestHostToDevice,
                                BmRequestToDevice,
                                0x91, // Request
                                *(USHORT*)params, // Value
                                params[2]); // Index  

	WDF_REQUEST_SEND_OPTIONS_INIT(&sendOptions, WDF_REQUEST_SEND_OPTION_TIMEOUT);
    WDF_REQUEST_SEND_OPTIONS_SET_TIMEOUT(&sendOptions, WDF_REL_TIMEOUT_IN_MS(500));

    status = WdfUsbTargetDeviceSendControlTransferSynchronously(
                                        GetControlExtension(DeviceObject)->devExt->UsbDevice, 
                                        newRequest, // Optional WDFREQUEST
                                        &sendOptions, // PWDF_REQUEST_SEND_OPTIONS
                                        &controlSetupPacket,
                                        NULL,
                                        NULL);

	WdfObjectDelete(newRequest);

	return status;
}