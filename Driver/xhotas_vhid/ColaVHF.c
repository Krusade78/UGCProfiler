/*++

Copyright (c) 2022 Alfredo Costalago

Module Name:

    ColaVhf.c

Abstract:

    This module contains the implementation of the queue

--*/
#include <ntddk.h>
#include <wdf.h>
#include <vhf.h>
#include "Driver.h"
#include "ColaVHF.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, EvtIoWriteVHF)
#pragma alloc_text(PAGE, VhfInitialize)
#pragma alloc_text(PAGE, VhfLimpiar)
#endif // ALLOC_PRAGMA

VOID EvtIoWriteVHF(_In_ WDFQUEUE Queue, _In_ WDFREQUEST Request, _In_ size_t Length)
{
    WDFMEMORY memory;
    UCHAR* pvoid;
    size_t length;
    NTSTATUS status;
    VHFHANDLE handle = NULL;

    PAGED_CODE();

    if (Length <= 1 + sizeof(size_t))
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

    VhfLimpiar(WdfIoQueueGetDevice(Queue));

    pvoid = (UCHAR*)WdfMemoryGetBuffer(memory, &length);
    if (pvoid == NULL)
    {
        WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
        return;
    }

    if (Length != (1 + sizeof(size_t) + *((size_t*)(pvoid + 1))))
    {
        WdfRequestComplete(Request, STATUS_INVALID_BUFFER_SIZE);
        return;
    }

    status = VhfInitialize(WdfIoQueueGetDevice(Queue), &handle, (pvoid + 1 + sizeof(size_t)), (size_t*)(pvoid + 1));
    if (!NT_SUCCESS(status))
    {
        WdfRequestComplete(Request, status);
        return;
    }

    status = VhfStart(handle);
    if (!NT_SUCCESS(status))
    {
        VhfDelete(handle, TRUE);
        WdfRequestComplete(Request, status);
        return;
    }

    WdfSpinLockAcquire(GetDeviceContext(WdfIoQueueGetDevice(Queue))->LockHandle);
    GetDeviceContext(WdfIoQueueGetDevice(Queue))->VhfHandle = handle;
    WdfSpinLockRelease(GetDeviceContext(WdfIoQueueGetDevice(Queue))->LockHandle);

    WdfRequestCompleteWithInformation(Request, STATUS_SUCCESS, Length);
}

NTSTATUS VhfInitialize(WDFDEVICE WdfDevice, VHFHANDLE* handle, PUCHAR ReportDescriptor, size_t* ReportLength)
{
    VHF_CONFIG		vhfConfig;

    PAGED_CODE();

    VHF_CONFIG_INIT(&vhfConfig, WdfDeviceWdmGetDeviceObject(WdfDevice), (USHORT)*ReportLength, ReportDescriptor);
    return VhfCreate(&vhfConfig, handle);
}

void VhfLimpiar(WDFDEVICE WdfDevice)
{
    VHFHANDLE handle = NULL;

    PAGED_CODE();

    WdfSpinLockAcquire(GetDeviceContext(WdfDevice)->LockHandle);
    if (GetDeviceContext(WdfDevice)->VhfHandle != NULL)
    {
        handle = GetDeviceContext(WdfDevice)->VhfHandle;
        GetDeviceContext(WdfDevice)->VhfHandle = NULL;
    }
    WdfSpinLockRelease(GetDeviceContext(WdfDevice)->LockHandle);
    if (handle != NULL)
    {
        VhfDelete(handle, TRUE);
    }
}