/*++

Copyright (c) 2015 Alfredo Costalago
Module Name:

salida_x52.c

Abstract: Filtro para - Human Interface Device (HID) USB driver

Environment:

Kernel mode

--*/

#define _PRIVATE_
#include "salida_x52.h"
#undef _PRIVATE_
//#include <usbdi.h>
//#include <wdfusb.h>
//#include <usb.h>
//#include <usbioctl.h>
//#include <hidport.h>
//#include "extensions.h"

#ifdef ALLOC_PRAGMA
	//#pragma alloc_text(PAGE, Set_Texto)
	//#pragma alloc_text(PAGE, Set_Fecha)
	//#pragma alloc_text(PAGE, EnviarOrden)
#endif

//NTSTATUS Luz_MFD(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	UCHAR params[3];
//	params[0] = *(SystemBuffer); params[1] = 0;
//	params[2] = 0xb1;
//	return EnviarOrden(DeviceObject, params);
//}
//
//NTSTATUS Luz_Global(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	UCHAR params[3];
//	params[0] = *(SystemBuffer); params[1] = 0;
//	params[2] = 0xb2;
//	return EnviarOrden(DeviceObject, params);
//}
//
//NTSTATUS Luz_Info(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	UCHAR params[3];
//	params[0] = *(SystemBuffer)+0x50; params[1] = 0;
//	params[2] = 0xb4;
//	return EnviarOrden(DeviceObject, params);
//}
//
//NTSTATUS Set_Pinkie(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	UCHAR params[3];
//	params[0] = *(SystemBuffer)+0x50; params[1] = 0;
//	params[2] = 0xfd;
//	return EnviarOrden(DeviceObject, params);
//}
//
//NTSTATUS Set_Texto(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer, __in size_t tamBuffer)
//{
//	NTSTATUS status = STATUS_SUCCESS;
//	CHAR i = 0;
//	UCHAR params[3];
//	UCHAR texto[17];
//
//	PAGED_CODE();
//
//	RtlZeroMemory(texto, 17);
//	if ((tamBuffer - 1) <= 16)
//		RtlCopyMemory(texto, &(SystemBuffer)[1], tamBuffer - 1);
//	else
//		RtlCopyMemory(texto, &(SystemBuffer)[1], 16);
//
//	params[0] = 0x0; params[1] = 0;
//	switch(*(SystemBuffer)) //linea
//	{
//		case 1:
//			params[2] = 0xd9;
//			break;
//		case 2:
//			params[2] = 0xda;
//			break;
//		case 3:
//			params[2] = 0xdc;
//	}
//	status = EnviarOrden(DeviceObject, params);
//	
//	if(NT_SUCCESS(status))
//	{
//		switch(*(SystemBuffer)) //linea
//		{
//			case 1:
//				params[2] = 0xd1;
//				break;
//			case 2:
//				params[2] = 0xd2;
//				break;
//			case 3:
//				params[2] = 0xd4;
//		}
//		for(i = 0; i < 17; i += 2)
//		{
//			if(texto[i] == 0)
//				break;
//			params[0] = texto[i];
//			params[1] = texto[i + 1];
//			status =EnviarOrden(DeviceObject, params);
//			if(!NT_SUCCESS(status))
//				break;
//		}
//	}
//	
//	return status;
//}
//
//NTSTATUS Set_Hora(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	UCHAR params[3];
//	params[0] = (SystemBuffer)[2];
//	params[1] = (SystemBuffer)[1];
//	params[2] = *(SystemBuffer) + 0xbf;
//	return EnviarOrden(DeviceObject, params);
//}
//
//NTSTATUS Set_Hora24(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	UCHAR params[3];
//	params[0] = (SystemBuffer)[2];
//	params[1] = (SystemBuffer)[1] + 0x80;
//	params[2] = *(SystemBuffer) + 0xbf;
//	return EnviarOrden(DeviceObject, params);
//}
//
//NTSTATUS Set_Fecha(__in WDFDEVICE DeviceObject, __in PUCHAR SystemBuffer)
//{
//	PDEVICE_EXTENSION itfdevExt= GetControlExtension(DeviceObject)->devExt;
//	UCHAR params[3];
//
//	PAGED_CODE();
//
//	switch(SystemBuffer[0]) {
//		case 1:
//			params[2]=0xc4;
//			params[1]=(UCHAR)(itfdevExt->fecha >> 8);
//			params[0]= SystemBuffer[1];
//			itfdevExt->fecha=*((USHORT*)params);
//			break;
//		case 2:
//			params[2]=0xc4;
//			params[1]= SystemBuffer[1];
//			params[0]=(UCHAR)(itfdevExt->fecha & 0xff);
//			itfdevExt->fecha=*((USHORT*)params);
//			break;
//		case 3:
//			params[2]=0xc8;
//			params[1]=0;
//			params[0]= SystemBuffer[1];
//	}
//
//	return EnviarOrden(DeviceObject, params);
//}

//NTSTATUS EnviarOrden(__in WDFDEVICE DeviceObject, __in UCHAR* params)
//{
//    NTSTATUS                        status = STATUS_SUCCESS;
////	WDF_USB_CONTROL_SETUP_PACKET    controlSetupPacket;
////	WDF_REQUEST_SEND_OPTIONS		sendOptions;
////	WDF_OBJECT_ATTRIBUTES			attributes;
////	WDFREQUEST						newRequest = NULL;
////
//	PAGED_CODE();
////
////	if(GetControlExtension(DeviceObject)->devExt->UsbDevice == NULL)
////		return STATUS_DEVICE_NOT_CONNECTED;
////
////	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
////		attributes.ParentObject = GetControlExtension(DeviceObject)->devExt->UsbDevice;
////	status = WdfRequestCreate(&attributes, WdfUsbTargetDeviceGetIoTarget(GetControlExtension(DeviceObject)->devExt->UsbDevice), &newRequest);
////	if(!NT_SUCCESS(status))
////		return status;
////
////	WDF_USB_CONTROL_SETUP_PACKET_INIT_VENDOR(&controlSetupPacket,
////                                BmRequestHostToDevice,
////                                BmRequestToDevice,
////                                0x91, // Request
////                                *(USHORT*)params, // Value
////                                params[2]); // Index  
////
////	WDF_REQUEST_SEND_OPTIONS_INIT(&sendOptions, WDF_REQUEST_SEND_OPTION_TIMEOUT);
////    WDF_REQUEST_SEND_OPTIONS_SET_TIMEOUT(&sendOptions, WDF_REL_TIMEOUT_IN_MS(500));
////
////    status = WdfUsbTargetDeviceSendControlTransferSynchronously(
////                                        GetControlExtension(DeviceObject)->devExt->UsbDevice, 
////                                        newRequest, // Optional WDFREQUEST
////                                        &sendOptions, // PWDF_REQUEST_SEND_OPTIONS
////                                        &controlSetupPacket,
////                                        NULL,
////                                        NULL);
////
////	WdfObjectDelete(newRequest);
////
//	return status;
//}