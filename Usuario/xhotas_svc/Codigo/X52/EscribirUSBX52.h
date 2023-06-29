#pragma once
#include <queue>

class CX52Salida
{
public:
	CX52Salida();
	~CX52Salida();
	static CX52Salida* Get() { return pLocal; }

	void SetWinUSB(void* ptr) { InterlockedExchangePointer(&wUSB, ptr); }

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
		USHORT valor;
		BYTE idx;
	} ORDEN, *PORDEN;

	static CX52Salida* pLocal;

	bool salir = false;
	HANDLE semCola = nullptr;
	void* wUSB = nullptr;
	std::queue<PORDEN> cola;
	HANDLE evCola = nullptr;

	void EnviarOrden(UCHAR* params, BYTE paquetes);
	static DWORD WINAPI WkEnviar(LPVOID param);
};
