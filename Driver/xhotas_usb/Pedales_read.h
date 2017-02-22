EXTERN_C_START

NTSTATUS IniciarPedales(_In_ WDFDEVICE device);
VOID CerrarPedales(_In_ WDFDEVICE device);

#ifdef _PRIVATE_
NTSTATUS CerrarIoTarget(_In_ WDFDEVICE device);
VOID CerrarIoTargetPassive(_In_ WDFDEVICE device);

#define HARDWARE_ID_PEDALES  L"\\??\\HID#Vid_044f&Pid_b653"
BOOLEAN IniciarIoTarget(_In_ PWSTR nombre, _In_ WDFDEVICE device);

#define TAM_REPORTPEDALES 8
NTSTATUS IniciarReports(WDFIOTARGET ioTarget);

//DWORD PnPCallback(
//	_In_ HCMNOTIFICATION    hNotify,
//	_In_opt_ PVOID          Context,
//	_In_ CM_NOTIFY_ACTION	Action,
//	_In_reads_bytes_(EventDataSize) PCM_NOTIFY_EVENT_DATA EventData,
//	_In_ DWORD              EventDataSize
//);
VOID EvIoTargetRemoveComplete(_In_ WDFIOTARGET ioTarget);
VOID EvIoTargetRemoveCanceled(_In_ WDFIOTARGET ioTarget);
VOID CompletionPedales(
	_In_ WDFREQUEST request,
	_In_ WDFIOTARGET ioTarget,
	_In_ PWDF_REQUEST_COMPLETION_PARAMS params,
	_In_ WDFCONTEXT context
);

VOID ProcesarEntradaPedales(_In_ WDFDEVICE device, _In_ PVOID buffer);
#endif

EXTERN_C_END
