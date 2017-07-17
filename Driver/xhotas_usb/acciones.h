EXTERN_C_START

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
BOOLEAN EstaHold(HID_CONTEXT devExt, UCHAR boton);
#endif // _ACCIONES_

EXTERN_C_END
