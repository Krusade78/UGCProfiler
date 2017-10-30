#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include <hidport.h>
#include <InitGuid.h>
#include "reports.h"

#define _IOCTL_
#include "ioctl.h"
#undef _IOCTL_

VOID
HF_InternIoCtl(
			  __in  WDFQUEUE Queue,
			  __in  WDFREQUEST Request,
			  __in  size_t OutputBufferLength,
			  __in  size_t InputBufferLength,
			  __in  ULONG IoControlCode
			)
{
	NTSTATUS			status	= STATUS_SUCCESS;
	WDFDEVICE           device;
    PDEVICE_EXTENSION   devExt	= NULL;

    UNREFERENCED_PARAMETER(OutputBufferLength);
    UNREFERENCED_PARAMETER(InputBufferLength);

    device = WdfIoQueueGetDevice(Queue);
    devExt = GetDeviceExtension(device);

	switch(IoControlCode)
	{
		case IOCTL_HID_GET_DEVICE_DESCRIPTOR:
			status = GetHidDescriptor(Request);
			break;
		case IOCTL_HID_GET_REPORT_DESCRIPTOR:
			status = GetReportDescriptor(Request);
			break;    
		case IOCTL_HID_GET_DEVICE_ATTRIBUTES:
			status = GetDeviceAttributes(Request);
			break;
		case IOCTL_HID_READ_REPORT:
			ProcesarReport(Request);
			return;
		default:
			status = STATUS_NOT_SUPPORTED;
	}

	WdfRequestComplete(Request, status);
}

NTSTATUS
GetHidDescriptor(
    IN WDFREQUEST Request
    )
{
    NTSTATUS            status = STATUS_SUCCESS;
    PHID_DESCRIPTOR		pHidDescriptor;
    WDFMEMORY           memory;
	size_t				buffLength = 0;

    //
    // This IOCTL is METHOD_NEITHER so WdfRequestRetrieveOutputMemory
    // will correctly retrieve buffer from Irp->UserBuffer. 
    // Remember that HIDCLASS provides the buffer in the Irp->UserBuffer
    // field irrespective of the ioctl buffer type. However, framework is very
    // strict about type checking. You cannot get Irp->UserBuffer by using
    // WdfRequestRetrieveOutputMemory if the ioctl is not a METHOD_NEITHER
    // internal ioctl.
    //
    status = WdfRequestRetrieveOutputMemory(Request, &memory);
    if(!NT_SUCCESS(status)) return status;

	pHidDescriptor = (PHID_DESCRIPTOR)WdfMemoryGetBuffer(memory, &buffLength);
	if(buffLength < sizeof(HID_DESCRIPTOR)) return STATUS_BUFFER_TOO_SMALL;

    RtlZeroMemory(pHidDescriptor, sizeof(*pHidDescriptor));

    pHidDescriptor->bLength                         = sizeof(*pHidDescriptor);
    pHidDescriptor->bDescriptorType                 = 0x21;
    pHidDescriptor->bcdHID                          = 0x0100;
    pHidDescriptor->bCountry                        = 0; /*not localized*/
    pHidDescriptor->bNumDescriptors                 = 1;
    pHidDescriptor->DescriptorList[0].bReportType   = 0x22 ;
    pHidDescriptor->DescriptorList[0].wReportLength = sizeof(DefaultReportDescriptor);

    WdfRequestSetInformation(Request, sizeof(*pHidDescriptor));

    return status;
}


NTSTATUS
GetReportDescriptor(
    IN WDFREQUEST Request
    )
{
    NTSTATUS            status = STATUS_SUCCESS;
    WDFMEMORY           memory;

    //
    // This IOCTL is METHOD_NEITHER so WdfRequestRetrieveOutputMemory
    // will correctly retrieve buffer from Irp->UserBuffer. 
    // Remember that HIDCLASS provides the buffer in the Irp->UserBuffer
    // field irrespective of the ioctl buffer type. However, framework is very
    // strict about type checking. You cannot get Irp->UserBuffer by using
    // WdfRequestRetrieveOutputMemory if the ioctl is not a METHOD_NEITHER
    // internal ioctl.
    //
    status = WdfRequestRetrieveOutputMemory(Request, &memory);
    if(!NT_SUCCESS(status)) return status;

	status = WdfMemoryCopyFromBuffer(memory, 0, (PVOID)DefaultReportDescriptor, sizeof(DefaultReportDescriptor));
	if (!NT_SUCCESS(status)) return status;

    WdfRequestSetInformation(Request, sizeof(DefaultReportDescriptor));

    return status;
}


NTSTATUS
GetDeviceAttributes(
    IN WDFREQUEST Request
    )
{
    NTSTATUS				status = STATUS_SUCCESS;
    PHID_DEVICE_ATTRIBUTES  deviceAttributes;
    WDFMEMORY				memory;
	size_t					buffLength = 0;

    //
    // This IOCTL is METHOD_NEITHER so WdfRequestRetrieveOutputMemory
    // will correctly retrieve buffer from Irp->UserBuffer. 
    // Remember that HIDCLASS provides the buffer in the Irp->UserBuffer
    // field irrespective of the ioctl buffer type. However, framework is very
    // strict about type checking. You cannot get Irp->UserBuffer by using
    // WdfRequestRetrieveOutputMemory if the ioctl is not a METHOD_NEITHER
    // internal ioctl.
    //
    status = WdfRequestRetrieveOutputMemory(Request, &memory);
    if(!NT_SUCCESS(status)) return status;

	deviceAttributes = (PHID_DEVICE_ATTRIBUTES)WdfMemoryGetBuffer(memory, &buffLength);
	if(buffLength < sizeof(HID_DEVICE_ATTRIBUTES)) return STATUS_BUFFER_TOO_SMALL;

    RtlZeroMemory( deviceAttributes, sizeof(HID_DEVICE_ATTRIBUTES));
    deviceAttributes->Size = sizeof(HID_DEVICE_ATTRIBUTES);
    deviceAttributes->VendorID = 0x06a3;
    deviceAttributes->ProductID = 0x0001;
    deviceAttributes->VersionNumber = 0x0200;

    WdfRequestSetInformation(Request, sizeof(HID_DEVICE_ATTRIBUTES));

    return status;
}