#pragma once
#include <deque>
#include "CEventPacket.h"

class CEventQueue
{
public:
	CEventQueue();
	~CEventQueue();

	static CEventQueue* Get() { return pNotifications; }

	void Clear();
	void Add(CEventPacket* event);
	CEventPacket* Read();
	HANDLE GetEvQueue() const { return evQueue; }
private:
	static CEventQueue* pNotifications;

	HANDLE mutexQueue = nullptr;
	HANDLE evQueue = nullptr;
	HANDLE evRead = nullptr;
	std::deque<CEventPacket*> queue;

	short priority = 0;
};

