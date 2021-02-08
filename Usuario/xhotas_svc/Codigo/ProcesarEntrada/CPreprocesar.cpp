#include "../framework.h"
#include "CPreprocesar.h"
#include "GenerarEventos/CGenerarEventos.h" //iniciar

CPreprocesar::CPreprocesar(CPerfil* pPerfil, CColaEventos* pColaEv, CColaHID* pCola)
{
	this->pPerfil = pPerfil;
	this->colaHID = pCola;
	CGenerarEventos::Iniciar(pPerfil, pColaEv);
	pedales = new CProcesarPedales(pPerfil);
	x52 = new CProcesarX52(pPerfil);
	nxt = new CProcesarNXT(pPerfil);
}

CPreprocesar::~CPreprocesar()
{
	salir = true;
	while (InterlockedCompareExchange16(&hiloCerrado, 0, 0) == FALSE) Sleep(1000);
	delete nxt;
	delete x52;
	delete pedales;
}

bool CPreprocesar::Iniciar()
{
	if (NULL != CreateThread(NULL, 0, HiloLectura, this, 0, NULL))
	{
		while (InterlockedCompareExchange16(&hiloCerrado, FALSE, FALSE))
		{
			Sleep(500);
		}
		return true;
	}

	return false;
}

DWORD WINAPI CPreprocesar::HiloLectura(LPVOID param)
{
	CPreprocesar* local = (CPreprocesar*)param;
	InterlockedExchange16(&local->hiloCerrado, FALSE);
	SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

	while (!local->salir)
	{
		CPaqueteHID* paq = local->colaHID->Leer();
		if (paq != nullptr)
		{
			if (paq->GetTipo() == TipoPaquete::Pedales)
			{
				local->HIDPedales(paq->GetDatos());
			}
			else if (paq->GetTipo() == TipoPaquete::X52)
			{
				local->HIDX52((PHIDX52_INPUT_DATA)paq->GetDatos());
			}
			else
			{
				local->HIDNXT(paq->GetDatos());
			}
			delete paq;
		}
	}

	InterlockedExchange16(&local->hiloCerrado, TRUE);
	return 0;
}

void CPreprocesar::HIDPedales(UCHAR* datos)
{
	VHID_INPUT_DATA hidData;
	RtlZeroMemory(&hidData, sizeof(VHID_INPUT_DATA));

	hidData.Ejes[3] = (UINT16)((((PUCHAR)datos)[2] >> 6) + (((PUCHAR)datos)[3] << 2)); //R
	hidData.Ejes[6] = (UCHAR)(((PUCHAR)datos)[1] & 0x7F); //frenoI
	hidData.Ejes[7] = (UCHAR)((((PUCHAR)datos)[1] >> 7) + ((((PUCHAR)datos)[2] & 0x3f) << 1)); //frenoD

	ConvertirEjeCentro0((UINT16*)&hidData.Ejes[3], 512, 256);
	ConvertirEjeCentro0((UINT16*)&hidData.Ejes[6], 128, 64);
	ConvertirEjeCentro0((UINT16*)&hidData.Ejes[7], 128, 64);

	// Calibrar
	if (!pPerfil->GetModoCalibrado())
	{
		CCalibrado::Calibrar(pPerfil, TipoJoy::Pedales, &hidData);
		ConvertirEjeRango(65536, &hidData.Ejes[3], 512);
		ConvertirEjeRango(256, &hidData.Ejes[6], 128);
		ConvertirEjeRango(256, &hidData.Ejes[7], 128);
	}

	pedales->Procesar(&hidData);
}

