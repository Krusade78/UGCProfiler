EXTERN_C_START

VOID ProcesarInputX52(WDFDEVICE device, _In_ PVOID inputData, BOOLEAN repetirUltimo);

#ifdef _PRIVATE_
VOID PreProcesarModos(WDFDEVICE device, _In_ PUCHAR entrada);
VOID ProcesarX52(WDFDEVICE device, _In_ PVOID inputData);
VOID ProcesarHID(WDFDEVICE device, _In_ PHID_INPUT_DATA hidData);
#endif

EXTERN_C_END

