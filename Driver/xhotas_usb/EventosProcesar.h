#ifdef _PUBLIC_
typedef struct
{
	UCHAR Tipo;
	UCHAR Dato;
} EV_COMANDO, *PEV_COMANDO;

typedef struct
{
	UCHAR Tipo;
	UCHAR Origen;
	UCHAR Modo;
	UCHAR Pinkie;
	USHORT Incremental;
	UCHAR Banda;
} EV_COMANDO_EXTENDIDO, *PEV_COMANDO_EXTENDIDO;

enum
{
	TipoComando_Tecla = 1,

	TipoComando_DxBoton,
	TipoComando_DxSeta,

	TipoComando_RatonBt1,
	TipoComando_RatonBt2,
	TipoComando_RatonBt3,
	TipoComando_RatonIzq,
	TipoComando_RatonDer,
	TipoComando_RatonArr,
	TipoComando_RatonAba,
	TipoComando_RatonWhArr,
	TipoComando_RatonWhAba,

	TipoComando_Delay = 20,
	TipoComando_Hold,
	TipoComando_Repeat,
	TipoComando_RepeatN,

	TipoComando_Modo = 30,
	TipoComando_Pinkie,

	TipoComando_MfdLuz = 40,
	TipoComando_Luz,
	TipoComando_InfoLuz,
	TipoComando_MfdPinkie,
	TipoComando_MfdTextoIni,
	TipoComando_MfdTexto,
	TipoComando_MfdTextoFin,
	TipoComando_MfdHora,
	TipoComando_MfdHora24,
	TipoComando_MfdFecha,

	TipoComando_Reservado_DxPosicion = 100,
	TipoComando_Reservado_EstadoHoldNormal = 101,
	TipoComando_Reservado_EstadoHoldEje = 102,
	//TipoComando_Reservado_RepeatIni,

	TipoComando_Soltar = 128
};

BOOLEAN ProcesarEventos(WDFDEVICE device);
VOID LimpiarEventos(WDFDEVICE device);
#endif // _PUBLIC_

#ifdef _PRIVATE_
typedef struct
{
	WDFCOLLECTION Cola;
}DELAY_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE(DELAY_CONTEXT)
EVT_WDF_TIMER TimerDelay;
EVT_WDF_WORKITEM TimerDelayWI;

BOOLEAN PrepararDirectX(WDFDEVICE device);
VOID ProcesarDirectX(WDFDEVICE device, PEV_COMANDO comando);
//
BOOLEAN PrepararRaton(WDFDEVICE device);
BOOLEAN ProcesarRaton(WDFDEVICE device, WDFCOLLECTION colaComandos, PEV_COMANDO comando);

BOOLEAN PrepararTeclado(WDFDEVICE device);
VOID ProcesarTeclado(WDFDEVICE device, PEV_COMANDO comando);

VOID ProcesarComandos(WDFDEVICE device);
BOOLEAN ProcesarEventoX52_Modos(WDFDEVICE device, WDFCOLLECTION colaComandos, PEV_COMANDO comando);

UCHAR ProcesarEventoRepeticiones_Delay(WDFDEVICE device, WDFCOLLECTION colaComandos, ULONG posEvento, PEV_COMANDO comando);
BOOLEAN EstaHold(WDFDEVICE device, PEV_COMANDO comando);
VOID BorrarBloqueRepeat(WDFCOLLECTION colaComandos, ULONG posComando, UCHAR tipoComando);
BOOLEAN CopiarColaConRepeticion1(WDFCOLLECTION colaComandos, ULONG posComando, UCHAR tipoComando);
BOOLEAN CopiarColaConRepeticion2(WDFCOLLECTION colaComandos, WDFCOLLECTION colaAux, ULONG posComando, UCHAR tipoComando);
#endif // _ACCIONES_

