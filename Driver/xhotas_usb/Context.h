#pragma once
#include "cola.h"

EXTERN_C_START

//HID_INPUT_DATA modificado
typedef struct _HID_INPUT_DATA
{
	UCHAR   Ejes[16];
	UCHAR	Setas[4];
	UCHAR	Botones[4];
	UCHAR   MiniStick;
} HID_INPUT_DATA, *PHID_INPUT_DATA;

#pragma region "X52 read"
typedef struct _HIDX52_INPUT_DATA
{
	UCHAR   EjesXYR[4];
	UCHAR	Ejes[4];
	UCHAR	Botones[4];
	UCHAR	Seta; // 2bits wheel + 2 blanco + 4 bits seta
	UCHAR	Ministick;
} HIDX52_INPUT_DATA, *PHIDX52_INPUT_DATA;

typedef struct _X52READ_CONTEXT
{
	WDFREQUEST			Request;
	WDFSPINLOCK			SpinLockRequest;
	WDFSPINLOCK			SpinLockPosicion;

	HIDX52_INPUT_DATA	UltimaPosicion;
} X52READ_CONTEXT;
#pragma endregion

typedef struct _HID_CONTEXT
{
	WDFSPINLOCK		SpinLockDeltaHid;
	HID_INPUT_DATA	DeltaHidData;

	UCHAR			EstadoModos;
	UCHAR			EstadoPinkie;
	BOOLEAN			ModoRaw;
	WDFTIMER		MenuTimer;
	BOOLEAN			MenuTimerEsperando;
	BOOLEAN			MenuActivado;

	WDFSPINLOCK		SpinLockAcciones;
	COLA			ColaAcciones;
	WDFCOLLECTION	ListaTimersDelay;

	WDFTIMER		RatonTimer;
	BOOLEAN			RatonActivado;

	UCHAR			stRaton[4];
	UCHAR			stTeclado[29];
	UCHAR			stSetas[4];
	UCHAR			stBotones[4];
} HID_CONTEXT;

#pragma region "Programación"
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
typedef struct _ST_COMANDO
{
	UCHAR tam;
	UINT16 *datos;
} COMANDO, *PCOMANDO;
typedef struct _PROGRAMADO_CONTEXT
{
	WDFSPINLOCK slCalibrado;
	STLIMITES	limites[4];
	STJITTER	jitter[4];

	struct
	{
		UCHAR Estado;	// 4 bit idc 4 bit total posiciones
		UCHAR Reservado; //padding
		UINT16 Indices[15];
	} MapaBotones[2][3][26]; // el ultimo es la rueda
	struct
	{
		UCHAR Estado;
		UCHAR Reservado; //padding
		UINT16 Indices[15];
	} MapaSetas[2][3][32];
	struct
	{
		UCHAR Mouse;
		UCHAR nEje;				//Mapeado 0:nada <20 normal >20 invertido
		UCHAR Sensibilidad[10];
		UCHAR Bandas[15];
		UCHAR Reservado; //padding
		UCHAR ResistenciaInc;
		UCHAR ResistenciaDec;
		UINT16 Indices[16];
	} MapaEjes[2][3][4];
	struct
	{
		UCHAR Mouse;
		UCHAR nEje;
		UCHAR Bandas[15];
		UCHAR Reservado; //padding
		UINT16 Indices[16];
		UCHAR ResistenciaInc;
		UCHAR ResistenciaDec;
	} MapaEjesPeque[2][3][4];
	struct
	{
		UCHAR Mouse;
		UCHAR nEje;
	} MapaEjesMini[2][3][2];

	USHORT	posVieja[2][3][7];

	UCHAR TickRaton;

	WDFSPINLOCK slMapas;

	COMANDO *Comandos;
	UINT16 nComandos;
	WDFSPINLOCK slComandos;
} PROGRAMADO_CONTEXT;
#pragma endregion

typedef struct _X52WRITE_CONTEXT
{
	WDFWAITLOCK		WaitLockX52;
	USHORT			fecha;
} X52WRITE_CONTEXT;

DECLARE_HANDLE(HNOTIFICATION);
typedef struct _PEDALES_CONTEXT
{
	PVOID			PnPNotifyHandle;
	WCHAR			SymbolicLink[200];
	WDFIOTARGET		IoTarget;
	WDFWAITLOCK		WaitLockIoTarget;
	WDFSPINLOCK		SpinLockPosicion;
	UCHAR			Activado;
	UCHAR			PedalSel;
	INT16			UltimaPosicion;
} PEDALES_CONTEXT;

typedef struct _DEVICE_CONTEXT
{
	WDFDEVICE			ControlDevice;
	WDFQUEUE			ColaRequest;
	X52READ_CONTEXT		EntradaX52;
	X52WRITE_CONTEXT	SalidaX52;
	PEDALES_CONTEXT		Pedales;
	HID_CONTEXT			HID;
	PROGRAMADO_CONTEXT	Programacion;
	WDFUSBDEVICE		UsbDevice;
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, GetDeviceContext);

EXTERN_C_END
