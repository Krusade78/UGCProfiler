EXTERN_C_START

NTSTATUS HF_IoEscribirComandos(_In_ WDFDEVICE device, _In_ WDFREQUEST Request);
NTSTATUS HF_IoEscribirMapa(_In_ WDFDEVICE device, _In_ WDFREQUEST Request);

VOID LimpiarMapa(WDFDEVICE device);

EXTERN_C_END