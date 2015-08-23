VOID ProcesarReport(IN WDFREQUEST Request);

EVT_WDF_TIMER	TimerTickRaton;
EVT_WDF_DPC		EvDpc;

#ifdef _REPORTS_

EVT_WDF_REQUEST_CANCEL  EvtRequestCancel;

VOID ProcesarHOTAS(IN WDFREQUEST request);

VOID ProcesarRaton(IN WDFREQUEST request);
VOID ActualizarTimerRaton(PDEVICE_EXTENSION devExt);

typedef	struct _EVENTO {
	UCHAR tipo;
	UCHAR dato;
} EVENTO, *PEVENTO;
BOOLEAN ProcesarComando(IN WDFREQUEST request);
NTSTATUS ProcesarEventoRaton(WDFREQUEST request, PEVENTO evento);
NTSTATUS ProcesarEventoTeclado_HOTAS(WDFREQUEST request, PEVENTO evento);
VOID ProcesarEventoX52_Modos(PDEVICE_EXTENSION devExt, PCOLA cola, PNODO nodo, PEVENTO evento);
BOOLEAN ProcesarEventoRepeticiones_Delay(WDFREQUEST request,
											PCOLA cola,
											PNODO nodo,
											PEVENTO evento);

BOOLEAN ReservarMemoriaRepeticiones(PCOLA cola, PNODO nodo, UCHAR eof);

BOOLEAN EstaHold(PDEVICE_EXTENSION devExt, UCHAR boton);

typedef struct _DELAY_CONTEXT {
	PNODO NodoIni;
	PNODO NodoFin;
} DELAY_CONTEXT, *PDELAY_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE(DELAY_CONTEXT)
EVT_WDF_TIMER TimerDelay;

#endif