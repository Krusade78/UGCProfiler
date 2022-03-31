#include "../framework.h"
#include "CPreprocesar.h"
#include "GenerarEventos/CGenerarEventos.h" //iniciar

CPreprocesar::CPreprocesar(CPerfil* pPerfil, CColaEventos* pColaEv)
{
	this->pPerfil = pPerfil;
	this->colaHID = new CColaHID();
	CGenerarEventos::Iniciar(pPerfil, pColaEv);
	pedales = new CProcesarPedales(pPerfil);
	x52 = new CProcesarX52(pPerfil);
	nxt = new CProcesarNXT(pPerfil);
	RtlZeroMemory(posFijada, 32 * sizeof(bool));
}

CPreprocesar::~CPreprocesar()
{
	salir = true;
	delete colaHID;
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
				local->HIDX52((PHIDX52_INPUT_DATA)(paq->GetDatos() + 1));
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

	// Calibrar
	if (!pPerfil->GetModoCalibrado())
	{
		CCalibrado::Calibrar(pPerfil, TipoJoy::Pedales, &hidData);
	}
	if (!pPerfil->GetModoRaw() && !pPerfil->GetModoCalibrado())
	{
		ConvertirEjeCentroMitad((UINT16*)&hidData.Ejes[3], 511, 255);
		ConvertirEjeCentroMitad((UINT16*)&hidData.Ejes[6], 127, 63);
		ConvertirEjeCentroMitad((UINT16*)&hidData.Ejes[7], 127, 63);

		ConvertirEjeRango(TipoJoy::Pedales, 3, &hidData.Ejes[3], 512);
		ConvertirEjeRango(TipoJoy::Pedales, 6, &hidData.Ejes[6], 128);
		ConvertirEjeRango(TipoJoy::Pedales, 7, &hidData.Ejes[7], 128);
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
	hidData_Ace.Ejes[2] = 255 - hidGameData->Ejes[0]; //Z
	hidData_Ace.Ejes[3] = hidGameData->Ejes[2];
	hidData_Ace.Ejes[4] = hidGameData->Ejes[1];
	hidData_Ace.Ejes[5] = hidGameData->Ejes[3];
	hidData_Joy.Botones[0] = ((hidGameData->Botones[1] >> 6) & 1) | ((hidGameData->Botones[0] << 1) & 4) | ((hidGameData->Botones[0] >> 2) & 8) | (hidGameData->Botones[0] & 16);
	hidData_Joy.Botones[0] |= ((hidGameData->Botones[0] & 1) << 1) |((hidGameData->Botones[2] & 0x80) >> 2) | ((hidGameData->Botones[3] & 3) << 6) ; //trg 1, modos
	hidData_Joy.Botones[1] = ((hidGameData->Botones[1] & 0x3f) << 2) | ((hidGameData->Botones[0] >> 2) & 3); //a,b, toggles
	hidData_Ace.Botones[0] = ((hidGameData->Botones[0] & 192) >> 6) | (hidGameData->Botones[3] & 252);
	hidData_Ace.Botones[1] = hidGameData->Seta & 0x3; //wheel
	hidData_Joy.Setas[0] = hidGameData->Seta >> 4;
	hidData_Joy.Setas[1] = Switch4To8((hidGameData->Botones[1] >> 7) + ((hidGameData->Botones[2] << 1) & 0xf));
	hidData_Ace.Setas[0] = Switch4To8((hidGameData->Botones[2] >> 3) & 0xf);
	if ((hidGameData->Ministick & 0xf) < 2)
	{
		hidData_Ace.Setas[1] = 8;
	}
	else if ((hidGameData->Ministick & 0xf) > 0xd)
	{
		hidData_Ace.Setas[1] = 2;
	}
	if ((hidGameData->Ministick >> 4) < 2)
	{
		hidData_Ace.Setas[1] |= 1;
	}
	else if ((hidGameData->Ministick >> 4) > 0xd)
	{
		hidData_Ace.Setas[1] |= 4;
	}
	hidData_Ace.Setas[1] = Switch4To8(hidData_Ace.Setas[1]);
	hidData_Ace.Ejes[6] = hidGameData->Ministick & 0xf; //x
	hidData_Ace.Ejes[7] = hidGameData->Ministick >> 4; //y

	// Calibrar
	if (!pPerfil->GetModoCalibrado())
	{
		CCalibrado::Calibrar(pPerfil, TipoJoy::X52_Joy, &hidData_Joy);
		CCalibrado::Calibrar(pPerfil, TipoJoy::X52_Ace, &hidData_Ace);
	}
	if (!pPerfil->GetModoRaw() && !pPerfil->GetModoCalibrado())
	{
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[0], 2047, 1023); //X
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[1], 2047, 1023); //Y
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[3], 1023, 511); //Rx
		ConvertirEjeCentroMitad((UINT16*)&hidData_Ace.Ejes[2], 255, 127); //Z
		ConvertirEjeCentroMitad((UINT16*)&hidData_Ace.Ejes[5], 255, 127); //Sl
		ConvertirEjeCentroMitad((UINT16*)&hidData_Ace.Ejes[3], 255, 127); //Rx
		ConvertirEjeCentroMitad((UINT16*)&hidData_Ace.Ejes[4], 255, 127); //Ry
		ConvertirEjeCentroMitad((UINT16*)&hidData_Ace.Ejes[6], 15, 8); //mX
		ConvertirEjeCentroMitad((UINT16*)&hidData_Ace.Ejes[7], 15, 8); //mY

		ConvertirEjeRango(TipoJoy::X52_Joy, 0, &hidData_Joy.Ejes[0], 2048); //X
		ConvertirEjeRango(TipoJoy::X52_Joy, 1, &hidData_Joy.Ejes[1], 2048); //Y
		ConvertirEjeRango(TipoJoy::X52_Joy, 3, &hidData_Joy.Ejes[3], 1024); //Rx
		ConvertirEjeRango(TipoJoy::X52_Ace, 2, &hidData_Ace.Ejes[2], 256); //Z
		ConvertirEjeRango(TipoJoy::X52_Ace, 5, &hidData_Ace.Ejes[5], 256); //Sl
		ConvertirEjeRango(TipoJoy::X52_Ace, 3, &hidData_Ace.Ejes[3], 256); //Rx
		ConvertirEjeRango(TipoJoy::X52_Ace, 4, &hidData_Ace.Ejes[4], 256); //Ry
		ConvertirEjeRango(TipoJoy::X52_Ace, 6, &hidData_Ace.Ejes[6], 16); //mX
		ConvertirEjeRango(TipoJoy::X52_Ace, 7, &hidData_Ace.Ejes[7], 16); //mY
	}

	x52->Procesar_Joy(&hidData_Joy);
	x52->Procesar_Ace(&hidData_Ace);
}

