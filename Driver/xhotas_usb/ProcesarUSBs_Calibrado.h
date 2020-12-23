#ifdef _CONTEXT_
typedef struct _STLIMITES {
	BOOLEAN Cal;
	UCHAR Nulo;
	UINT16 Izq;
	UINT16 Cen;
	UINT16 Der;
} STLIMITES, * PSTLIMITES;

typedef struct _STJITTER {
	BOOLEAN Antiv;
	UCHAR PosRepetida;
	UCHAR Margen;
	UCHAR Resistencia;
	UINT16 PosElegida;
} STJITTER, * PSTJITTER;

typedef struct _CALIBRADO_CONTEXT
{
	WDFWAITLOCK WaitLockCalibrado;
	STLIMITES	Limites[4];
	STJITTER	Jitter[4];
} CALIBRADO_CONTEXT;
#endif // _CONTEXT_
#ifdef _PUBLIC_
NTSTATUS ConfigurarCalibrado(_In_ WDFDEVICE device, _In_ WDFREQUEST request);
NTSTATUS ConfigurarAntivibracion(_In_ WDFDEVICE device, _In_ WDFREQUEST request);
VOID Calibrar(WDFDEVICE device, PHID_INPUT_DATA datosHID);
#endif // _PUBLIC_

