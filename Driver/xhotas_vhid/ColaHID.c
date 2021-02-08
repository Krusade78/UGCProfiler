/*++

Copyright (c) 2021 Alfredo Costalago

Module Name:

    ColaHid.c

Abstract:

    This module contains the implementation of the queue

--*/
#include <ntddk.h>
#include <wdf.h>
#include <vhf.h>
#include "Driver.h"
#include "ColaHID.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, CrearColaHID)
#endif // ALLOC_PRAGMA

NTSTATUS CrearColaHID(_In_ WDFDEVICE Device, _Out_ WDFQUEUE	*Queue)
{
    NTSTATUS                status;
    WDF_IO_QUEUE_CONFIG     queueConfig;
    WDFQUEUE                queue;

    PAGED_CODE();

    WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&queueConfig, WdfIoQueueDispatchSequential);
        queueConfig.EvtIoWrite = EvtIoWriteHID;

    status = WdfIoQueueCreate(Device, &queueConfig, WDF_NO_OBJECT_ATTRIBUTES, &queue);
    if (!NT_SUCCESS(status)) 
    {
        return status;
    }

    *Queue = queue;
    return status;
}


VOID EvtIoWriteHID(_In_ WDFQUEUE Queue, _In_ WDFREQUEST Request, _In_ size_t Length)
{
    WDFMEMORY memory;
    void *pvoid;
    size_t length;
    NTSTATUS status;

    if (Length < 5) 
    {
        WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
        return;
    }

    status = WdfRequestRetrieveInputMemory(Request, &memory);
    if (!NT_SUCCESS(status)) 
    {
        WdfRequestComplete(Request, status);
        return;
    }
    pvoid = WdfMemoryGetBuffer(memory, &length);
    if (pvoid == NULL)
    {
        WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
        return;
    }

    // 
    // Complete the input IRP if we have one
    //
    VhfSubmitReadReport(WdfIoQueueGetDevice(Queue), pvoid, (ULONG)Length);

    //
    // set status and information
    //
    WdfRequestCompleteWithInformation(Request, STATUS_SUCCESS, Length);
}


VOID VhfSubmitReadReport(_In_ WDFDEVICE Device,_In_ PUCHAR Report, _In_ ULONG ReportSize)
{
    HID_XFER_PACKET transferPacket;

    transferPacket.reportId = *Report;
    transferPacket.reportBufferLen = ReportSize;
    transferPacket.reportBuffer = Report;

    VhfReadReportSubmit(GetDeviceContext(Device)->VhfHandle, &transferPacket);
}


