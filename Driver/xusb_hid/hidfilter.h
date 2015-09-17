#include <ntddk.h>
#include <wdf.h>

DRIVER_INITIALIZE DriverEntry;

EVT_WDF_DRIVER_DEVICE_ADD HF_AddDevice;

NTSTATUS IniciarInterfazControl(IN WDFDRIVER Driver, IN WDFDEVICE device);// PDEVICE_EXTENSION devExt)

EVT_WDF_DEVICE_PREPARE_HARDWARE  EvtDevicePrepareHardware;

EVT_WDF_OBJECT_CONTEXT_CLEANUP EvtCleanupCallback;
