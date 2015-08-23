VOID AccionarHOTAS
(
	PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA accion
	);

VOID AccionarRaton
(
	PDEVICE_EXTENSION devExt,
	PUCHAR accion
	);

VOID AccionarComando
(
	PDEVICE_EXTENSION devExt,
	UINT16 accionId,
	UCHAR boton
	);
