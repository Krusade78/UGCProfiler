VOID Calibrado(
	PITFDEVICE_EXTENSION idevExt,
	PHID_INPUT_DATA datosHID
	);

VOID SensibilidadYMapeado(
	PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA viejo,
	PHID_INPUT_DATA entrada,
	PHID_INPUT_DATA salida
	);

VOID GenerarAccionesEjes(
	PDEVICE_EXTENSION devExt,
	UCHAR idx,
	USHORT nuevo
	);

VOID GenerarHIDEjes(
	PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA datosHid
	);

#ifdef _EJES_

VOID GenerarAccionRaton
(
	PDEVICE_EXTENSION idevExt,
	UCHAR eje,
	CHAR mov
	);

UCHAR TraducirGiratorio(
	PITFDEVICE_EXTENSION idevExt,
	UCHAR eje,
	USHORT nueva
	);

#endif