#pragma once
#include <deque>
#include <unordered_map>
#include <span>
#include "../EventQueue/CEventPacket.h"
#include "CProfile.Status.h"
#include "CProfile.Programming.h"
#include "CProfile.Calibration.h"

class CProfile
{
public:
	CProfile();
	~CProfile();

	inline void SetCalibrationMode(bool mode) { InterlockedExchange16(&calibrationMode, mode); if (PauseWinUSB != nullptr) { PauseWinUSB(hidInput, mode); } };
	inline void SetRawMode(bool mode) { InterlockedExchange16(&rawMode, mode); if (PauseWinUSB != nullptr) { PauseWinUSB(hidInput, mode); } };
	void WriteCalibration(std::span<const std::uint8_t> msg);
	void WriteAntivibration(std::span<const std::uint8_t> msg);
	bool HF_IoWriteCommands(std::span<const std::uint8_t> msg);
	bool HF_IoWriteMap(std::span<const std::uint8_t> msg);

	inline bool GetCalibrationMode() { return InterlockedCompareExchange16(&calibrationMode, FALSE, FALSE) == TRUE; };
	inline bool GetRawMode() { return InterlockedCompareExchange16(&rawMode, FALSE, FALSE) == TRUE; };

	inline void InitCalibrationRead() const { WaitForSingleObject(hMutexCalibration, INFINITE); }
	inline CALIBRATION* GetCalibration() { return &calibration; }
	inline void EndCalibrationRead() const { ReleaseMutex(hMutexCalibration); }

	inline void BeginProfileRead() const { WaitForSingleObject(hMutexProgram, INFINITE); }
	inline PROGRAMMING* GetProfile() { return &profile; };
	inline void EndProfileRead() const { ReleaseMutex(hMutexProgram); }

	inline void LockStatus() const { WaitForSingleObject(hMutexStatus, INFINITE); }
	inline STATUS* GetStatus() { return &status; }
	inline void UnlockStatus() const { ReleaseMutex(hMutexStatus); }

	inline void SetRefreshDevicesCallback(void* hidInput, void (*FnRefreshHidInputDevice)(void*, UINT32*, UCHAR)) { this->hidInput = hidInput, this->RefreshHidInputDevice = FnRefreshHidInputDevice; }
	inline void SetPauseWinUSBCallback(void (*FnPauseWinUSB)(void*, bool)) { this->PauseWinUSB = FnPauseWinUSB; }
private:
	HANDLE hMutexProgram = nullptr;
	HANDLE hMutexCalibration = nullptr;
	HANDLE hMutexStatus = nullptr;
	short rawMode = FALSE;
	short calibrationMode = FALSE;
	PROGRAMMING profile;
	CALIBRATION calibration;
	STATUS status;
	bool newProfileOut = false;
	bool newProfileIn = false;
	bool resetComanmds = false;

	void* hidInput = nullptr;
	void (*RefreshHidInputDevice)(void*, UINT32*, UCHAR) = nullptr;
	void (*PauseWinUSB)(void*, bool) = nullptr;

	void ClearProfile();
};
