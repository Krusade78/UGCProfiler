#pragma once
#include <deque>
#include "CHIDPacket.h"

class CHIDQueue
{
public:
	CHIDQueue();
	~CHIDQueue();
	bool Add(UCHAR* buff, DWORD size);
	CHIDPacket* Read();

private:
	HANDLE mutexQueueW = nullptr;
	HANDLE mutexQueueL = nullptr;
	HANDLE semQueue = nullptr;
	HANDLE evQueue = nullptr;
	std::deque<CHIDPacket*> queue;
};

