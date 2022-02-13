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
#include "ColaVHF.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, CrearColaHID)
#endif // ALLOC_PRAGMA

NTSTATUS CrearColaHID(_In_ WDFDEVICE Device, _Out_ WDFQUEUE	*Queue)
{
    NTSTATUS                status;
    WDF_IO_QUEUE_CONFIG     queueConfig;
    WDF_OBJECT_ATTRIBUTES   attributes;
    WDFQUEUE                queue;

    PAGED_CODE();

    WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
    attributes.ExecutionLevel = WdfExecutionLevelPassive;

    WDF_IO_QUEUE_CONFIG_INIT_DEFAULT_QUEUE(&queueConfig, WdfIoQueueDispatchSequential);
        queueConfig.EvtIoWrite = EvtIoWriteHID;

    status = WdfIoQueueCreate(Device, &queueConfig, &attributes, &queue);
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

    if (Length < 1) 
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

    if (*(PUCHAR)pvoid == 0)
    {
        EvtIoWriteVHF(Queue, Request, Length);
    }
    else
    {
        // 
        // Complete the input IRP if we have one
        //
        status = VhfSubmitReadReport(WdfIoQueueGetDevice(Queue), pvoid, (ULONG)Length);

        //
        // set status and information
        //
        WdfRequestCompleteWithInformation(Request, status, Length);
    }
}


NTSTATUS VhfSubmitReadReport(_In_ WDFDEVICE Device,_In_ PUCHAR Report, _In_ ULONG ReportSize)
{
    HID_XFER_PACKET transferPacket;
    NTSTATUS status;

    transferPacket.reportId = *Report;
    transferPacket.reportBufferLen = ReportSize;
    transferPacket.reportBuffer = Report;

    WdfSpinLockAcquire(GetDeviceContext(Device)->LockHandle);
    status = VhfReadReportSubmit(GetDeviceContext(Device)->VhfHandle, &transferPacket);
    WdfSpinLockRelease(GetDeviceContext(Device)->LockHandle);
    return status;
}


