#ifdef _CONTEXT_
typedef struct _X52WRITE_CONTEXT
{
	WDFWAITLOCK		WaitLockX52;
	USHORT			Fecha;

	WDFCOLLECTION	Ordenes;
	WDFWAITLOCK		WaitLockOrdenes;
	PETHREAD		Hilo;
} X52WRITE_CONTEXT;
#endif // _CONTEXT_

#ifdef _PUBLIC_
NTSTATUS Luz_MFD(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Luz_Global(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Luz_Info(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Pinkie(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Texto(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer, __in size_t tamBuffer);
NTSTATUS Set_Hora(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Hora24(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Fecha(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
VOID LimpiarSalidaX52(WDFOBJECT  Object);
#endif // _PUBLIC_

#ifdef _PRIVATE_
typedef struct
{
	USHORT valor;
	UCHAR idx;
} ORDEN_X52, *PORDEN_X52;

NTSTATUS EnviarOrden(_In_ WDFDEVICE DeviceObject, _In_ UCHAR* params, _In_ UCHAR nparams);
//NTSTATUS EnviarOrdenUSB(_In_ WDFDEVICE DeviceObject, _In_ USHORT* valor, UCHAR* idx, UCHAR nordenes);
//EVT_WDF_WORKITEM EnviarOrdenWI;
KSTART_ROUTINE EnviarOrdenHilo;
#endif
