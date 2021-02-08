#pragma once
#define IOCTL_TEXTO			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0804, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0805, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_HORA24		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0806, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_FECHA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0807, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_RAW		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0808, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_CALIBRADO	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0809, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_ANTIVIBRACION	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x080e, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_MAPA		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x080a, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_COMANDOS	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x080b, METHOD_BUFFERED, FILE_WRITE_ACCESS)
//typedef struct _X52WRITE_CONTEXT
//{
//	WDFWAITLOCK		WaitLockX52;
//	USHORT			Fecha;
//
//	WDFCOLLECTION	Ordenes;
//	WDFWAITLOCK		WaitLockOrdenes;
//	PETHREAD		Hilo;
//} X52WRITE_CONTEXT;

class CX52Salida
{
public:
	static void Luz_MFD(PUCHAR SystemBuffer);
	static void Luz_Global(PUCHAR SystemBuffer);
	static void Luz_Info(PUCHAR SystemBuffer);
	static void Set_Pinkie(PUCHAR SystemBuffer);
	static void Set_Texto(PUCHAR SystemBuffer, __in size_t tamBuffer);
	static void Set_Hora(PUCHAR SystemBuffer);
	static void Set_Hora24(PUCHAR SystemBuffer);
	static void Set_Fecha(PUCHAR SystemBuffer);
	//VOID LimpiarSalidaX52(WDFOBJECT  Object);
private:
	typedef struct
	{
		USHORT valor;
		UCHAR idx;
	} ORDEN_X52, * PORDEN_X52;

	static void EnviarOrden(UCHAR* params, UCHAR nparams);
	////NTSTATUS EnviarOrdenUSB(_In_ WDFDEVICE DeviceObject, _In_ USHORT* valor, UCHAR* idx, UCHAR nordenes);
	////EVT_WDF_WORKITEM EnviarOrdenWI;
	//KSTART_ROUTINE EnviarOrdenHilo;
};
