#pragma once
#include "../../Profile/CProfile.h"

class CButtonsHats
{
public:
	CButtonsHats(CProfile* pProfile);
	~CButtonsHats();
public:
	static void PressButton(CProfile* pProfile, UINT32 joyId, UCHAR idx);
	static void ReleaseButton(CProfile* pProfile, UINT32 joyId, UCHAR idx);

	static void PressHat(CProfile* pProfile, UINT32 joyId, UCHAR idx);
	static void ReleaseHat(CProfile* pProfile, UINT32 joyId, UCHAR idx);
private:
	typedef struct
	{
		PTP_TIMER Timer;
		CButtonsHats* Parent;
		UINT32 JoyId;
		UINT16 HatButton_Idx; // 8bit hat or button + 8 bit pos;
	} TIMER_CTX;

	std::unordered_map<UINT64, PTP_TIMER>* pStButtonsMap = nullptr;
	std::unordered_map<UINT64, PTP_TIMER>* pStHatsMap = nullptr;
	CProfile* pProfile = nullptr;
	HANDLE hMutexStatus = nullptr;

	static CButtonsHats* mainProcess;
	static void CreateInstance(CProfile* pProfile);

	void PressButton(UINT32 joyId, UCHAR idx, bool longPress);
	void ReleaseButton(UINT32 joyId, UCHAR idx, bool shortRelease);

	void PressHat(UINT32 joyId, UCHAR idx, bool longPress);
	void ReleaseHat(UINT32 joyId, UCHAR idx, bool shortRelease);

	static void CALLBACK EvtLongPress(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
	static void CALLBACK EvtShortRelease(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
};


