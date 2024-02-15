#pragma once
#include <deque>
#include "../../Profile/CProfile.h"
#include "../../EventQueue/CEventPacket.h"

class COther
{
public:
	static bool Process(CProfile* pProfile, std::deque<CEventPacket*>::iterator* posEvent, void* parent);
private:
	static void CALLBACK EvtDelay(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
	static bool IsHoldOn(CProfile* pProfile, PEV_COMMAND command);
	static void DeleteRepeatBlock(CEventPacket* queue, UCHAR commandType);
	static void CopyQueueWithRepeat(CEventPacket* queue, UCHAR commandType);
};



