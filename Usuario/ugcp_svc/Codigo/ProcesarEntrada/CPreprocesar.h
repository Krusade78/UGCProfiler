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
	CPreprocesar(CPerfil* pPerfil, CColaEventos* colaEv);
	~CPreprocesar();

	bool Iniciar();
	void AñadirACola(UCHAR* buff, DWORD tam) { colaHID->Añadir(buff, tam); };

private:
	//HID original X52
	typedef struct
	{
		UCHAR   EjesXYR[4];
		UCHAR	Ejes[4];
		UCHAR	Botones[4];
		UCHAR	Seta; // 2bits wheel + 2 blanco + 4 bits seta
		UCHAR	Ministick;
	} HIDX52_INPUT_DATA, * PHIDX52_INPUT_DATA;

	//HID original NXT
	typedef struct
	{
		UCHAR   EjeX[2];
		UCHAR	EjeY[2];
		UCHAR	EjeR[2];
		UCHAR	EjeZ[2];
		UCHAR	EjeMx[2];
		UCHAR	EjeMy[2];
		UCHAR	hidNoUsado1[4];
		UCHAR	Encoders;
		UCHAR	hidNoUsado[30];
		UCHAR	Base[1];
		UCHAR	Botones[5];
		UCHAR	sinUso[10];

	} HIDNXT_INPUT_DATA, * PHIDNXT_INPUT_DATA;

	CPerfil* pPerfil = nullptr;
	CColaHID* colaHID = nullptr;

	short hiloCerrado = TRUE;
	bool salir = false;

	bool posFijada[4][8];
	INT16 posFija[4][8];

	CProcesarPedales* pedales = nullptr;
	CProcesarX52* x52 = nullptr;
	CProcesarNXT* nxt = nullptr;

	static DWORD WINAPI HiloLectura(LPVOID param);

	void HIDPedales(UCHAR* datos);
	void HIDX52(PHIDX52_INPUT_DATA datos);
	void HIDNXT(UCHAR* datos);

	UCHAR Switch4To8(UCHAR in);
};