void CPreprocesar::HIDX52(PHIDX52_INPUT_DATA hidGameData)
{
	x52->PreProcesarModos(((hidGameData->Botones[3] & 3) << 1) | (hidGameData->Botones[2] >> 7));

	VHID_INPUT_DATA hidData_Joy, hidData_Ace;
	RtlZeroMemory(&hidData_Joy, sizeof(VHID_INPUT_DATA));
	RtlZeroMemory(&hidData_Ace, sizeof(VHID_INPUT_DATA));

	((PUCHAR)hidData_Joy.Ejes)[0] = hidGameData->EjesXYR[0];
	((PUCHAR)hidData_Joy.Ejes)[1] = hidGameData->EjesXYR[1] & 0x7;
	((PUCHAR)hidData_Joy.Ejes)[2] = (hidGameData->EjesXYR[1] >> 3) | ((hidGameData->EjesXYR[2] & 0x7) << 5);
	((PUCHAR)hidData_Joy.Ejes)[3] = (hidGameData->EjesXYR[2] >> 3) & 0x7;
	((PUCHAR)hidData_Joy.Ejes)[6] = (hidGameData->EjesXYR[2] >> 6) | ((hidGameData->EjesXYR[3] & 0x3f) << 2);
	((PUCHAR)hidData_Joy.Ejes)[7] = hidGameData->EjesXYR[3] >> 6;
	hidData_Ace.Ejes[2] = hidGameData->Ejes[0]; //Z
	hidData_Ace.Ejes[3] = hidGameData->Ejes[2];
	hidData_Ace.Ejes[4] = hidGameData->Ejes[1];
	hidData_Ace.Ejes[6] = hidGameData->Ejes[3];
	hidData_Joy.Botones[0] = ((hidGameData->Botones[1] >> 6) & 1) | ((hidGameData->Botones[0] << 1) & 4) | ((hidGameData->Botones[0] >> 2) & 8) | (hidGameData->Botones[0] & 16);
	hidData_Joy.Botones[0] |= ((hidGameData->Botones[0] & 1) << 1) |((hidGameData->Botones[2] & 0x80) >> 2) | ((hidGameData->Botones[3] & 3) << 6) ; //trg 1, modos
	hidData_Joy.Botones[1] = ((hidGameData->Botones[1] & 0x3f) << 2) | ((hidGameData->Botones[0] >> 2) & 3); //a,b, toggles
	hidData_Ace.Botones[0] = ((hidGameData->Botones[0] & 128) >> 6) | ((hidGameData->Botones[0] >> 5) & 2) | (hidGameData->Botones[3] & 252);
	hidData_Ace.Botones[1] = hidGameData->Seta & 0x3; //wheel
	RtlCopyMemory(hidData_Ace.Botones, hidData_Joy.Botones, sizeof(hidData_Joy.Botones));
	hidData_Ace.Setas[0] = hidGameData->Seta >> 4;
	hidData_Joy.Setas[0] = Switch4To8((hidGameData->Botones[1] >> 7) + ((hidGameData->Botones[2] << 1) & 0xf));
	hidData_Joy.Setas[1] = Switch4To8((hidGameData->Botones[2] >> 3) & 0xf);
	switch (hidGameData->Ministick & 0xf)
	{
	case 0:
		hidData_Ace.Setas[1] = 8;
		break;
	case 0xf:
		hidData_Ace.Setas[1] = 2;
		break;
	default: hidData_Ace.Setas[1] = 0;
	}
	switch (hidGameData->Ministick >> 4)
	{
	case 0:
		hidData_Ace.Setas[1] |= 1;
		break;
	case 0xf:
		hidData_Ace.Setas[1] |= 4;
		break;
	}
	hidData_Ace.Setas[1] = Switch4To8(hidData_Ace.Setas[1]);
	hidData_Ace.Ejes[0] = hidGameData->Ministick & 0xf;
	hidData_Ace.Ejes[1] = hidGameData->Ministick >> 4;

	ConvertirEjeCentro0((UINT16*)&hidData_Joy.Ejes[0], 2048, 1024); //X
	ConvertirEjeCentro0((UINT16*)&hidData_Joy.Ejes[1], 2048, 1024); //Y
	ConvertirEjeCentro0((UINT16*)&hidData_Joy.Ejes[3], 1024, 512); //Rx
	ConvertirEjeCentro0((UINT16*)&hidData_Ace.Ejes[2], 256, 128); //Z
	ConvertirEjeCentro0((UINT16*)&hidData_Ace.Ejes[5], 256, 128); //Sl
	ConvertirEjeCentro0((UINT16*)&hidData_Ace.Ejes[3], 256, 128); //Rx
	ConvertirEjeCentro0((UINT16*)&hidData_Ace.Ejes[4], 256, 128); //Ry
	ConvertirEjeCentro0((UINT16*)&hidData_Ace.Ejes[6], 16, 8); //mX
	ConvertirEjeCentro0((UINT16*)&hidData_Ace.Ejes[7], 16, 8); //mY

	// Calibrar
	if (!pPerfil->GetModoCalibrado())
	{
		CCalibrado::Calibrar(pPerfil, TipoJoy::X52_Joy, &hidData_Joy);
		CCalibrado::Calibrar(pPerfil, TipoJoy::X52_Ace, &hidData_Ace);
		ConvertirEjeRango(65536, &hidData_Joy.Ejes[0], 2048); //X
		ConvertirEjeRango(65536, &hidData_Joy.Ejes[1], 2048); //Y
		ConvertirEjeRango(65536, &hidData_Joy.Ejes[3], 1024); //Rx
		ConvertirEjeRango(65536, &hidData_Ace.Ejes[2], 256); //Z
		ConvertirEjeRango(65536, &hidData_Ace.Ejes[5], 256); //Sl
		ConvertirEjeRango(65536, &hidData_Ace.Ejes[3], 256); //Rx
		ConvertirEjeRango(65536, &hidData_Ace.Ejes[4], 256); //Ry
		ConvertirEjeRango(256, &hidData_Ace.Ejes[6], 16); //mX
		ConvertirEjeRango(256, &hidData_Ace.Ejes[7], 16); //mY
	}

	x52->Procesar_Joy(&hidData_Joy);
	x52->Procesar_Ace(&hidData_Ace);
}

void CPreprocesar::HIDNXT(UCHAR* datos)
{
	//// Calibrar
	//if (!pPerfil->GetModoCalibrado())
	//{
	//	CCalibrado::Calibrar(pPerfil, TipoJoy::Pedales, &hidData);
	//}
	nxt->Procesar();
}

UCHAR CPreprocesar::Switch4To8(UCHAR in)
{
	switch (in)
	{
		case 0: return 0;
		case 1: return 1;
		case 2: return 3;
		case 3: return 2;
		case 4: return 5;
		case 6: return 4;
		case 8: return 7;
		case 9: return 8;
		case 12: return 6;
		default: return 0;
	}
}

void CPreprocesar::ConvertirEjeCentro0(UINT16* pos, UINT16 rango, UINT16 centro)
{
	UINT16 max = (rango / 2) - 1;
	bool negativo = false;

	if (*pos > centro)
	{
		*pos -= centro;
	}
	else if (*pos < centro)
	{
		*pos = centro - *pos;
		negativo = true;
	}
	else
	{
		*pos = 0;
	}
	if (*pos > max)
	{
		*pos = max;
	}

	if (negativo)
	{
		*pos = (rango - *pos);
	}
}

void CPreprocesar::ConvertirEjeRango(UINT32 nuevoRango, INT16* pos, UINT16 rango)
{
	*pos *= nuevoRango / rango;
}
