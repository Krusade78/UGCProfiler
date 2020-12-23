#ifdef _CONTEXT_
//HID original X52
typedef struct _HIDX52_INPUT_DATA
{
	UCHAR   EjesXYR[4];
	UCHAR	Ejes[4];
	UCHAR	Botones[4];
	UCHAR	Seta; // 2bits wheel + 2 blanco + 4 bits seta
	UCHAR	Ministick;
} HIDX52_INPUT_DATA, * PHIDX52_INPUT_DATA;

//HID_INPUT_DATA modificado
typedef struct _HID_INPUT_DATA
{
	UINT16  Ejes[9]; // X, Y, Z, R
	UCHAR	Setas[4];
	UCHAR	Botones[4];
	UCHAR   MiniStick;
} HID_INPUT_DATA, *PHID_INPUT_DATA;

typedef struct _USB_HIDX52_CONTEXT
{
	WDFWAITLOCK			WaitLockProcesar;
	HIDX52_INPUT_DATA	UltimaPosicion;

	BOOLEAN				ModoRaw;

	struct
	{
		WDFWAITLOCK		WaitLockUltimoEstado;
		HID_INPUT_DATA	DeltaHidData;
		USHORT	PosIncremental[2][3][9];
		UCHAR	Banda[2][3][9];
	} UltimoEstado;
} USB_HIDX52_CONTEXT;
#endif // _CONTEXT_

#ifdef _PUBLIC_
VOID ProcesarEntradaUSB(WDFDEVICE device, PHIDX52_INPUT_DATA inputData, BOOLEAN pedales);
#endif // _PUBLIC_

#ifdef _PRIVATE_
VOID PreProcesarModos(WDFDEVICE device, _In_ PHIDX52_INPUT_DATA entrada);
UCHAR Switch4To8(UCHAR in);
VOID ConvertirEjesA2048(PHID_INPUT_DATA hid, BOOLEAN pedales);
VOID ConvertirDatosUSBaHID(WDFDEVICE device, _In_ PHIDX52_INPUT_DATA hidGameData);
VOID ProcesarHID(WDFDEVICE device, _In_ PHID_INPUT_DATA hidData);
#endif


