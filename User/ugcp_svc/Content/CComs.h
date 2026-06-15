#pragma once
#include "Profile/CProfile.h"
#include <span>

class CComs
{
public:
	explicit CComs(CProfile& pProfile);
	~CComs();

	bool Init();
	void SetHwnd(HWND hWnd) { hWndMessages = hWnd; };
private:
	enum class MsjType : std::uint8_t { RawMode, CalibrationMode, Calibration, Antiv, Map, Commands};

	CProfile& pProfile;
	unique_handle hPipe;
	HWND hWndMessages = nullptr;
	std::jthread thread;

	void ThreadRead(std::stop_token exit);
	bool ProcessMessage(std::span<const uint8_t> msg);
};

