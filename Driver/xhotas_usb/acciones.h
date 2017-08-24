EXTERN_C_START

enum
{
	TipoComando_Tecla = 0,
	TipoComando_RatonBt1,
	TipoComando_RatonBt2,
	TipoComando_RatonBt3,
	TipoComando_RatonIzq,
	TipoComando_RatonDer,
	TipoComando_RatonArr,
	TipoComando_RatonAba,
	TipoComando_RatonWhArr,
	TipoComando_RatonWhAba,
	TipoComando_Delay,
	TipoComando_Hold,
	TipoComando_Repeat,
	TipoComando_RepeatN,
	TipoComando_Modo,
	TipoComando_Pinkie = 16,
	TipoComando_RepeatIni,
	TipoComando_DxBoton,
	TipoComando_DxSeta,
	TipoComando_MfdLuz,
	TipoComando_Luz,
	TipoComando_InfoLuz,
	TipoComando_MfdPinkie,
	TipoComando_MfdTexto,
	TipoComando_MfdHora,
	TipoComando_MfdHora24,
	TipoComando_MfdFecha,
	TipoComando_RepeatFin = 44,
	TipoComando_RepeatNFin,
	TipoComando_MfdTextoFin = 56
};

VOID AccionarRaton(WDFDEVICE device, PUCHAR accion, BOOLEAN enDelay);
VOID AccionarComando(WDFDEVICE device, UINT16 accionId,	UCHAR boton);
VOID ProcesarAcciones(WDFDEVICE device, BOOLEAN enDelay);
VOID LimpiarAcciones(WDFDEVICE device);

#ifdef _ACCIONES_
typedef COLA DELAY_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE(DELAY_CONTEXT)
EVT_WDF_TIMER TimerDelay;

VOID ProcesarComandos(_In_ WDFDEVICE device, _In_ BOOLEAN enDelay);

VOID ProcesarDirectX(WDFDEVICE device, BOOLEAN enDelay, UCHAR tipo, UCHAR dato);

VOID ProcesarEventoX52_Modos(_In_ WDFDEVICE device, PCOLA cola, PNODO nodo, UCHAR tipo, UCHAR dato);

BOOLEAN ProcesarEventoRepeticiones_Delay(WDFDEVICE device, PCOLA cola, PNODO nodo, UCHAR* tipo, UCHAR dato);
BOOLEAN ReservarMemoriaRepeticiones(PCOLA cola, PNODO nodo, UCHAR eof);
BOOLEAN EstaHold(HID_CONTEXT* devExt, UCHAR boton);
#endif // _ACCIONES_

EXTERN_C_END
