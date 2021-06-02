#include "../framework.h"
#include "EscribirHIDNXT.h"
#include <hidsdi.h>

CNXTSalida* CNXTSalida::pLocal = nullptr;

CNXTSalida::CNXTSalida()
{
	pLocal = this;
	semCola = CreateSemaphore(NULL, 1, 1, NULL);
	semDriver = CreateSemaphore(NULL, 1, 1, NULL);
	wkPool = CreateThreadpoolWork(WkEnviar, this, NULL);

	UCHAR cabeceraPaquete[8] = { 0x59, 0xA5, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x01 };
	RtlZeroMemory(paqueteHID, 0x81);
	RtlCopyMemory(paqueteHID, cabeceraPaquete, 8);
	RtlZeroMemory(&estadoLedBase, sizeof(estadoLedBase));
}

CNXTSalida::~CNXTSalida()
{
	pLocal = nullptr;
	WaitForSingleObject(semCola, INFINITE);
	{
		while (!cola.empty())
		{
			PORDEN orden = cola.front();
			delete[] orden->buff;
			delete orden;
			cola.pop();
		}
	}
	ReleaseSemaphore(semCola, 1, NULL);
	WaitForThreadpoolWorkCallbacks(wkPool, TRUE);
	CloseThreadpoolWork(wkPool);
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		if (rutaDriver != nullptr)
		{
			delete[] rutaDriver;
			rutaDriver = nullptr;
		}
	}
	ReleaseSemaphore(semDriver, 1, NULL);
	CloseHandle(semCola);
	CloseHandle(semDriver);
}

void CNXTSalida::SetRuta(wchar_t* ruta)
{
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		if (rutaDriver != nullptr)
		{
			delete rutaDriver;
			rutaDriver = nullptr;
		}
		if (ruta != nullptr)
		{
			size_t tam = wcsnlen_s(ruta, MAX_PATH) + 1;
			rutaDriver = new wchar_t[tam];
			RtlCopyMemory(rutaDriver, ruta, tam * sizeof(wchar_t));
			ReleaseSemaphore(semDriver, 1, NULL);
			AbrirDriver();
			return;
		}
	}
	ReleaseSemaphore(semDriver, 1, NULL);
}

bool CNXTSalida::AbrirDriver()
{
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		hDriver = CreateFile(rutaDriver, GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
		if (hDriver == INVALID_HANDLE_VALUE)
		{
			DWORD err = GetLastError();
			hDriver = nullptr;
			ReleaseSemaphore(semDriver, 1, NULL);
			return false;
		}
	}
	ReleaseSemaphore(semDriver, 1, NULL);
	return true;
}

void CNXTSalida::SetLed(UCHAR* params)
{
	if (params[0] == 0)
	{
		UCHAR modo = (params[3] >> 5) & 0x7;
		bool led2 = (modo == 0) || (modo == 5); //azul, color1
		bool led1 = (modo == 1) || (modo == 6); //rojo, color2
		if ((params[3] & 0b0001'1100) == 0)
		{
			if (led1) estadoLedBase.Base &= 0b0010;
			if (led2) estadoLedBase.Base &= 0b0001;
			if ((estadoLedBase.Base & 0b0011) != 0)
			{
				RtlCopyMemory(params, (estadoLedBase.Base == 1) ? estadoLedBase.Antiguo1 : estadoLedBase.Antiguo2 , 4);
			}
		}
		else
		{
			estadoLedBase.Base |= (led1) ? 1 : 0;
			estadoLedBase.Base |= (led2) ? 2 : 0;
			if (led1) RtlCopyMemory(estadoLedBase.Antiguo1, params, 4);
			if (led2) RtlCopyMemory(estadoLedBase.Antiguo2, params, 4);
		}
		if ((estadoLedBase.Base & 0b0011) == 3)
		{
			params[1] = estadoLedBase.Antiguo2[1];
			params[2] = estadoLedBase.Antiguo2[2] & 0b1;
			params[2] |= estadoLedBase.Antiguo1[2] & 0b1111'1110;
			params[3] &= 0b1111'1100;
			params[3] |= estadoLedBase.Antiguo1[3] & 0b11;
			params[3] = (params[3] & 0b0001'1111) | (4 << 5);
		}
	}
	EnviarOrden(params);
}

void CNXTSalida::EnviarOrden(UCHAR* buffer)
{
	PORDEN orden = new ORDEN;
	orden->buff = new BYTE[4];
	RtlCopyMemory(orden->buff, buffer, 4);

	WaitForSingleObject(semCola, INFINITE);
	{
		cola.push(orden);
	}
	ReleaseSemaphore(semCola, 1, NULL);
	SubmitThreadpoolWork(wkPool);
}

VOID CALLBACK CNXTSalida::WkEnviar(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work)
{
	if (Context != NULL)
	{
		CNXTSalida* local = static_cast<CNXTSalida*>(Context);
		PORDEN orden = nullptr;
		WaitForSingleObject(local->semCola, INFINITE);
		{
			if (!local->cola.empty())
			{
				orden = local->cola.front();
				local->cola.pop();
			}
		}
		ReleaseSemaphore(local->semCola, 1, NULL);

		if (orden != nullptr)
		{
			RtlCopyMemory(&local->paqueteHID[8], orden->buff, 4);
			*(WORD*)&local->paqueteHID[3] = local->CalcularCRC(&local->paqueteHID[5]);
			for (char i = 0; i < 2; i++)
			{
				WaitForSingleObject(local->semDriver, INFINITE);
				{
					if (local->hDriver != nullptr)
					{
						if (!HidD_SetFeature(local->hDriver, local->paqueteHID, 0x81))
						{
							DWORD err = GetLastError();
						}
						else
						{
							ReleaseSemaphore(local->semDriver, 1, NULL);
							break;
						}
					}
				}
				ReleaseSemaphore(local->semDriver, 1, NULL);
				if (!local->AbrirDriver())
				{
					break;
				}
			}
			delete[] orden->buff;
			delete orden;
		}
	}
}

WORD CNXTSalida::CalcularCRC(UCHAR* bloque)
{
	__int16 result = -1; // 0xffff;

	for (char i = 0; i < 6; i++)
	{
		UCHAR v5 = bloque[i];
		result = v5 ^ result;
		for (char j = 0; j < 8; j++)
		{		
			if ((result & 1) != 0)
			{
				result = (int)(unsigned __int16)result >> 1;
				result = result ^ 0xA001;
			}
			else
			{
				result = (int)(unsigned __int16)result >> 1;
			}
		}
	}
	return result;
}
