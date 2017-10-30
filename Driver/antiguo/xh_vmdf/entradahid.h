void ProcesarEntradaPedales(IN PDEVICE_EXTENSION devExt, IN PVOID buffer);
void ProcesarEntradaHOTAS(IN PDEVICE_EXTENSION devExt, IN PVOID buffer, BOOLEAN x52);

#ifdef _DIRECTX_

typedef struct _HIDX36_INPUT_DATA
{
    UCHAR   EjesXY[3];
	UCHAR	Ejes[4];
	UCHAR	Botones[2];
	UCHAR	Setas[2];
} HIDX36_INPUT_DATA, *PHIDX36_INPUT_DATA;
typedef struct _HIDX52_INPUT_DATA
{
    UCHAR   EjesXYR[4];
	UCHAR	Ejes[4];
	UCHAR	Botones[4];
	UCHAR	Seta; // 2bits wheel + 2 blanco + 4 bits seta
	UCHAR	Ministick;
} HIDX52_INPUT_DATA, *PHIDX52_INPUT_DATA;

UCHAR Switch4To8(UCHAR in);

VOID 
PreProcesarHID(   
    PDEVICE_EXTENSION devExt,
	PVOID inputData,
	BOOLEAN esX52
    );

VOID 
ProcesarHID(   
    PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA inputData
    );

#endif