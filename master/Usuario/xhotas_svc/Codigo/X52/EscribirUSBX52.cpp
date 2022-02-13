#include "../framework.h"
#include "EscribirUSBX52.h"

CX52Salida* CX52Salida::pLocal = nullptr;

CX52Salida::CX52Salida()
{
	pLocal = this;
	semCola = CreateSemaphore(NULL, 1, 1, NULL);
	semDriver = CreateSemaphore(NULL, 1, 1, NULL);
	wkPool = CreateThreadpoolWork(WkEnviar, this, NULL);
	AbrirDriver();
}

CX52Salida::~CX52Salida()
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
	if (hDriver != nullptr)
	{
		CloseHandle(hDriver);
		hDriver = nullptr;
	}
	CloseHandle(semCola);
	CloseHandle(semDriver);
}

bool CX52Salida::AbrirDriver()
{
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		hDriver = CreateFile(L"\\\\.\\X52_XHOTASControl", GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
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

#pragma region "Ordenes"
void CX52Salida::Luz_MFD(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { *(SystemBuffer) ,0 , 0xb1};
	EnviarOrden(IOCTL_X52, params, 3);
}

void CX52Salida::Luz_Global(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { *(SystemBuffer),0 ,0xb2 };
	EnviarOrden(IOCTL_X52, params, 3);
}

void CX52Salida::Luz_Info(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { static_cast<UCHAR>(*(SystemBuffer) + 0x50),0 , 0xb4 };
	EnviarOrden(IOCTL_X52, params, 3);
}

void CX52Salida::Set_Pinkie(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { static_cast<UCHAR>(*(SystemBuffer) + 0x50),0 , 0xfd };
	EnviarOrden(IOCTL_X52, params, 3);
}

void CX52Salida::Set_Texto(PUCHAR SystemBuffer, BYTE tamBuffer)
{
	EnviarOrden(IOCTL_TEXTO, SystemBuffer, tamBuffer);
}

void CX52Salida::Set_Hora(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { (SystemBuffer)[2] ,(SystemBuffer)[1] , static_cast<UCHAR>(*(SystemBuffer) + 0xbf) };
	EnviarOrden(IOCTL_X52, params, 3);
}

void CX52Salida::Set_Hora24(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { (SystemBuffer)[2] ,static_cast<UCHAR>((SystemBuffer)[1] + 0x80), static_cast<UCHAR>(*(SystemBuffer) + 0xbf) };
	EnviarOrden(IOCTL_X52, params, 3);
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
	EnviarOrden(IOCTL_X52, params, 3);
}
#pragma endregion

void CX52Salida::EnviarOrden(DWORD dwIoControlCode, UCHAR* buffer, BYTE tamaño)
{
	PORDEN orden = new ORDEN;
	orden->ioCtl = dwIoControlCode;
	orden->buff = new BYTE[tamaño];
	RtlCopyMemory(orden->buff, buffer, tamaño);
	orden->tam = tamaño;

	WaitForSingleObject(semCola, INFINITE);
	{
		cola.push(orden);
	}
	ReleaseSemaphore(semCola, 1, NULL);
	SubmitThreadpoolWork(wkPool);
}

VOID CALLBACK CX52Salida::WkEnviar(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work)
{	
	if (Context != NULL)
	{
		CX52Salida* local = static_cast<CX52Salida*>(Context);
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
			DWORD tam = 0;
			if (!DeviceIoControl(local->hDriver, orden->ioCtl, orden->buff, orden->tam, NULL, 0, &tam, NULL))
			{
				DWORD err = GetLastError();
				if (local->AbrirDriver())
				{
					DeviceIoControl(local->hDriver, orden->ioCtl, orden->buff, orden->tam, NULL, 0, &tam, NULL);
				}
			}
			delete[] orden->buff;
			delete orden;
		}
	}
}