void CPreprocesar::HIDNXT(UCHAR* datos)
{
	if (datos[0] != 1)
	{
		return;
	}

	PHIDNXT_INPUT_DATA hidData = (PHIDNXT_INPUT_DATA)&datos[1];
	VHID_INPUT_DATA hidData_Joy;
	RtlZeroMemory(&hidData_Joy, sizeof(VHID_INPUT_DATA));

	hidData_Joy.Ejes[0] = *((INT16*)&hidData->EjeX);
	hidData_Joy.Ejes[1] = *((INT16*)&hidData->EjeY);
	hidData_Joy.Ejes[2] = *((INT16*)&hidData->EjeZ);
	hidData_Joy.Ejes[3] = *((INT16*)&hidData->EjeR);
	hidData_Joy.Ejes[6] = *((INT16*)&hidData->EjeMx);
	hidData_Joy.Ejes[7] = *((INT16*)&hidData->EjeMy);
	hidData_Joy.Botones[0] = hidData->Encoders | ((hidData->Base[0] & 0x10) << 2) | ((hidData->Base[0] & 0x40) >> 2) | (hidData->Base[0] & 0x20);
	hidData_Joy.Botones[1] = hidData->Botones[0] | ((hidData->Botones[3] & 1) << 5) | (((hidData->Botones[3] >> 3) & 1) << 6);
	if (hidData_Joy.Ejes[6] < 128)
	{
		hidData_Joy.Setas[0] = 8;
	}
	else if (hidData_Joy.Ejes[6] > 866)
	{
		hidData_Joy.Setas[0] = 2;
	}
	if (hidData_Joy.Ejes[7] < 128)
	{
		hidData_Joy.Setas[0] |= 4;
	}
	else if (hidData_Joy.Ejes[7] > 866)
	{
		hidData_Joy.Setas[0] |= 1;
	}
	hidData_Joy.Setas[0] = Switch4To8(hidData_Joy.Setas[0]);
	hidData_Joy.Setas[1] = nxt->ConvertirSeta(hidData->Botones[2] & 0x0f);
	hidData_Joy.Setas[2] = nxt->ConvertirSeta(hidData->Botones[1] >> 4);
	hidData_Joy.Setas[3] = nxt->ConvertirSeta(hidData->Botones[2] >> 4);

	// Calibrar
	if (!pPerfil->GetModoCalibrado())
	{
		CCalibrado::Calibrar(pPerfil, TipoJoy::NXT, &hidData_Joy);
	}
	if (!pPerfil->GetModoRaw() && !pPerfil->GetModoCalibrado())
	{
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[0], 4095, 2048); //X
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[1], 4095, 2048); //Y
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[2], 2047, 1024); //Z
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[3], 4095, 2048); //Rx
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[6], 1023, 512); //mX
		ConvertirEjeCentroMitad((UINT16*)&hidData_Joy.Ejes[7], 1023, 512); //mY

		ConvertirEjeRango(TipoJoy::NXT, 0, &hidData_Joy.Ejes[0], 4096); //X
		ConvertirEjeRango(TipoJoy::NXT, 1, &hidData_Joy.Ejes[1], 4096); //Y
		ConvertirEjeRango(TipoJoy::NXT, 2, &hidData_Joy.Ejes[2], 2048); //Z
		ConvertirEjeRango(TipoJoy::NXT, 3, &hidData_Joy.Ejes[3], 4096); //Rx
		ConvertirEjeRango(TipoJoy::NXT, 6, &hidData_Joy.Ejes[6], 1024); //mX
		ConvertirEjeRango(TipoJoy::NXT, 7, &hidData_Joy.Ejes[7], 1024); //mY
	}

	nxt->Procesar(&hidData_Joy);
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

