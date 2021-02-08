#pragma once
#include "../Perfil/CPerfil.h"
#include "../ColaEventos/CColaEventos.h"
#include "../ColaEntrada/CColaHID.h"
#include "ProcesarUSBs_Calibrado.h"
#include "CProcesarPedales.h"
#include "CProcesarX52.h"
#include "CProcesarNXT.h"

class CPreprocesar
{
public:
	CPreprocesar(CPerfil* pPerfil, CColaEventos* colaEv, CColaHID* cola);
	~CPreprocesar();

	bool Iniciar();

private:
	//HID original X52
	typedef struct _HIDX52_INPUT_DATA
	{
		UCHAR   EjesXYR[4];
		UCHAR	Ejes[4];
		UCHAR	Botones[4];
		UCHAR	Seta; // 2bits wheel + 2 blanco + 4 bits seta
		UCHAR	Ministick;
	} HIDX52_INPUT_DATA, * PHIDX52_INPUT_DATA;

	CPerfil* pPerfil = nullptr;
	CColaHID* colaHID = nullptr;

	short hiloCerrado = TRUE;
	bool salir = false;

	CProcesarPedales* pedales = nullptr;
	CProcesarX52* x52 = nullptr;
	CProcesarNXT* nxt = nullptr;

	static DWORD WINAPI HiloLectura(LPVOID param);

	void HIDPedales(UCHAR* datos);
	void HIDX52(PHIDX52_INPUT_DATA datos);
	void HIDNXT(UCHAR* datos);

	UCHAR Switch4To8(UCHAR in);
	void ConvertirEjeCentro0(UINT16* pos, UINT16 rango, UINT16 centro);
	void ConvertirEjeRango(UINT32 nuevoRango, INT16* pos, UINT16 rango);
};

