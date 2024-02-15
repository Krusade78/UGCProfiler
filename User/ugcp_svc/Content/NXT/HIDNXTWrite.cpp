#include "../framework.h"
#include "HIDNXTWrite.h"
#include <hidsdi.h>

CNXTWrite* CNXTWrite::pLocal = nullptr;

CNXTWrite::CNXTWrite()
{
	pLocal = this;
	semQueue = CreateSemaphore(NULL, 1, 1, NULL);
	semDriver = CreateSemaphore(NULL, 1, 1, NULL);
	wkPool = CreateThreadpoolWork(WkSend, this, NULL);

	UCHAR packetHeader[8] = { 0x59, 0xA5, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x01 };
	RtlZeroMemory(hidPacket, 0x81);
	RtlCopyMemory(hidPacket, packetHeader, 8);
	RtlZeroMemory(&statusBaseLed, sizeof(statusBaseLed));
}

CNXTWrite::~CNXTWrite()
{
	pLocal = nullptr;
	WaitForSingleObject(semQueue, INFINITE);
	{
		while (!queue.empty())
		{
			PORDEN order = queue.front();
			delete[] order->buff;
			delete order;
			queue.pop();
		}
	}
	ReleaseSemaphore(semQueue, 1, NULL);
	WaitForThreadpoolWorkCallbacks(wkPool, TRUE);
	CloseThreadpoolWork(wkPool);
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		if (pathDriver != nullptr)
		{
			delete[] pathDriver;
			pathDriver = nullptr;
		}
	}
	ReleaseSemaphore(semDriver, 1, NULL);
	CloseHandle(semQueue);
	CloseHandle(semDriver);
}

void CNXTWrite::SetPath(wchar_t* path)
{
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		if (pathDriver != nullptr)
		{
			delete pathDriver;
			pathDriver = nullptr;
		}
		if (path != nullptr)
		{
			size_t size = wcsnlen_s(path, MAX_PATH) + 1;
			pathDriver = new wchar_t[size];
			RtlCopyMemory(pathDriver, path, size * sizeof(wchar_t));
			ReleaseSemaphore(semDriver, 1, NULL);
			OpenDriver();
			return;
		}
	}
	ReleaseSemaphore(semDriver, 1, NULL);
}

bool CNXTWrite::OpenDriver()
{
	WaitForSingleObject(semDriver, INFINITE);
	{
		if (hDriver != nullptr)
		{
			CloseHandle(hDriver);
			hDriver = nullptr;
		}
		hDriver = CreateFile(pathDriver, GENERIC_WRITE | GENERIC_READ, FILE_SHARE_WRITE | FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
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

void CNXTWrite::SetLed(UCHAR* params)
{
	if (params[0] == 0)
	{
		UCHAR mode = (params[3] >> 5) & 0x7;
		bool led2 = (mode == 0) || (mode == 5); //azul, color1
		bool led1 = (mode == 1) || (mode == 6); //rojo, color2
		if ((params[3] & 0b0001'1100) == 0)
		{
			if (led1) statusBaseLed.Base &= 0b0010;
			if (led2) statusBaseLed.Base &= 0b0001;
			if ((statusBaseLed.Base & 0b0011) != 0)
			{
				RtlCopyMemory(params, (statusBaseLed.Base == 1) ? statusBaseLed.Old1 : statusBaseLed.Old2 , 4);
			}
		}
		else
		{
			statusBaseLed.Base |= (led1) ? 1 : 0;
			statusBaseLed.Base |= (led2) ? 2 : 0;
			if (led1) RtlCopyMemory(statusBaseLed.Old1, params, 4);
			if (led2) RtlCopyMemory(statusBaseLed.Old2, params, 4);
		}
		if ((statusBaseLed.Base & 0b0011) == 3)
		{
			params[1] = statusBaseLed.Old2[1];
			params[2] = statusBaseLed.Old2[2] & 0b1;
			params[2] |= statusBaseLed.Old1[2] & 0b1111'1110;
			params[3] &= 0b1111'1100;
			params[3] |= statusBaseLed.Old1[3] & 0b11;
			params[3] = (params[3] & 0b0001'1111) | (4 << 5);
		}
	}
	SendOrder(params);
}

void CNXTWrite::SendOrder(UCHAR* buffer)
{
	PORDEN order = new ORDEN;
	order->buff = new BYTE[4];
	RtlCopyMemory(order->buff, buffer, 4);

	WaitForSingleObject(semQueue, INFINITE);
	{
		queue.push(order);
	}
	ReleaseSemaphore(semQueue, 1, NULL);
	SubmitThreadpoolWork(wkPool);
}

VOID CALLBACK CNXTWrite::WkSend(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_WORK Work)
{
	if (Context != NULL)
	{
		CNXTWrite* local = static_cast<CNXTWrite*>(Context);
		PORDEN order = nullptr;
		WaitForSingleObject(local->semQueue, INFINITE);
		{
			if (!local->queue.empty())
			{
				order = local->queue.front();
				local->queue.pop();
			}
		}
		ReleaseSemaphore(local->semQueue, 1, NULL);

		if (order != nullptr)
		{
			RtlCopyMemory(&local->hidPacket[8], order->buff, 4);
			*(WORD*)&local->hidPacket[3] = local->CalculateCRC(&local->hidPacket[5]);
			for (char i = 0; i < 2; i++)
			{
				WaitForSingleObject(local->semDriver, INFINITE);
				{
					if (local->hDriver != nullptr)
					{
						if (!HidD_SetFeature(local->hDriver, local->hidPacket, 0x81))
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
				if (!local->OpenDriver())
				{
					break;
				}
			}
			delete[] order->buff;
			delete order;
		}
	}
}

WORD CNXTWrite::CalculateCRC(UCHAR* block)
{
	__int16 result = -1; // 0xffff;

	for (char i = 0; i < 6; i++)
	{
		UCHAR v5 = block[i];
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
