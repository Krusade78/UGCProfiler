/*++

Copyright (c) 2015 Alfredo Costalago
Module Name:

    ioctl_user.c

Abstract: User mode ioctls

Environment:

    Kernel mode

--*/

#include "ioctl_user.h"
#include "salida_x52.h"


#ifdef ALLOC_PRAGMA
    #pragma alloc_text(PAGE, HF_IoControl)
#endif

#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0107, METHOD_BUFFERED, FILE_WRITE_ACCESS)

void HF_IoControl(
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
	UNREFERENCED_PARAMETER(InputBufferLength);
	UNREFERENCED_PARAMETER(Queue);

	PAGED_CODE();

    WdfRequestSetInformation(Request, 0);
	status = WdfRequestRetrieveInputBuffer(Request, 0, (PVOID*)&SystemBuffer, NULL);
	if(!NT_SUCCESS(status))
	{
		WdfRequestComplete(Request, status);
		return;
	}

	switch(IoControlCode)
	{
		//case IOCTL_MFD_LUZ:
		//	{
		//		status = Luz_MFD(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		//		break;
		//	}
		//case IOCTL_GLOBAL_LUZ:
		//	{
		//		status = Luz_Global(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		//		break;
		//	}
		//case IOCTL_INFO_LUZ:
		//	{
		//		status = Luz_Info(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		//		break;
		//	}
		//case IOCTL_PINKIE:
		//	{
		//		status = Set_Pinkie(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		//		break;
		//	}
		//case IOCTL_TEXTO:
		//	{
		//		status = Set_Texto(WdfIoQueueGetDevice(Queue), SystemBuffer, InputBufferLength);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, InputBufferLength);
		//		break;
		//	}
		//case IOCTL_HORA:
		//	{
		//		status = Set_Hora(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		//		break;
		//	}
		//case IOCTL_HORA24:
		//	{
		//		status = Set_Hora24(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 1);
		//		break;
		//	}
		//case IOCTL_FECHA:
		//	{
		//		status = Set_Fecha(WdfIoQueueGetDevice(Queue), SystemBuffer);
		//		if(NT_SUCCESS(status)) WdfRequestSetInformation(Request, 2);
		//		break;
		//	}
		default:
			status = STATUS_NOT_SUPPORTED;
	}

	WdfRequestComplete(Request, status);
}

