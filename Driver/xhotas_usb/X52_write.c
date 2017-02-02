/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

x52_write.c

Abstract:

Escritura de datos al USB del X52.

Environment:

User-mode Driver Framework 2

--*/
#define INITGUID

#include <Windows.h>
#include <wdf.h>
#include <usbdi.h>
#include <wdfusb.h>
#include "Context.h"
#define _PRIVATE_
#include "x52_write.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, Luz_MFD)
#pragma alloc_text(PAGE, Luz_Global)
#pragma alloc_text(PAGE, Luz_Info)
#pragma alloc_text(PAGE, Set_Pinkie)
#pragma alloc_text(PAGE, Set_Texto)
#pragma alloc_text(PAGE, Set_Hora)
#pragma alloc_text(PAGE, Set_Hora24)
#pragma alloc_text(PAGE, Set_Fecha)
#pragma alloc_text(PAGE, EnviarOrden)
#endif

//PASSIVE_LEVEL
NTSTATUS Luz_MFD(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	params[0] = *(SystemBuffer); params[1] = 0;
	params[2] = 0xb1;
	return EnviarOrden(DeviceObject, params);
}

//PASSIVE_LEVEL
NTSTATUS Luz_Global(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	params[0] = *(SystemBuffer); params[1] = 0;
	params[2] = 0xb2;
	return EnviarOrden(DeviceObject, params);
}

//PASSIVE_LEVEL
NTSTATUS Luz_Info(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	params[0] = *(SystemBuffer)+0x50; params[1] = 0;
	params[2] = 0xb4;
	return EnviarOrden(DeviceObject, params);
}

//PASSIVE_LEVEL
NTSTATUS Set_Pinkie(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	params[0] = *(SystemBuffer)+0x50; params[1] = 0;
	params[2] = 0xfd;
	return EnviarOrden(DeviceObject, params);
}

//PASSIVE_LEVEL
NTSTATUS Set_Texto(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer, _In_ size_t tamBuffer)
{
	NTSTATUS status = STATUS_SUCCESS;
	CHAR i = 0;
	UCHAR params[3];
	UCHAR texto[17];

	PAGED_CODE();

	if ((tamBuffer - 1) > 16)
		return STATUS_INVALID_BUFFER_SIZE;

	RtlZeroMemory(texto, 17);
	if ((tamBuffer - 1) <= 16)
		RtlCopyMemory(texto, &(SystemBuffer)[1], tamBuffer - 1);


	params[0] = 0x0; params[1] = 0;
	switch (*(SystemBuffer)) //linea
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

	if (NT_SUCCESS(status))
	{
		switch (*(SystemBuffer)) //linea
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
		for (i = 0; i < 17; i += 2)
		{
			if (texto[i] == 0)
				break;
			params[0] = texto[i];
			params[1] = texto[i + 1];
			status = EnviarOrden(DeviceObject, params);
			if (!NT_SUCCESS(status))
				break;
		}
	}

	return status;
}

//PASSIVE_LEVEL
NTSTATUS Set_Hora(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	params[0] = (SystemBuffer)[2];
	params[1] = (SystemBuffer)[1];
	params[2] = *(SystemBuffer)+0xbf;
	return EnviarOrden(DeviceObject, params);
}

//PASSIVE_LEVEL
NTSTATUS Set_Hora24(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	params[0] = (SystemBuffer)[2];
	params[1] = (SystemBuffer)[1] + 0x80;
	params[2] = *(SystemBuffer)+0xbf;
	return EnviarOrden(DeviceObject, params);
}

//PASSIVE_LEVEL
NTSTATUS Set_Fecha(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer)
{
	UCHAR params[3];

	PAGED_CODE();

	switch (SystemBuffer[0]) {
	case 1:
		params[2] = 0xc4;
		params[1] = (UCHAR)(GetDeviceContext(DeviceObject)->SalidaX52.fecha >> 8);
		params[0] = SystemBuffer[1];
		GetDeviceContext(DeviceObject)->SalidaX52.fecha = *((USHORT*)params);
		break;
	case 2:
		params[2] = 0xc4;
		params[1] = SystemBuffer[1];
		params[0] = (UCHAR)(GetDeviceContext(DeviceObject)->SalidaX52.fecha & 0xff);
		GetDeviceContext(DeviceObject)->SalidaX52.fecha = *((USHORT*)params);
		break;
	case 3:
		params[2] = 0xc8;
		params[1] = 0;
		params[0] = SystemBuffer[1];
	}

	return EnviarOrden(DeviceObject, params);
}

//PASSIVE
NTSTATUS EnviarOrden(_In_ WDFDEVICE DeviceObject, _In_ UCHAR* params)
{
	NTSTATUS                        status;
	WDF_USB_CONTROL_SETUP_PACKET    controlSetupPacket;
	WDF_REQUEST_SEND_OPTIONS		sendOptions;
	WDF_OBJECT_ATTRIBUTES			attributes;
	WDFREQUEST						newRequest = NULL;

	PAGED_CODE();

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = DeviceObject;
	status = WdfRequestCreate(&attributes, WdfDeviceGetIoTarget(DeviceObject), &newRequest);
	if (!NT_SUCCESS(status))
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
		(WDFUSBDEVICE)DeviceObject,
		newRequest, // Optional WDFREQUEST
		&sendOptions, // PWDF_REQUEST_SEND_OPTIONS
		&controlSetupPacket,
		NULL,
		NULL);

	WdfObjectDelete(newRequest);

	return status;
}