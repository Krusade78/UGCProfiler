#include "../framework.h"
#include "USBX52Write.h"
#include <winusb.h>


CX52Write* CX52Write::pLocal = nullptr;

CX52Write::CX52Write()
{
	pLocal = this;
	semQueue = CreateSemaphore(NULL, 1, 1, NULL);
	evQueue = CreateEvent(NULL, TRUE, FALSE, NULL);
	CreateThread(NULL, 0, WkSend, this, 0, NULL);
}

CX52Write::~CX52Write()
{
	pLocal = nullptr;
	exit = true;
	WaitForSingleObject(semQueue, INFINITE);
	{
		while (!queue.empty())
		{
			PORDER order = queue.front();
			delete order;
			queue.pop();
		}
	}
	ReleaseSemaphore(semQueue, 1, NULL);
	SetEvent(evQueue);
	while (exit) { Sleep(500); }
	CloseHandle(semQueue);
	CloseHandle(evQueue);
}

#pragma region "Orders"
void CX52Write::Light_MFD(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { *(SystemBuffer) ,0 , 0xb1};
	SendOrder(params, 1);
}

void CX52Write::Light_Global(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { *(SystemBuffer),0 ,0xb2 };
	SendOrder(params, 1);
}

void CX52Write::Light_Info(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { static_cast<UCHAR>(*(SystemBuffer) + 0x50),0 , 0xb4 };
	SendOrder(params, 1);
}

void CX52Write::Set_Pinkie(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { static_cast<UCHAR>(*(SystemBuffer) + 0x50),0 , 0xfd };
	SendOrder(params, 1);
}

void CX52Write::Set_Text(PUCHAR SystemBuffer, BYTE bufferSize)
{
	UCHAR params[3 * 17]{};
	UCHAR text[16];
	UCHAR nparams = 1;
	UCHAR paramIdx = 0;

	if ((bufferSize - 1) > 16)
		return;

	RtlZeroMemory(text, 16);
	RtlCopyMemory(text, &(SystemBuffer)[1], bufferSize - 1);


	params[0] = 0x0; params[1] = 0;
	switch (*(SystemBuffer)) //line
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
		if (text[i] == 0)
			break;
		params[0 + (3 * nparams)] = text[i];
		params[1 + (3 * nparams)] = text[i + 1];
		params[2 + (3 * nparams)] = paramIdx;
		nparams++;
	}

	SendOrder(params, nparams);
}

void CX52Write::Set_Hour(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { (SystemBuffer)[2] ,(SystemBuffer)[1] , static_cast<UCHAR>(*(SystemBuffer) + 0xbf) };
	SendOrder(params, 1);
}

void CX52Write::Set_Hour24(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { (SystemBuffer)[2] ,static_cast<UCHAR>((SystemBuffer)[1] + 0x80), static_cast<UCHAR>(*(SystemBuffer) + 0xbf) };
	SendOrder(params, 1);
}

void CX52Write::Set_Date(PUCHAR SystemBuffer)
{
	UCHAR params[3] = { 0, 0, 0 };

	switch (SystemBuffer[0])
	{
	case 1:
		params[2] = 0xc4;
		params[1] = (UCHAR)(Date >> 8);
		params[0] = SystemBuffer[1];
		Date = *((USHORT*)params);
		break;
	case 2:
		params[2] = 0xc4;
		params[1] = SystemBuffer[1];
		params[0] = (UCHAR)(Date & 0xff);
		Date = *((USHORT*)params);
		break;
	case 3:
		params[2] = 0xc8;
		params[1] = 0;
		params[0] = SystemBuffer[1];
	}
	SendOrder(params, 1);
}
#pragma endregion

void CX52Write::SendOrder(UCHAR* buffer, BYTE paquetes)
{
	for (BYTE processed = 0; processed < paquetes; processed++)
	{
		PORDER order = new ORDER;
		order->value = *((USHORT*)&buffer[processed * 3]);
		order->idx = buffer[2 + (processed * 3)];

		WaitForSingleObject(semQueue, INFINITE);
		{
			queue.push(order);
			SetEvent(evQueue);
		}
		ReleaseSemaphore(semQueue, 1, NULL);
	}
}

DWORD WINAPI  CX52Write::WkSend(LPVOID param)
{	
	CX52Write* local = static_cast<CX52Write*>(param);
	while (!local->exit)
	{
		PORDER order = nullptr;
		WaitForSingleObject(local->semQueue, INFINITE);
		{
			if (!local->queue.empty())
			{
				order = local->queue.front();
				local->queue.pop();
				ResetEvent(local->evQueue);
			}
		}
		ReleaseSemaphore(local->semQueue, 1, NULL);

		if (order != nullptr)
		{
			WINUSB_SETUP_PACKET controlSetupPacket
			{
				controlSetupPacket.RequestType = 0b01000000,
				controlSetupPacket.Request = 0x91, // Request
				controlSetupPacket.Value = order->value, // Value
				controlSetupPacket.Index = order->idx, // Index  
				controlSetupPacket.Length = 0
			};

			delete order;

			if (InterlockedCompareExchangePointer(&local->wUSB, nullptr, nullptr) != nullptr)
			{
				WinUsb_ControlTransfer(local->wUSB, controlSetupPacket, NULL, 0, NULL, NULL);
			}
		}
		else
		{
			WaitForSingleObject(local->evQueue, INFINITE);
		}
	}

	local->exit = false;
	return 0;
}