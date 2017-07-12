EXTERN_C_START

VOID ProcesarInputX52(WDFDEVICE device, PVOID inputData);

#ifdef _PRIVATE_
typedef struct _HIDX52_INPUT_DATA
{
	UCHAR   EjesXYR[4];
	UCHAR	Ejes[4];
	UCHAR	Botones[4];
	UCHAR	Seta; // 2bits wheel + 2 blanco + 4 bits seta
	UCHAR	Ministick;
} HIDX52_INPUT_DATA, *PHIDX52_INPUT_DATA;

VOID PreProcesarModos(WDFDEVICE device, _Inout_ PUCHAR entrada);
VOID ProcesarHID(WDFDEVICE device, _Inout_ PHID_INPUT_DATA hidData);
#endif

EXTERN_C_END

