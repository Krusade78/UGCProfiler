#pragma once
#include "../Profile/CProfile.h"
#include "../EventQueue/CEventQueue.h"
#include "../InputQueue/CHIDQueue.h"
#include "CHIDDevices.h"
#include "../ProcessInput/ProcessUSBs_Calibration.h"
#include "../ProcessInput/CProcessInput.h"

class CPreprocess
{
public:
	CPreprocess(CProfile* pProfile, CEventQueue* evQueue, void* ptrHIDInput, void (*FnLockDevices)(void*), void (*FnUnlockDevices)(void*), CHIDDevices* (*FnGetDevice)(void*, UINT32));
	~CPreprocess();

	bool Init();
	void AddToQueue(UCHAR* buff, DWORD size) { hidQueue->Add(buff, size); };

private:
	CProfile* pProfile = nullptr;
	CHIDQueue* hidQueue = nullptr;

	short threadClosed = TRUE;
	bool exit = false;
	CCalibration calibration;

	CProcessInput* processInput = nullptr;

	//callbacks
	void* pHIDInput = nullptr;
	void (*LockDevices)(void*) = nullptr;
	void (*UnlockDevices)(void*) = nullptr;
	CHIDDevices* (*GetDevice)(void*, UINT32) = nullptr;

	static DWORD WINAPI ThreadRead(LPVOID param);

	void ConvertToCommon(UCHAR* data, DWORD size, UINT32 joyId);

	//UCHAR Switch4To8(UCHAR in);
};

