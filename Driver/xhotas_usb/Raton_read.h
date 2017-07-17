EXTERN_C_START

EVT_WDF_IO_QUEUE_STATE EvtRatonListo;
BOOLEAN ProcesarEventoRaton(WDFDEVICE device, UCHAR tipo, UCHAR dato);

EVT_WDF_TIMER EvtTickRaton;

EXTERN_C_END

