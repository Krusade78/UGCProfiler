#pragma once
#include <winioctl.h>
#include <queue>

#define IOCTL_X52		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0800, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_TEXTO		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0801, METHOD_BUFFERED, FILE_WRITE_ACCESS)


class CX52Salida
{
public:
	CX52Salida();
	~CX52Salida();
	static CX52Salida* Get() { return pLocal; }

	void Luz_MFD(PUCHAR SystemBuffer);
	void Luz_Global(PUCHAR SystemBuffer);
	void Luz_Info(PUCHAR SystemBuffer);
	void Set_Pinkie(PUCHAR SystemBuffer);
	void Set_Texto(PUCHAR SystemBuffer, BYTE tamBuffer);
	void Set_Hora(PUCHAR SystemBuffer);
	void Set_Hora24(PUCHAR SystemBuffer);
	void Set_Fecha(PUCHAR SystemBuffer);
private:
	USHORT Fecha = 0;

	typedef struct
	{
		DWORD ioCtl;
		PUCHAR buff;
		BYTE tam;
	} ORDEN, *PORDEN;

	static CX52Salida* pLocal;

	HANDLE semCola = nullptr;
	HANDLE semDriver = nullptr;
	PTP_WORK wkPool = nullptr;
	HANDLE hDriver = nullptr;
	std::queue<PORDEN> cola;

	bool AbrirDriver();
	void EnviarOrden(DWORD ioctl, UCHAR* params, BYTE tam);
	static VOID CALLBACK WkEnviar(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work);
};
