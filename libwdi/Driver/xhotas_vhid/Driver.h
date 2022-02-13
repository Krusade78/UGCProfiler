#pragma once


DRIVER_INITIALIZE DriverEntry;
EVT_WDF_DRIVER_DEVICE_ADD EvtAddDevice;
EVT_WDF_DEVICE_SELF_MANAGED_IO_CLEANUP  EvtDeviceSelfManagedIoCleanup;

typedef struct DEVICE_CONTEXT
{
    WDFQUEUE				ColaHID; // Queue for handling requests that come from the rawPdo
    VHFHANDLE               VhfHandle;
    WDFSPINLOCK             LockHandle;
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, GetDeviceContext);

#define DOS_DEVICE_NAME  L"\\DosDevices\\XHOTAS_VHID_Interface"