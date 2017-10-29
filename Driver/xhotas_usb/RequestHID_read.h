EXTERN_C_START

EVT_WDF_IO_QUEUE_STATE EvtRequestHIDLista;
EVT_WDF_TIMER EvtTickRaton;

VOID ProcesarRequest(WDFDEVICE device);
VOID CompletarRequestDirectX(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestTeclado(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestRaton(WDFDEVICE device, WDFREQUEST request);

#ifdef _PRIVATE_
#endif // _PRIVATE_

EXTERN_C_END

