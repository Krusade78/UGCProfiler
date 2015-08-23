/*++

Copyright (c) 2005 Alfredo Costalago
Module Name:

    vhidmini.c

Environment:

    Kernel framework mode

--*/

#include "VHidMini.h"

#ifdef ALLOC_PRAGMA
#pragma alloc_text( INIT, DriverEntry )
#pragma alloc_text( PAGE, HidKmdfAddDevice)
#pragma alloc_text( PAGE, HidKmdfUnload)
#pragma alloc_text( PAGE, PnP)
#endif

NTSTATUS
DriverEntry (
    __in PDRIVER_OBJECT  DriverObject,
    __in PUNICODE_STRING RegistryPath
    )
/*++

Routine Description:

    Installable driver initialization entry point.
    This entry point is called directly by the I/O system.

Arguments:

    DriverObject - pointer to the driver object

    RegistryPath - pointer to a unicode string representing the path,
                   to driver-specific key in the registry.

Return Value:

    STATUS_SUCCESS if successful,
    STATUS_UNSUCCESSFUL otherwise.

--*/
{
    HID_MINIDRIVER_REGISTRATION hidMinidriverRegistration;
    NTSTATUS status;
    ULONG i;

    //
    // Initialize the dispatch table to pass through all the IRPs.
    //
    for (i = 0; i <= IRP_MJ_MAXIMUM_FUNCTION; i++) {
        DriverObject->MajorFunction[i] = HidKmdfPassThrough;
    }
	DriverObject->MajorFunction[IRP_MJ_PNP]   = PnP;

    //
    // Special case power irps so that we call PoCallDriver instead of IoCallDriver
    // when sending the IRP down the stack.
    //
    DriverObject->MajorFunction[IRP_MJ_POWER] = HidKmdfPowerPassThrough;

    DriverObject->DriverExtension->AddDevice = HidKmdfAddDevice;
    DriverObject->DriverUnload = HidKmdfUnload;

    RtlZeroMemory(&hidMinidriverRegistration,
                  sizeof(hidMinidriverRegistration));

    //
    // Revision must be set to HID_REVISION by the minidriver
    //
    hidMinidriverRegistration.Revision            = HID_REVISION;
    hidMinidriverRegistration.DriverObject        = DriverObject;
    hidMinidriverRegistration.RegistryPath        = RegistryPath;
    hidMinidriverRegistration.DeviceExtensionSize = 0;
    hidMinidriverRegistration.DevicesArePolled = FALSE;

    //
    // Register with hidclass
    //
    status = HidRegisterMinidriver(&hidMinidriverRegistration);

    return status;
}


NTSTATUS
HidKmdfAddDevice(
    __in PDRIVER_OBJECT DriverObject,
    __in PDEVICE_OBJECT FunctionalDeviceObject
    )
/*++

Routine Description:

    HidClass Driver calls our AddDevice routine after creating an FDO for us.
    We do not need to create a device object or attach it to the PDO.
    Hidclass driver will do it for us.

Arguments:

    DriverObject - pointer to the driver object.

    FunctionalDeviceObject -  pointer to the FDO created by the
                            Hidclass driver for us.

Return Value:

    NT status code.

--*/
{
    PAGED_CODE();

    UNREFERENCED_PARAMETER(DriverObject);

    FunctionalDeviceObject->Flags &= ~DO_DEVICE_INITIALIZING;

    return STATUS_SUCCESS;
}


NTSTATUS
HidKmdfPassThrough(
    __in PDEVICE_OBJECT DeviceObject,
    __in PIRP Irp
    )
/*++

Routine Description:

    Pass through routine for all the IRPs except power.

Arguments:

   DeviceObject - pointer to a device object.

   Irp - pointer to an I/O Request Packet.

Return Value:

      NT status code

--*/
{
    //
    // Copy current stack to next instead of skipping. We do this to preserve 
    // current stack information provided by hidclass driver to the minidriver
    //
    IoCopyCurrentIrpStackLocationToNext(Irp);
    return IoCallDriver(GET_NEXT_DEVICE_OBJECT(DeviceObject), Irp);
}

NTSTATUS  
PnP (
    IN PDEVICE_OBJECT DeviceObject,
    IN PIRP Irp
    )
