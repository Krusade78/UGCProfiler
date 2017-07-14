EXTERN_C_START

NTSTATUS HF_IoEscribirComandos(_In_ WDFREQUEST Request);
NTSTATUS HF_IoEscribirMapa(_In_ WDFREQUEST Request);

VOID LimpiarMemoria(_In_ WDFDEVICE device);

EXTERN_C_END