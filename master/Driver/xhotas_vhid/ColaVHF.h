#pragma once

EVT_WDF_IO_QUEUE_IO_WRITE EvtIoWriteVHF;
NTSTATUS VhfInitialize(WDFDEVICE WdfDevice, VHFHANDLE* handle, PUCHAR ReportDescriptor, size_t* ReportLength);
void VhfLimpiar(WDFDEVICE WdfDevice);