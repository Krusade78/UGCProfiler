EXTERN_C_START

DRIVER_NOTIFICATION_CALLBACK_ROUTINE PnPCallbackPedales;

NTSTATUS IniciarPedales(_In_ WDFDEVICE device);
VOID CerrarPedales(_In_ WDFDEVICE device);

#ifdef _PRIVATE_
NTSTATUS CerrarIoTarget(_In_ WDFDEVICE device);
VOID CerrarIoTargetPassive(_In_ WDFDEVICE device);
VOID CerrarIoTargetWI(_In_ WDFWORKITEM workItem);

NTSTATUS IniciarIoTargetPassive(_In_ WDFDEVICE device);
VOID IniciarIoTargetWI(_In_ WDFWORKITEM workItem);
NTSTATUS IniciarReports(WDFIOTARGET ioTarget);

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
