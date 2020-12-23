#ifdef _PUBLIC_
enum
{
	Origen_Boton = 0,
	Origen_Seta,
	Origen_Eje
};

VOID GenerarEventoRaton(WDFDEVICE device, PVOID accion); //PVOID porque PEV_COMANDO generar un bucle de includes
VOID GenerarEventoComando(WDFDEVICE device, UINT16 accionId, UCHAR origen, UCHAR tipoOrigen, PVOID datosEje);
VOID GenerarEventoDirectX(WDFDEVICE device, PHID_INPUT_DATA inputData);
#endif // _PUBLIC_


