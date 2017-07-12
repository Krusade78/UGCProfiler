EXTERN_C_START

NTSTATUS EscribirCalibrado(_In_ WDFDEVICE device, _In_ WDFREQUEST request);
VOID Calibrar(WDFDEVICE device, PHID_INPUT_DATA datosHID);

#ifdef _PRIVATE_
typedef	struct _CALIBRADO {
	UINT16	i;
	UINT16	c;
	UINT16	d;
	UCHAR	n;
	UCHAR	Margen;
	UCHAR	Resistencia;
	BOOLEAN cal;
	BOOLEAN antiv;
} CALIBRADO, *PCALIBRADO;

#endif // _PRIVATE_

EXTERN_C_END