/*++
Routine Description:

    Handles PnP Irps sent to FDO .

Arguments:

    DeviceObject - Pointer to deviceobject
    Irp          - Pointer to a PnP Irp.
    
Return Value:

    NT Status is returned.
--*/
{
    NTSTATUS            ntStatus;
    PIO_STACK_LOCATION  IrpStack;

    PAGED_CODE();

    IrpStack = IoGetCurrentIrpStackLocation (Irp);

    if(IrpStack->MinorFunction == IRP_MN_QUERY_ID)
	{

        //
        // This check is required to filter out QUERY_IDs forwarded
        // by the HIDCLASS for the parent FDO. These IDs are sent
        // by PNP manager for the parent FDO if you root-enumerate this driver.
        //
        PIO_STACK_LOCATION previousSp = ((PIO_STACK_LOCATION) ((UCHAR *) (IrpStack) + sizeof(IO_STACK_LOCATION)));
        
        if(previousSp->DeviceObject == DeviceObject) {
            //
            // Filtering out this basically prevents the Found New Hardware 
            // popup for the root-enumerated VHIDMINI on reboot.
            // 
            ntStatus = Irp->IoStatus.Status;
	        IoCompleteRequest (Irp, IO_NO_INCREMENT);
		    return ntStatus;
        }
       
        switch (IrpStack->Parameters.QueryId.IdType) 
        {
        case BusQueryDeviceID:
        case BusQueryHardwareIDs:
		{
#define VHID_HARDWARE_IDS    L"HID\\NullVirtualHidDevice\0\0"
#define VHID_HARDWARE_IDS_LENGTH sizeof (VHID_HARDWARE_IDS)
				// HIDClass is asking for child deviceid & hardwareids.
				// Let us just make up some  id for our child device.
				//
				PWCHAR buffer = (PWCHAR)ExAllocatePoolWithTag(
															  PagedPool, 
															  VHID_HARDWARE_IDS_LENGTH, 
															  (ULONG)'diHV'
															  );
				if (buffer) {
					// Do the copy, store the buffer in the Irp
					RtlCopyMemory(buffer, 
								  VHID_HARDWARE_IDS, 
								  VHID_HARDWARE_IDS_LENGTH
								  );
                
					Irp->IoStatus.Information = (ULONG_PTR)buffer;
					ntStatus = STATUS_SUCCESS;
				} 
				else
				{
					//  No memory
					ntStatus = STATUS_INSUFFICIENT_RESOURCES;
				}

				Irp->IoStatus.Status = ntStatus;
				//
				// We don't need to forward this to our bus. This query 
				// is for our child so we should complete it right here. 
				// fallthru.
				//
				IoCompleteRequest (Irp, IO_NO_INCREMENT);     
#undef VHID_HARDWARE_IDS
#undef VHID_HARDWARE_IDS_LENGTH
				return ntStatus;           
			}    
        default:            
            ntStatus = Irp->IoStatus.Status;
            IoCompleteRequest (Irp, IO_NO_INCREMENT);          
            return ntStatus;
        }
    }
    
    //Irp->IoStatus.Status = ntStatus;
    IoCopyCurrentIrpStackLocationToNext(Irp);
    return IoCallDriver(GET_NEXT_DEVICE_OBJECT(DeviceObject), Irp);
    
}

NTSTATUS
HidKmdfPowerPassThrough(
    __in PDEVICE_OBJECT DeviceObject,
    __in PIRP Irp
    )
/*++

Routine Description:

    Pass through routine for power IRPs .

Arguments:

   DeviceObject - pointer to a device object.

   Irp - pointer to an I/O Request Packet.

Return Value:

      NT status code

--*/
{
    //
    // Must start the next power irp before skipping to the next stack location
    //
    PoStartNextPowerIrp(Irp);

    //
    // Copy current stack to next instead of skipping. We do this to preserve 
    // current stack information provided by hidclass driver to the minidriver
    //
    IoCopyCurrentIrpStackLocationToNext(Irp);
    return PoCallDriver(GET_NEXT_DEVICE_OBJECT(DeviceObject), Irp);
}


VOID
HidKmdfUnload(
    __in PDRIVER_OBJECT DriverObject
    )
/*++

Routine Description:

    Free all the allocated resources, etc.

Arguments:

    DriverObject - pointer to a driver object.

Return Value:

    VOID.

--*/
{
    UNREFERENCED_PARAMETER(DriverObject);

    PAGED_CODE ();

    return;
}
