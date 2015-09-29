#pragma once
#include <ntddk.h>
#include <wdf.h>


DRIVER_INITIALIZE DriverEntry;

#ifdef _PRIVATE_

EVT_WDF_DRIVER_DEVICE_ADD AddDevice;

NTSTATUS IniciarInterfazControl(_In_ WDFDEVICE device);

EVT_WDF_DEVICE_PREPARE_HARDWARE  DevicePrepareHardware;

EVT_WDF_OBJECT_CONTEXT_CLEANUP CleanupCallback;

#endif
