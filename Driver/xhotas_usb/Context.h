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

	HID_INPUT_DATA	UltimaPosicion;
} X52READ_CONTEXT;

typedef struct _HID_CONTEXT
{
	WDFSPINLOCK		SpinLockDeltaHid;
	HID_INPUT_DATA	DeltaHidData;

	UCHAR			EstadoModos;
	UCHAR			EstadoPinkie;
	UCHAR			ModoRaw;
} HID_CONTEXT;

typedef struct _STLIMITES {
	BOOLEAN cal;
	UINT16 i;
	UINT16 c;
	UINT16 d;
	UCHAR n;
} STLIMITES, *PSTLIMITES;
typedef struct _STJITTER {
	BOOLEAN antiv;
	UINT16 PosElegida;
	UCHAR PosRepetida;
	UCHAR Margen;
	UCHAR Resistencia;
} STJITTER, *PSTJITTER;
typedef struct _PROGRAMADO_CONTEXT
{
	WDFSPINLOCK slCalibrado;
	STLIMITES	limites[4];
	STJITTER	jitter[4];

} PROGRAMADO_CONTEXT;

typedef struct _X52WRITE_CONTEXT
{
	USHORT			fecha;
} X52WRITE_CONTEXT;

DECLARE_HANDLE(HNOTIFICATION);
typedef struct _PEDALES_CONTEXT
{
	PVOID			PnPNotifyHandle;
	WCHAR			SymbolicLink[100];
	WDFIOTARGET		IoTarget;
	WDFWAITLOCK		WaitLockIoTarget;
	WDFSPINLOCK		SpinLockPosicion;
	UCHAR			Activado;
	UCHAR			PedalSel;
	INT16			UltimaPosicion;
} PEDALES_CONTEXT;

typedef struct _DEVICE_CONTEXT
{
	WDFDEVICE			Device;
	WDFQUEUE			ColaTeclado;
	WDFQUEUE			ColaRaton;
	X52READ_CONTEXT		EntradaX52;
	X52WRITE_CONTEXT	SalidaX52;
	PEDALES_CONTEXT		Pedales;
	HID_CONTEXT			HID;
	PROGRAMADO_CONTEXT	Programacion;
	//WDFUSBDEVICE		UsbDevice;
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, GetDeviceContext);

EXTERN_C_END
