#ifdef _CONTEXT_
typedef struct _PEDALES_PNP_CONTEXT
{
	PVOID			PnPNotifyHandle;
	WCHAR			SymbolicLink[200];
	WDFIOTARGET		IoTarget;
	WDFWAITLOCK		WaitLockIoTarget;
} PEDALES_PNP_CONTEXT;
#endif //_CONTEXT_

#ifdef _PUBLIC_
VOID CerrarPedales(_In_ WDFDEVICE device);

DRIVER_NOTIFICATION_CALLBACK_ROUTINE PnPCallbackPedales;

VOID CerrarIoTarget(_In_ WDFDEVICE device);
#endif // _PUBLIC_

#ifdef _PRIVATE_
NTSTATUS IniciarIoTarget(_In_ WDFDEVICE device);

EVT_WDF_WORKITEM CerrarIoTargetWI;
VOID CerrarIoTargetPassive(_In_ WDFDEVICE device);

EVT_WDF_IO_TARGET_REMOVE_COMPLETE EvIoTargetRemoveComplete;
EVT_WDF_IO_TARGET_REMOVE_CANCELED EvIoTargetRemoveCanceled;
#endif