//void CPreprocesar::ConvertirEjeCentro0(UINT16* pos, UINT16 rango, UINT16 centro)
//{
//	UINT16 max = (rango / 2) - 1;
//	bool negativo = false;
//
//	if (*pos > centro)
//	{
//		*pos -= centro;
//	}
//	else if (*pos < centro)
//	{
//		*pos = centro - *pos;
//		negativo = true;
//	}
//	else
//	{
//		*pos = 0;
//	}
//	if (*pos > max)
//	{
//		*pos = max;
//	}
//
//	if (negativo)
//	{
//		*pos = (65535 - *pos) + 1;
//	}
//}
void CPreprocesar::ConvertirEjeCentroMitad(UINT16* pos, UINT16 maximo, UINT16 centro)
{
	if ((maximo - centro) > centro)
	{
		*pos += 1;
		if (*pos == 1)
		{
			*pos = 0;
		}
	}
	else if (*pos == maximo)
	{
		*pos = maximo + 1;
	}
}

void CPreprocesar::ConvertirEjeRango(TipoJoy joy, char eje, UINT16* pos, UINT16 rango)
{
	UINT16 nuevoRango;
	UINT32 npos = *pos;
	UCHAR preciso = 0;
	pPerfil->InicioLecturaPr();
	nuevoRango = pPerfil->GetPr()->RangosEntrada[static_cast<UCHAR>(joy)][eje];
	pPerfil->FinLecturaPr();
	pPerfil->LockEstado();
	preciso = pPerfil->GetEstado()->ModoPrecisoEje[static_cast<UCHAR>(joy)][eje];
	pPerfil->UnlockEstado();
	if (preciso)
	{
		if (!posFijada[static_cast<UCHAR>(joy)][eje])
		{
			posFijada[static_cast<UCHAR>(joy)][eje] = true;
			posFija[static_cast<UCHAR>(joy)][eje] = npos;
		}
		else
		{
			UINT32 nuevoRangoP = (((nuevoRango / rango) / preciso) * rango);
			npos = ((posFija[static_cast<UCHAR>(joy)][eje] * nuevoRango) / rango) + (((npos - posFija[static_cast<UCHAR>(joy)][eje]) * nuevoRangoP) / rango);
			*pos = static_cast<UINT16>(npos);
		}
	}
	else
	{
		posFijada[static_cast<UCHAR>(joy)][eje] = false;
		if (nuevoRango != rango)
		{
			npos = (npos * nuevoRango) / rango;
			*pos = static_cast<UINT16>(npos);
		}
	}
}
