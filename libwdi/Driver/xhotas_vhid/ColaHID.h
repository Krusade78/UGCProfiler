#pragma once

NTSTATUS CrearColaHID(_In_ WDFDEVICE Device, _Out_ WDFQUEUE *Queue);

EVT_WDF_IO_QUEUE_IO_WRITE EvtIoWriteHID;

NTSTATUS VhfSubmitReadReport(_In_ WDFDEVICE Device, _In_ PUCHAR Report, _In_ ULONG ReportSize);

