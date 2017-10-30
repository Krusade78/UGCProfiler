EVT_WDF_IO_QUEUE_IO_DEVICE_CONTROL HF_Control;

#ifdef _CONTROL_

NTSTATUS
HF_IoEscribirCalibrado(
    IN WDFREQUEST Request,
    IN PDEVICE_EXTENSION DeviceExtension
	);

#endif