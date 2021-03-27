#ifdef _CONTEXT_
typedef struct _X52WRITE_CONTEXT
{
	WDFWAITLOCK		WaitLockX52;

	WDFCOLLECTION	Ordenes;
	WDFWAITLOCK		WaitLockOrdenes;
	PETHREAD		Hilo;
} X52WRITE_CONTEXT;
#endif // _CONTEXT_

#ifdef _PUBLIC_
NTSTATUS Set_Texto(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer, __in size_t tamBuffer);
NTSTATUS EnviarOrden(_In_ WDFDEVICE DeviceObject, _In_ UCHAR* params, _In_ UCHAR nparams);
VOID LimpiarSalidaX52(WDFOBJECT  Object);
#endif // _PUBLIC_

#ifdef _PRIVATE_
typedef struct
{
	USHORT valor;
	UCHAR idx;
} ORDEN_X52, *PORDEN_X52;

KSTART_ROUTINE EnviarOrdenHilo;
#endif
