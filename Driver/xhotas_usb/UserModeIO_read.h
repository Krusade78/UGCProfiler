EXTERN_C_START

NTSTATUS IniciarIoCtlAplicacion(_In_ WDFDEVICE device);

#ifdef _PRIVATE_
#define IOCTL_MFD_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0800, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_GLOBAL_LUZ	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0801, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_INFO_LUZ		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0802, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_PINKIE		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0803, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0804, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0805, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0806, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0807, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_RAW		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0808, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_CALIBRADO	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0809, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_MAPA		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x080a, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_COMANDOS	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x080b, METHOD_BUFFERED, FILE_WRITE_ACCESS)

typedef struct _CONTROL_CONTEXT
{
	WDFDEVICE padre;
} CONTROL_CONTEXT, *PCONTROL_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(CONTROL_CONTEXT, GetControlContext);

EVT_WDF_IO_QUEUE_IO_DEVICE_CONTROL EvtIOCtlAplicacion;
#endif

EXTERN_C_END

