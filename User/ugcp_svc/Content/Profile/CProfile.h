#pragma once
#include <deque>
#include <unordered_map>
#include "../EventQueue/CEventPacket.h"
#include "CProfile.Status.h"
#include "CProfile.Programming.h"
#include "CProfile.Calibration.h"

class CProfile
{
public:
	CProfile(void* pVHID);
	~CProfile();

	inline void SetCalibrationMode(bool mode) { InterlockedExchange16(&calibrationMode, mode); if (PauseWinUSB != nullptr) { PauseWinUSB(hidInput, mode); } };
	inline void SetRawMode(bool mode) { InterlockedExchange16(&rawMode, mode); if (PauseWinUSB != nullptr) { PauseWinUSB(hidInput, mode); } };
	void WriteCalibration(BYTE* data);
	void WriteAntivibration(BYTE* data);
	bool HF_IoWriteCommands(BYTE* data, DWORD size);
	bool HF_IoWriteMap(BYTE* data, DWORD size);

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
	void* pVHID = nullptr;
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
