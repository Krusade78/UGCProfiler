EXTERN_C_START

VOID AccionarRaton(WDFDEVICE device, PUCHAR accion);
VOID AccionarComando(WDFDEVICE device, UINT16 accionId,	UCHAR boton);
VOID ProcesarAcciones(WDFDEVICE device);

#ifdef _ACCIONES_
typedef	struct _EVENTO {
	UCHAR tipo;
	UCHAR dato;
} EVENTO, *PEVENTO;

typedef struct _DELAY_CONTEXT {
	PNODO NodoIni;
	PNODO NodoFin;
} DELAY_CONTEXT, *PDELAY_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE(DELAY_CONTEXT)
EVT_WDF_TIMER TimerDelay;

VOID ProcesarComandos(_In_ WDFDEVICE device);

VOID ProcesarEventoX52_Modos(_In_ WDFDEVICE device, PCOLA cola, PNODO nodo, PEVENTO evento);

BOOLEAN ProcesarEventoRepeticiones_Delay(WDFDEVICE device, PCOLA cola, PNODO nodo, PEVENTO evento);
BOOLEAN ReservarMemoriaRepeticiones(PCOLA cola, PNODO nodo, UCHAR eof);
BOOLEAN EstaHold(HID_CONTEXT devExt, UCHAR boton);
#endif // _ACCIONES_

EXTERN_C_END
