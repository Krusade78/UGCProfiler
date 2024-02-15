#pragma once
#include "../Profile/CProfile.h"
#include "../EventQueue/CEventQueue.h"
#include "CPreprocess.h"
#include "CHIDDevices.h"

class CHIDInput
{
public:
	CHIDInput(CProfile* profile, CEventQueue* evQueue);
	~CHIDInput();

	bool Init(HINSTANCE hInst);
	HWND GetHwnd() const { return pnpHWnd; }
	void LoopWnd();

	//callbacks
	inline static void CallbackRefreshDevices(void* ptrThis, UINT32* ids, UCHAR size) { static_cast<CHIDInput*>(ptrThis)->RefreshDevices(ids, size); }
	inline static void CallbackPauseWinUSB(void* ptrThis, bool onoff) { static_cast<CHIDInput*>(ptrThis)->PauseWinUSB(onoff); }

	inline static void LockDevices(void* ptrThis) { WaitForSingleObject(static_cast<CHIDInput*>(ptrThis)->mutexDevices, INFINITE); }
	inline static CHIDDevices* GetDevice(void* ptrThis, UINT32 joyId)
	{
		std::unordered_map<UINT32, CHIDDevices*>::const_iterator idev = static_cast<CHIDInput*>(ptrThis)->hidDevices.find(joyId);
		return (idev == static_cast<CHIDInput*>(ptrThis)->hidDevices.end()) ? nullptr : idev->second;
	}
	inline static void UnlockDevices(void* ptrThis) { ReleaseSemaphore(static_cast<CHIDInput*>(ptrThis)->mutexDevices, 1, NULL); }

private:
	CPreprocess* processHID = nullptr;

	HWND pnpHWnd = NULL;
	HDEVNOTIFY pnpHdn = NULL;
	CProfile* pProfile = nullptr;

	std::unordered_map<UINT32, short> threadClosed;
	std::unordered_map<UINT32, bool> exit;

	HANDLE mutexDevices = nullptr;
	std::unordered_map<UINT32, CHIDDevices*> hidDevices;

	void RefreshDevices(UINT32* ids, UCHAR size);
	void PauseWinUSB(bool onoff);
	bool PnpNotification(HINSTANCE hInst);
	static LRESULT CALLBACK PnpMsjProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

	static DWORD WINAPI ThreadRead(LPVOID param);

	typedef struct
	{
		CHIDInput* Parent;
		UINT32 joyId;
	} ST_THREAD_PARAMS;
};
