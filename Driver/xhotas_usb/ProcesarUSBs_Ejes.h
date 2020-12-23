#ifdef _PUBLIC_
VOID SensibilidadYMapeado(
	WDFDEVICE		device,
	PHID_INPUT_DATA viejo,
	PHID_INPUT_DATA entrada,
	PHID_INPUT_DATA salida
);

VOID MoverEje(WDFDEVICE device, UCHAR idx, UINT16 nuevo);
#endif // _PUBLIC_

#ifdef _PRIVATE_
VOID EjeARaton(WDFDEVICE device, UCHAR eje, CHAR mov);
UCHAR TraducirGiratorio(WDFDEVICE device, UCHAR eje, UINT16 nueva, UCHAR pinkie, UCHAR modos);
#endif // _PRIVATE_
