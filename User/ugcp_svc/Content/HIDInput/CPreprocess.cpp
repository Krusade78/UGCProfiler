#include "../framework.h"
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

void CPreprocess::ConvertToCommon(UCHAR* data, DWORD size, UINT32 joyId)
{
	HID_INPUT_DATA hidData{};
	DWORD idx = 0;
	UCHAR idxAxis = 0;
	UCHAR idxButton = 0;
	UCHAR* report = &data[1];

	LockDevices(pHIDInput);
	CHIDDevices* pDev = GetDevice(pHIDInput, joyId);
	if (pDev != nullptr)
	{
		for (auto const& mapIndex : *pDev->GetMap())
		{
			if (mapIndex->ReportId != data[0]) { UnlockDevices(pHIDInput); return; }
			if (mapIndex->Bits > 0)
			{
				UCHAR shift = idx % 8;
				if (mapIndex->IsButton)
				{
					UCHAR v[17]{};
					RtlCopyMemory(&v, &report[idx / 8], ((idx + mapIndex->Bits - 1) / 8) + 1 - (idx / 8));
					v[(mapIndex->Bits + shift) / 8] &= (1 << ((mapIndex->Bits + shift) % 8)) - 1;
					UINT64* pv64 = reinterpret_cast<UINT64*>(v);
					if (shift != 0)
					{
						*pv64 = (*pv64 >> shift) | *reinterpret_cast<UINT64*>(&v[8]) << (64 - shift);
						pv64 = reinterpret_cast<UINT64*>(&v[8]);
						*pv64 = (*pv64 >> shift) | static_cast<UINT64>(v[16]) << (64 - shift);
						pv64 = reinterpret_cast<UINT64*>(v);
					}
					UCHAR shiftBt = idxButton % 64;
					if (idxButton > 63)
					{
						hidData.Buttons[1] |= *pv64 << shiftBt;
					}
					else
					{
						hidData.Buttons[0] |= *pv64 << shiftBt;
						if (shiftBt != 0) { hidData.Buttons[1] |= *pv64 >> (64 - shiftBt); }
						hidData.Buttons[1] |= *(UINT64*)&v[8] << shiftBt;
					}
					idxButton += mapIndex->Bits;
				}
				else
				{
					UINT32 v = 0;
					RtlCopyMemory(&v, &report[idx / 8], ((idx + mapIndex->Bits - 1) / 8) + 1 - (idx / 8));
					v = (v >> shift) & ((1 << mapIndex->Bits) - 1);
					if (!mapIndex->Skip)
					{
						hidData.Axis[idxAxis++] = static_cast<UINT16>(v);
					}
				}
				idx += mapIndex->Bits;
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
