EVT_WDF_IO_QUEUE_IO_DEVICE_CONTROL HF_Control;

#ifdef _CONTROL_

NTSTATUS SetFecha(IN WDFDEVICE Device, IN UCHAR* datos);
NTSTATUS SetLinea(IN WDFDEVICE Device, IN CHAR linea, IN UCHAR* texto);
NTSTATUS EnviarOrden(IN WDFDEVICE Device, UCHAR* params);

#endif
