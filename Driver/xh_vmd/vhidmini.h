#include <wdm.h>
#include <hidport.h>

#define GET_NEXT_DEVICE_OBJECT(DO) \
    (((PHID_DEVICE_EXTENSION)(DO)->DeviceExtension)->NextDeviceObject)
//
// driver routines
//

DRIVER_INITIALIZE DriverEntry;

DRIVER_ADD_DEVICE HidKmdfAddDevice;

DRIVER_DISPATCH HidKmdfPassThrough;

__drv_dispatchType(IRP_MJ_PNP) 
DRIVER_DISPATCH PnP;

__drv_dispatchType(IRP_MJ_POWER) 
DRIVER_DISPATCH HidKmdfPowerPassThrough;

DRIVER_UNLOAD HidKmdfUnload;