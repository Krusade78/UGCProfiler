EXTERN_C_START

//HID_INPUT_DATA modificado
typedef struct _HID_INPUT_DATA
{
	UCHAR   Ejes[16];
	UCHAR	Setas[4];
	UCHAR	Botones[4];
	UCHAR   MiniStick;
} HID_INPUT_DATA, *PHID_INPUT_DATA;

typedef struct _X52READ_CONTEXT
{
	WDFCOLLECTION	ListaRequest;
	WDFSPINLOCK		SpinLockRequest;
	WDFSPINLOCK		SpinLockPosicion;
	HID_INPUT_DATA	Posicion;
} X52READ_CONTEXT;

typedef struct _X52WRITE_CONTEXT
{
	USHORT			fecha;
} X52WRITE_CONTEXT;

DECLARE_HANDLE(HNOTIFICATION);
typedef struct _PEDALES_CONTEXT
{
	HNOTIFICATION	PnPNotifyHandle;
	WCHAR			SymbolicLink[100];
	WDFIOTARGET		IoTarget;
	WDFWAITLOCK		WaitLockIoTarget;
	WDFSPINLOCK		SpinLockPosicion;
	BOOLEAN			Activado;
	UCHAR			PedalSel;
	INT16			Posicion;
} PEDALES_CONTEXT;

typedef struct _DEVICE_CONTEXT
{
	//WDFDEVICE			Self;
	X52READ_CONTEXT		EntradaX52;
	X52WRITE_CONTEXT	SalidaX52;
	PEDALES_CONTEXT		Pedales;
	//WDFUSBDEVICE		UsbDevice;
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, GetDeviceContext);

EXTERN_C_END
