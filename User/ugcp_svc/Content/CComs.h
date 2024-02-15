#pragma once
#include "Profile/CProfile.h"

class CComs
{
public:
	CComs(CProfile* pProfile);
	~CComs();

	bool Init();
	void SetHwnd(HWND hWnd) { hWndMessages = hWnd; };
private:
	enum class MsjType : BYTE { RawMode, CalibrationMode, Calibration, Antiv, Map, Commands};

	CProfile* pProfile = nullptr;
	HANDLE hPipe = nullptr;
	HWND hWndMessages = nullptr;
	bool exit = false;
	short threadClosed = TRUE;

	static DWORD WINAPI ThreadRead(LPVOID param);
	bool ProcessMessage(BYTE* msg, DWORD size);
};

