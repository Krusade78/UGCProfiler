#ifdef _CONTEXT_
typedef struct _PEDALESREAD_CONTEXT
{
	WDFWAITLOCK		WaitLockPosicion;
	BOOLEAN			Activado;
	UCHAR			UltimaPosicionFrenoI;
	UCHAR			UltimaPosicionFrenoD;
	UINT16			UltimaPosicionRz;
} PEDALESREAD_CONTEXT;
#endif //_CONTEXT_

#ifdef _PUBLIC_
NTSTATUS IniciarReportsUSBPedales(WDFIOTARGET ioTarget);
#endif // _PUBLIC_

#ifdef _PRIVATE_
#define TAM_REPORTPEDALES 4

EVT_WDF_REQUEST_COMPLETION_ROUTINE CompletionPedales;

VOID ProcesarEntradaPedales(_In_ WDFDEVICE device, _In_ PVOID buffer);
typedef struct _WI_CONTEXT
{
	UCHAR Buffer[TAM_REPORTPEDALES];
} WI_CONTEXT, * PWI_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(WI_CONTEXT, GetWIContext);
EVT_WDF_WORKITEM ProcesarEntradaPedalesWI;
#endif


