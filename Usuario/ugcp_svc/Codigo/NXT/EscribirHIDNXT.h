#pragma once
#include <queue>

class CNXTSalida
{
public:
	CNXTSalida();
	~CNXTSalida();
	static CNXTSalida* Get() { return pLocal; }

	void SetRuta(wchar_t* ruta);

	void SetLed(UCHAR* params);
private:
	typedef struct
	{
		PUCHAR buff;
	} ORDEN, * PORDEN;

	static CNXTSalida* pLocal;

	HANDLE semCola = nullptr;
	HANDLE semDriver = nullptr;
	PTP_WORK wkPool = nullptr;
	HANDLE hDriver = nullptr;
	wchar_t* rutaDriver = nullptr;
	std::queue<PORDEN> cola;

	UCHAR paqueteHID[0x81];

	struct
	{
		UCHAR Base;
		UCHAR Antiguo1[4];
		UCHAR Antiguo2[4];
	} estadoLedBase;

	bool AbrirDriver();
	void EnviarOrden(UCHAR* params);
	static VOID CALLBACK WkEnviar(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work);
	WORD CalcularCRC(UCHAR* bloque);
};

