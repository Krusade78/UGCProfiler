#include "../framework.h"
#include "EscribirUSBX52.h"
#include <winusb.h>


CX52Salida* CX52Salida::pLocal = nullptr;

CX52Salida::CX52Salida()
{
	pLocal = this;
	semCola = CreateSemaphore(NULL, 1, 1, NULL);
	evCola = CreateEvent(NULL, TRUE, FALSE, NULL);
	CreateThread(NULL, 0, WkEnviar, this, 0, NULL);
}

CX52Salida::~CX52Salida()
{
	pLocal = nullptr;
	salir = true;
	WaitForSingleObject(semCola, INFINITE);
	{
		while (!cola.empty())
		{
			PORDEN orden = cola.front();
			delete orden;
			cola.pop();
		}
	}
	ReleaseSemaphore(semCola, 1, NULL);
	SetEvent(evCola);
	while (salir) { Sleep(500); }
	CloseHandle(semCola);
	CloseHandle(evCola);
}

#pragma region "Ordenes"
void CX52Salida::Luz_MFD(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { *(SystemBuffer) ,0 , 0xb1};
	EnviarOrden(params, 1);
}

void CX52Salida::Luz_Global(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { *(SystemBuffer),0 ,0xb2 };
	EnviarOrden(params, 1);
}

void CX52Salida::Luz_Info(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { static_cast<UCHAR>(*(SystemBuffer) + 0x50),0 , 0xb4 };
	EnviarOrden(params, 1);
}

void CX52Salida::Set_Pinkie(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { static_cast<UCHAR>(*(SystemBuffer) + 0x50),0 , 0xfd };
	EnviarOrden(params, 1);
}

void CX52Salida::Set_Texto(PUCHAR SystemBuffer, BYTE tamBuffer)
{
	UCHAR params[3 * 17];
	UCHAR texto[16];
	UCHAR nparams = 1;
	UCHAR paramIdx = 0;

	if ((tamBuffer - 1) > 16)
		return;

	RtlZeroMemory(texto, 16);
	RtlCopyMemory(texto, &(SystemBuffer)[1], tamBuffer - 1);


	params[0] = 0x0; params[1] = 0;
	switch (*(SystemBuffer)) //linea
	{
	case 1:
		params[2] = 0xd9;
		paramIdx = 0xd1;
		break;
	case 2:
		params[2] = 0xda;
		paramIdx = 0xd2;
		break;
	case 3:
		params[2] = 0xdc;
		paramIdx = 0xd4;
	}
	for (UCHAR i = 0; i < 16; i += 2)
	{
		if (texto[i] == 0)
			break;
		params[0 + (3 * nparams)] = texto[i];
		params[1 + (3 * nparams)] = texto[i + 1];
		params[2 + (3 * nparams)] = paramIdx;
		nparams++;
	}

	EnviarOrden(params, nparams);
}

void CX52Salida::Set_Hora(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { (SystemBuffer)[2] ,(SystemBuffer)[1] , static_cast<UCHAR>(*(SystemBuffer) + 0xbf) };
	EnviarOrden(params, 1);
}

void CX52Salida::Set_Hora24(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { (SystemBuffer)[2] ,static_cast<UCHAR>((SystemBuffer)[1] + 0x80), static_cast<UCHAR>(*(SystemBuffer) + 0xbf) };
	EnviarOrden(params, 1);
}

void CX52Salida::Set_Fecha(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { 0, 0, 0 };

	switch (SystemBuffer[0])
	{
	case 1:
		params[2] = 0xc4;
		params[1] = (UCHAR)(Fecha >> 8);
		params[0] = SystemBuffer[1];
		Fecha = *((USHORT*)params);
		break;
	case 2:
		params[2] = 0xc4;
		params[1] = SystemBuffer[1];
		params[0] = (UCHAR)(Fecha & 0xff);
		Fecha = *((USHORT*)params);
		break;
	case 3:
		params[2] = 0xc8;
		params[1] = 0;
		params[0] = SystemBuffer[1];
	}
	EnviarOrden(params, 1);
}
#pragma endregion

void CX52Salida::EnviarOrden(UCHAR* buffer, BYTE paquetes)
{
	for (BYTE procesados = 0; procesados < paquetes; procesados++)
	{
		PORDEN orden = new ORDEN;
		orden->valor = *((USHORT*)&buffer[procesados * 3]);
		orden->idx = buffer[2 + (procesados * 3)];

		WaitForSingleObject(semCola, INFINITE);
		{
			cola.push(orden);
			SetEvent(evCola);
		}
		ReleaseSemaphore(semCola, 1, NULL);
	}
}

DWORD WINAPI  CX52Salida::WkEnviar(LPVOID param)
{	
	CX52Salida* local = static_cast<CX52Salida*>(param);
	while (!local->salir)
	{
		PORDEN orden = nullptr;
		WaitForSingleObject(local->semCola, INFINITE);
		{
			if (!local->cola.empty())
			{
				orden = local->cola.front();
				local->cola.pop();
				ResetEvent(local->evCola);
			}
		}
		ReleaseSemaphore(local->semCola, 1, NULL);

		if (orden != nullptr)
		{
			WINUSB_SETUP_PACKET controlSetupPacket
			{
				controlSetupPacket.RequestType = 0b01000000,
				controlSetupPacket.Request = 0x91, // Request
				controlSetupPacket.Value = orden->valor, // Value
				controlSetupPacket.Index = orden->idx, // Index  
				controlSetupPacket.Length = 0
			};

			delete orden;

			if (InterlockedCompareExchangePointer(&local->wUSB, nullptr, nullptr) != nullptr)
			{
				WinUsb_ControlTransfer(local->wUSB, controlSetupPacket, NULL, 0, NULL, NULL);
			}
		}
		else
		{
			WaitForSingleObject(local->evCola, INFINITE);
		}
	}

	local->salir = false;
	return 0;
}