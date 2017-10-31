EXTERN_C_START

NTSTATUS Luz_MFD(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Luz_Global(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Luz_Info(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Pinkie(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Texto(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer, __in size_t tamBuffer);
NTSTATUS Set_Hora(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Hora24(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);
NTSTATUS Set_Fecha(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer);

#ifdef _PRIVATE_
typedef struct _WI_CONTEXT
{
	USHORT valor[18];
	UCHAR idx[18];
	UCHAR nordenes;
} WI_CONTEXT, *PWI_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(WI_CONTEXT, GetWIContext);

NTSTATUS EnviarOrdenesPassive(_In_ WDFDEVICE DeviceObject, _In_ USHORT* valor, UCHAR* idx, UCHAR nordenes);
VOID EnviarOrdenWI(_In_ WDFWORKITEM workItem);

NTSTATUS EnviarOrden(_In_ WDFDEVICE DeviceObject, _In_ UCHAR* params, _In_ UCHAR nparams);
#endif

EXTERN_C_END