#include "../framework.h"
#include "CEventQueue.h"

CEventQueue* CEventQueue::pNotifications = nullptr;

CEventQueue::CEventQueue()
{
	mutexQueue = CreateSemaphore(NULL, 1, 1, NULL);
	evQueue = CreateEvent(NULL, TRUE, FALSE, NULL);
	evRead = CreateEvent(NULL, FALSE, TRUE, NULL);
}

CEventQueue::~CEventQueue()
{
	pNotifications = nullptr;
	while (!queue.empty())
	{
		delete queue.front();
		queue.pop_front();
	}
	CloseHandle(mutexQueue);
	CloseHandle(evQueue);
	CloseHandle(evRead);
}

void CEventQueue::Clear()
{
	WaitForSingleObject(mutexQueue, INFINITE);
	while (!queue.empty())
	{
		delete queue.front();
		queue.pop_front();

	}
	CloseHandle(mutexQueue);
}

void CEventQueue::Add(CEventPacket* event)
{
	if (InterlockedCompareExchange16(&priority, 0, 0) == 1)
	{
		WaitForSingleObject(evRead, INFINITE);
	}
	WaitForSingleObject(mutexQueue, INFINITE);
	queue.push_back(event);
	SetEvent(evQueue);
	ReleaseSemaphore(mutexQueue, 1, NULL);
}

CEventPacket* CEventQueue::Read()
{
	CEventPacket* paq = nullptr;
	bool wait = false;
	
	InterlockedIncrement16(&priority);

	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexQueue, INFINITE))
	{
		return paq;
	}

	paq = queue.front();
	queue.pop_front();
	if (queue.empty())
	{
		ResetEvent(evQueue);
		InterlockedDecrement16(&priority);
		SetEvent(evRead);
	}
	ReleaseSemaphore(mutexQueue, 1, NULL);

	return paq;
}

