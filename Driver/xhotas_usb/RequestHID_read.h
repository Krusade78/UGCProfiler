EXTERN_C_START

EVT_WDF_IO_QUEUE_STATE EvtRequestHIDLista;

BOOLEAN ProcesarEventoTeclado(WDFDEVICE device, UCHAR tipo, UCHAR dato);
BOOLEAN ProcesarEventoRaton(WDFDEVICE device, UCHAR tipo, UCHAR dato);
EVT_WDF_TIMER EvtTickRaton;

EXTERN_C_END

