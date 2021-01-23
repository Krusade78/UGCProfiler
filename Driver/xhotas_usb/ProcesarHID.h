#ifdef _CONTEXT_
typedef struct _HID_CONTEXT
{
	WDFQUEUE		ColaRequestSinUsar;
	WDFWAITLOCK		WaitLockRequest;

	WDFWAITLOCK		WaitLockEventos;
	WDFCOLLECTION	ColaEventos;
	WDFCOLLECTION	ListaTimersDelay;
//
	WDFTIMER		RatonTimer;
//	BOOLEAN			RatonActivado;
//
	UCHAR			TurnoReport;
	struct
	{
		WDFWAITLOCK		WaitLockEstado;  //para Modos, Pinkie
		UCHAR			Modos;
		UCHAR			Pinkie;
		UCHAR			Raton[4];
		UCHAR			Teclado[29];
		HID_INPUT_DATA	DirectX;
	} Estado;
} HID_CONTEXT, *PHID_CONTEXT;
#endif // _CONTEXT_

#ifdef _PUBLIC_
EVT_WDF_IO_QUEUE_IO_DEFAULT EvtRequestHID;
EVT_WDF_TIMER EvtTickRaton;

VOID ProcesarRequestHIDForzada(WDFDEVICE device);
#endif // _PUBLIC_

#ifdef _PRIVATE_
typedef struct _HID_REPORT1
{
	UINT16  Ejes[6]; // X, Y, Z, Rx, Ry, Sl
	UCHAR	Setas[4];
	UCHAR	Botones[4];
} HID_REPORT1;
typedef struct _HID_REPORT2
{
	UINT16  Ejes[3]; // R, FrenoI, FrenoD
	UCHAR   MiniStick;
} HID_REPORT2;

VOID ProcesarRequest(_In_ WDFDEVICE Device, WDFREQUEST Request);
VOID CompletarRequestDirectX1(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestDirectX2(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestTeclado(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestRaton(WDFDEVICE device, WDFREQUEST request);
#endif // _PRIVATE_


