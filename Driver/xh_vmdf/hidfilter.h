#ifndef GLOBALS

#define GLOBALS

#include "cola.h"

typedef struct _HID_INPUT_DATA
{
    UCHAR   Ejes[18];
	UCHAR	Setas[4];
	UCHAR	Botones[7];
    UCHAR   MiniStick;
} HID_INPUT_DATA, *PHID_INPUT_DATA;

typedef	struct _CALIBRADO {
	UINT16	i;
	UINT16	c;
	UINT16	d;
	UCHAR	n;
	UCHAR	Margen;
	UCHAR	Resistencia;
	BOOLEAN cal;
	BOOLEAN antiv;
} CALIBRADO, *PCALIBRADO;

typedef struct _STLIMITES{
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

typedef struct _ITFDEVICE_EXTENSION
{
	//ULONG id;

	WDFSPINLOCK slCalibrado;
	STLIMITES	limites[4];
	STJITTER	jitter[4];
	BOOLEAN		descalibrar;

	struct
	{
		UCHAR Estado;	// 4 bit idc 4 bit total posiciones
		UINT16 Indices[15];
	} MapaBotones[2][3][3][26]; // el ultimo es la rueda
	struct
	{
		UCHAR Estado;
		UINT16 Indices[15];
	} MapaSetas[2][3][3][32];
	struct
	{
		UCHAR Mouse;
		UCHAR nEje;				//Mapeado 0:nada <20 normal >20 invertido
		UCHAR Sensibilidad[10];
		UCHAR Bandas[15];
		UINT16 Indices[16];
	} MapaEjes[2][3][3][4];
	struct
	{
		UCHAR Mouse;
		UCHAR nEje;
		UCHAR Bandas[16];
		UINT16 Indices[16];
	} MapaEjesPeque[2][3][3][3];
	struct
	{
		UCHAR Mouse;
		UCHAR nEje;
	} MapaEjesMini[2][3][3][2];

	UCHAR	stPinkie;
	UCHAR	stModo;
	UCHAR	stAux;
	USHORT	posVieja[2][3][3][7];

	WDFSPINLOCK slMapas;

	COMANDO *Comandos;
	UINT16 nComandos;
	WDFSPINLOCK slComandos;

} ITFDEVICE_EXTENSION, *PITFDEVICE_EXTENSION;

typedef struct _DEVICE_EXTENSION
{
	WDFDEVICE		ControlDevice;
	ITFDEVICE_EXTENSION itfExt;
	 
	// IoTargets
	WDFWORKITEM		WorkItemTargets;
	WDFTIMER		TimerIoTargets;
	WDFDPC			DpcTargets[5];	// 0,1,2 Abrir Pedales,HOTAS,USB ; 4,5 Cerrar Pedales,HOTAS

	WDFIOTARGET		TargetHIDPedales;
	WDFIOTARGET		TargetHIDHOTAS;
	WDFIOTARGET		TargetUSBX52;
	//WDFREQUEST		RequestUSBX52;
	//WDFSPINLOCK		SpinLockTargets;
	WDFWAITLOCK		WaitLockCierre;
	BOOLEAN			D0Apagado;
	// -------------------------

	// entradahid
	BOOLEAN			PedalesActivados;
	UCHAR			PedalSel;
	UCHAR			stModos;
	UCHAR			stAux;
	WDFSPINLOCK		SpinLockDeltaHid;
	HID_INPUT_DATA	DeltaHidData;
	//-------------------------

	// acciones, ioctl, reports
	WDFSPINLOCK		SpinLockAcciones;
	COLA			ColaAccionesComando;
	COLA			ColaAccionesRaton;
	COLA			ColaAccionesHOTAS;
	//---------------------------------

	//reports
	WDFDPC			DpcRequest;
	WDFREQUEST		RequestEnEspera;
	CHAR			TurnoReport;
	WDFTIMER		TimerRaton;
	BOOLEAN			TimerRatonOn;
	UCHAR			TickRaton;
	UCHAR			stRaton[4];
	UCHAR			stTeclado[29];
	HID_INPUT_DATA	stHOTAS;

	//x52
	USHORT			fecha;
} DEVICE_EXTENSION, * PDEVICE_EXTENSION;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_EXTENSION, GetDeviceExtension);

typedef struct _CONTROL_EXTENSION
{
	PDEVICE_EXTENSION devExt;
} CONTROL_EXTENSION, *PCONTROL_EXTENSION;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(CONTROL_EXTENSION, GetControlExtension);
#endif

#ifdef _HIDFILTER_

DRIVER_INITIALIZE DriverEntry;

EVT_WDF_DRIVER_DEVICE_ADD HF_AddDevice;

NTSTATUS IniciarExtensiones(IN WDFDEVICE device);

NTSTATUS IniciarInterfazControl(IN WDFDRIVER Driver, IN PDEVICE_EXTENSION devExt);

EVT_WDF_IO_QUEUE_IO_DEFAULT HF_DefaultIoDeviceControl;

EVT_WDF_DEVICE_D0_ENTRY HF_EvtDeviceD0Entry;

EVT_WDF_DEVICE_D0_EXIT HF_EvtDeviceD0Exit;

EVT_WDF_DEVICE_RELEASE_HARDWARE  HF_EvtReleaseHardware;

EVT_WDF_OBJECT_CONTEXT_CLEANUP EvtCleanupCallback;

#endif