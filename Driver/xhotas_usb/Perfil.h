#ifdef _CONTEXT_
typedef struct _PROGRAMADO_CONTEXT
{
	WDFWAITLOCK WaitLockMapas;
	struct
	{
		UCHAR PosActual;
		UCHAR TamIndices;
		UINT16 Indices[15];
	} MapaBotones[2][3][26]; // el ultimo es la rueda
	struct
	{
		UCHAR PosActual;
		UCHAR TamIndices;
		UINT16 Indices[15];
	} MapaSetas[2][3][32];
	struct
	{
		UCHAR SensibilidadRaton;
		UCHAR TipoEje; 				//Mapeado en bits 0:ninguno, 1:Normal, 10:Invertido, 100:Mini, 1000:Raton, 10000:Incremental, 100000: Bandas
		UCHAR Eje;
		UCHAR Sensibilidad[10];  // Curva de sernsibilidad
		UCHAR Bandas[15];
		UINT16 Indices[16];
		UCHAR ResistenciaInc;
		UCHAR ResistenciaDec;
	} MapaEjes[2][3][4];
	struct
	{
		UCHAR SensibilidadRaton;
		UCHAR TipoEje;
		UCHAR Eje;      //0:Nada, 1:X , Y, R, Z, Rx, Ry, Sl1, Sl2, Sl3, 32:miniX, 33:miniY, 64:ratonX, 65:ratonY
		UCHAR Bandas[15];
		UINT16 Indices[16];
		UCHAR ResistenciaInc;
		UCHAR ResistenciaDec;
	} MapaEjesPeque[2][3][5];
	struct
	{
		UCHAR SensibilidadRaton;
		UCHAR TipoEje;
		UCHAR nEje;
		UCHAR Reservado; //padding
	} MapaEjesMini[2][3][2];

	WDFWAITLOCK WaitLockAcciones;
	WDFCOLLECTION Acciones;

	UCHAR TickRaton;

} PROGRAMADO_CONTEXT;
#endif // _CONTEXT_

#ifdef _PUBLIC_
VOID LimpiarPerfil(WDFDEVICE device);

NTSTATUS HF_IoEscribirComandos(_In_ WDFDEVICE device, _In_ WDFREQUEST Request, _In_ BOOLEAN vacio);
NTSTATUS HF_IoEscribirMapa(_In_ WDFDEVICE device, _In_ WDFREQUEST Request);
#endif // _PUBLIC_

#ifdef _PRIVATE_
#endif // _PRIVATE_
