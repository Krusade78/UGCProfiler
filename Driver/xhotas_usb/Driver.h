#ifdef _PRIVATE_

DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD EvtAddDevice;
EVT_WDF_DEVICE_PREPARE_HARDWARE  EvtDevicePrepareHardware;
NTSTATUS IniciarContext(WDFDEVICE device);
NTSTATUS IniciarContextX52(_In_ WDFDEVICE device);
NTSTATUS IniciarContextPedales(_In_ WDFDEVICE device);
NTSTATUS IniciarContextHID(_In_ WDFDEVICE device);
EVT_WDF_OBJECT_CONTEXT_CLEANUP EvtCleanupCallback;

#endif _PRIVATE_
