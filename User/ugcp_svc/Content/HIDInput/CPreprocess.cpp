#include "../framework.h"
#include <hidsdi.h>
#include "CPreprocess.h"
#include "../ProcessInput/GenerateEvents/CGenerateEvents.h" //init
#include "Hid_Input_Data.h"

CPreprocess::CPreprocess(CProfile* pProfile, CEventQueue* pEvQueue, void* ptrHIDInput, void (*FnLockDevices)(void*), void (*FnUnlockDevices)(void*), CHIDDevices* (*FnGetDevice)(void*, UINT32))
{
	this->pProfile = pProfile;
	this->hidQueue = new CHIDQueue();
	CGenerateEvents::Init(pProfile, pEvQueue);

	processInput = new CProcessInput(pProfile);
	pHIDInput = ptrHIDInput;
	LockDevices = FnLockDevices;
	UnlockDevices = FnUnlockDevices;
	GetDevice = FnGetDevice;
}

CPreprocess::~CPreprocess()
{
	exit = true;
	delete hidQueue;
	while (InterlockedCompareExchange16(&threadClosed, 0, 0) == FALSE) Sleep(1000);
	delete processInput;
}

bool CPreprocess::Init()
{
	if (NULL != CreateThread(NULL, 0, ThreadRead, this, 0, NULL))
	{
		while (InterlockedCompareExchange16(&threadClosed, FALSE, FALSE))
		{
			Sleep(500);
		}
		return true;
	}

	return false;
}

DWORD WINAPI CPreprocess::ThreadRead(LPVOID param)
{
	CPreprocess* local = (CPreprocess*)param;
	InterlockedExchange16(&local->threadClosed, FALSE);
	SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

	while (!local->exit)
	{
		CHIDPacket* paq = local->hidQueue->Read();
		if (paq != nullptr)
		{
			local->ConvertToCommon(paq->GetData(), paq->GetSize(), paq->GetJoyId());
			delete paq;
		}
	}

	InterlockedExchange16(&local->threadClosed, TRUE);
	return 0;
}

void CPreprocess::ConvertToCommon(UCHAR* buffer, DWORD size, UINT32 joyId)
{
	HID_INPUT_DATA hidData{};
	PHIDP_DATA data = reinterpret_cast<PHIDP_DATA>(buffer);

	LockDevices(pHIDInput);
	CHIDDevices* pDev = GetDevice(pHIDInput, joyId);
	if (pDev != nullptr)
	{
		size /= sizeof(HIDP_DATA);
		for (DWORD idxData = 0; idxData < size; idxData++)
		{
			UCHAR idxButton = 0;
			UCHAR idxAxis = 0;
			UCHAR idxHat = 0;
			for (auto const& mapIndex : *pDev->GetMap())
			{
				if (mapIndex->IsButton)
				{
					if ((data[idxData].DataIndex >= mapIndex->Index) && (data[idxData].DataIndex < (mapIndex->Index + mapIndex->Bits)))
					{
						UCHAR idx = data[idxData].DataIndex - mapIndex->Index + idxButton;
						if (idx > 63)
						{
							hidData.Buttons[1] |= 1ull << (idx - 64);
						}
						else
						{
							hidData.Buttons[0] |= 1ull << idx;
						}
					}
					idxButton += mapIndex->Bits;
				}
				else if (mapIndex->IsHat)
				{
					if (data[idxData].DataIndex == mapIndex->Index)
					{
						hidData.Hats[idxHat] = (static_cast<UCHAR>(data[idxData].RawValue) < (mapIndex->IsHat & 0xf)) || (static_cast<UCHAR>(data[idxData].RawValue) > (mapIndex->IsHat >> 4)) ? 255 : static_cast<UCHAR>(data[idxData].RawValue) - (mapIndex->IsHat & 0xf);
					}
					idxHat++;
				}
				else
				{
					if (data[idxData].DataIndex == mapIndex->Index)
					{
						hidData.Axis[idxAxis] = static_cast<UINT16>(data[idxData].RawValue);
					}
					idxAxis++;;
				}
			}
		}
	}
	UnlockDevices(pHIDInput);

	// Calibrate
	if (!pProfile->GetCalibrationMode())
	{
		calibration.Calibrate(pProfile, joyId, &hidData);
	}

	processInput->Process(joyId, &hidData);
}

//UCHAR CPreprocess::Switch4To8(UCHAR in)
//{
//	switch (in)
//	{
//		case 0: return 0;
//		case 1: return 1;
//		case 2: return 3;
//		case 3: return 2;
//		case 4: return 5;
//		case 6: return 4;
//		case 8: return 7;
//		case 9: return 8;
//		case 12: return 6;
//		default: return 0;
//	}
//}
