#pragma once
#include <deque>
#include "../Profile/CProfile.h"
#include "CVirtualHID.h"
#include "../EventQueue/CEventPacket.h"


class CProcessOutput
{
public:
	CProcessOutput(CProfile* pPerfil, CVirtualHID* pVhid);
	~CProcessOutput();
	void ClearEvents();

	static CProcessOutput* Get() {	return pNotifications; }

	void Process(CEventPacket* packet);
	HANDLE GetEvQueue() const { return hEvEmptyQueue_OnlyHolds; }

	std::deque<CEventPacket*>* GetEventQueue() { return &eventsQueue; }

	typedef struct
	{
		PTP_TIMER Timer;
		CProcessOutput* Parent;
		CEventPacket* Queue;
	} TIMER_CTX;
	std::deque<TIMER_CTX*>* GetTimersDelayList() { return &timersDelayList; }
	void ProcessDelay(TIMER_CTX* ctx);
private:
	static CProcessOutput* pNotifications;

	CProfile* pProfile = nullptr;
	CVirtualHID* pVhid = nullptr;
	HANDLE hWaitLockEvents = nullptr;
	std::deque<CEventPacket*> eventsQueue;
	HANDLE hEvEmptyQueue_OnlyHolds = nullptr;
	PTP_TIMER hMouseTimer = nullptr;
	std::deque<TIMER_CTX*> timersDelayList;

	void ProcessRequest();
	static void CALLBACK EvtMouseTick(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
};

