EXTERN_C_START

VOID AccionarRaton(WDFDEVICE device, PUCHAR accion, BOOLEAN enDelay);
VOID AccionarComando(WDFDEVICE device, UINT16 accionId,	UCHAR boton);
VOID AccionarDirectX(WDFDEVICE device, PHID_INPUT_DATA inputData);

EXTERN_C_END
