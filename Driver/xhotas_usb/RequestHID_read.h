EXTERN_C_START

EVT_WDF_IO_QUEUE_IO_DEFAULT EvtRequestHID;
EVT_WDF_TIMER EvtTickRaton;

VOID ForzarProcesarRequest(WDFDEVICE device);

#ifdef _PRIVATE_
VOID ProcesarRequest(_In_ WDFDEVICE Device, _In_ WDFREQUEST Request);
VOID CompletarRequestDirectX(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestTeclado(WDFDEVICE device, WDFREQUEST request);
VOID CompletarRequestRaton(WDFDEVICE device, WDFREQUEST request);
#endif // _PRIVATE_

EXTERN_C_END

