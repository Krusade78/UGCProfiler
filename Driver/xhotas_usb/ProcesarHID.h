EXTERN_C_START

VOID ProcesarInputX52(WDFDEVICE device, PVOID inputData, BOOLEAN repetirUltimo);

#ifdef _PRIVATE_
VOID PreProcesarModos(WDFDEVICE device, _Inout_ PUCHAR entrada);
VOID ProcesarHID(WDFDEVICE device, _Inout_ PHID_INPUT_DATA hidData);
#endif

EXTERN_C_END

