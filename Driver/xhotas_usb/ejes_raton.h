EXTERN_C_START

VOID SensibilidadYMapeado(
	WDFDEVICE		device,
	PHID_INPUT_DATA viejo,
	PHID_INPUT_DATA entrada,
	PHID_INPUT_DATA salida
	);

VOID GenerarAccionesEjes(WDFDEVICE device, UCHAR idx, USHORT nuevo);

#ifdef _EJES_

VOID GenerarAccionRaton(WDFDEVICE device, UCHAR eje, CHAR mov);
UCHAR TraducirGiratorio(WDFDEVICE device, UCHAR eje, USHORT nueva);

#endif

EXTERN_C_END