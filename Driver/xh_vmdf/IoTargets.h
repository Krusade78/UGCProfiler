EVT_WDF_WORKITEM IniciarIoTargets;
typedef UCHAR WI_CONTEXT, *PWI_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE(WI_CONTEXT)

void CerrarIoTargets();

EVT_WDF_TIMER	TimerTickTargets;

EVT_WDF_DPC		EvDpcAbrirCerrarTargets;
typedef struct _DPCTARGETS_CONTEXT
{
	WDFIOTARGET target;
	CHAR		tipo; // 1 Pedales, 2 HOTAS, 3 USB * Positivos abrir, negativos cerrar
} DPCTARGETS_CONTEXT, *PDPCTARGETS_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE(DPCTARGETS_CONTEXT)

#ifdef _IOTARGETS_
void IoTargetRemove(IN WDFIOTARGET IoTarget);

EVT_WDF_IO_TARGET_REMOVE_COMPLETE  EvIoTargetRemove;

void IniciarReportsPedales(IN WDFDEVICE Device);
void IniciarReportsHOTAS(IN WDFDEVICE Device, size_t tamBuffer);
WDFREQUEST IniciarRequestHID(IN WDFDEVICE Device, WDFIOTARGET IoTarget, size_t tamBuffer);

EVT_WDF_REQUEST_COMPLETION_ROUTINE  CompletionPedales;
EVT_WDF_REQUEST_COMPLETION_ROUTINE  CompletionHOTAS;

#endif