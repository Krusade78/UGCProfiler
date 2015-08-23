#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "x52.h"

#define _X52_
#include "x52.h"
#undef _X52_

#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0107, METHOD_BUFFERED, FILE_WRITE_ACCESS)

VOID EnviarOrdenX52(PDEVICE_EXTENSION devExt, UCHAR tipo, PVOID SystemBuffer, UCHAR tam)
{
	UINT32					ctlCode;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFREQUEST				newRequest = NULL;
	WDFMEMORY				writeBufferMemHandle;
	WDFMEMORY_OFFSET		offset;
	NTSTATUS				status;	
	PVOID					buffer;

	if(!devExt->TargetUSBX52)
		return;

	switch(tipo)
	{
		case 0:
			ctlCode = IOCTL_MFD_LUZ;
			break;
		case 1:
			ctlCode = IOCTL_GLOBAL_LUZ;
			break;
		case 2:
			ctlCode = IOCTL_INFO_LUZ;
			break;
		case 3:
			ctlCode = IOCTL_PINKIE;
			break;
		case 4:
			ctlCode = IOCTL_TEXTO;
			break;
		case 5:
			ctlCode = IOCTL_HORA;
			break;
		case 6:
			ctlCode = IOCTL_HORA24;
			break;
		case 7:
			ctlCode = IOCTL_FECHA;
			break;
		default:
			ctlCode = IOCTL_TEXTO;
	}

	offset.BufferOffset = 0;
	offset.BufferLength = tam;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = devExt->TargetUSBX52;

	status = WdfRequestCreate(&attributes, devExt->TargetUSBX52, &newRequest);
	if(!NT_SUCCESS(status))
		return;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = newRequest;
	status = WdfMemoryCreate(&attributes, NonPagedPool, 0,  tam, &writeBufferMemHandle, &buffer);
	if(!NT_SUCCESS(status))
	{
		WdfObjectDelete(newRequest);
		return;
	}

	RtlCopyMemory(buffer, SystemBuffer, tam);

	status = WdfIoTargetFormatRequestForIoctl(devExt->TargetUSBX52, newRequest, ctlCode, writeBufferMemHandle, &offset, NULL, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(writeBufferMemHandle);
		WdfObjectDelete(newRequest);
		return;
	}

	WdfRequestSetCompletionRoutine(newRequest, CompletionX52, NULL);

	if(WdfRequestSend(newRequest, devExt->TargetUSBX52, NULL) == FALSE)
	{
		WdfObjectDelete(writeBufferMemHandle);
		WdfObjectDelete(newRequest);
	}
}

VOID CompletionX52(
    IN WDFREQUEST  Request,
    IN WDFIOTARGET  Target,
    IN PWDF_REQUEST_COMPLETION_PARAMS  Params,
    IN WDFCONTEXT  Context
    )
{
	UNREFERENCED_PARAMETER(Target);
	UNREFERENCED_PARAMETER(Params);
	UNREFERENCED_PARAMETER(Context);

	WdfObjectDelete(Request);
}