#include "../framework.h"
#include "CHIDQueue.h"

CHIDQueue::CHIDQueue()
{
	mutexQueueW = CreateMutex(NULL, false, NULL);
	mutexQueueL = CreateMutex(NULL, false, NULL);
	semQueue = CreateSemaphore(NULL, 100, 100, NULL);
	evQueue = CreateEvent(NULL, TRUE, FALSE, NULL);
}

CHIDQueue::~CHIDQueue()
{
	HANDLE old1 = mutexQueueW;
	InterlockedExchangePointer(&mutexQueueW, nullptr);
	WaitForSingleObject(old1, INFINITE);
	WaitForSingleObject(mutexQueueL, INFINITE);
	HANDLE old2 = mutexQueueL;
	mutexQueueL = nullptr;
	while (!queue.empty())
	{
		delete queue.front();
		queue.pop_front();
	}
	ReleaseMutex(old1);
	ReleaseMutex(old2);
	CloseHandle(old1);
	CloseHandle(old2);
	WaitForSingleObject(semQueue, INFINITE);
	old1 = semQueue;
	semQueue = nullptr;
	CloseHandle(old1);
	old1 = evQueue;
	evQueue = nullptr;
	SetEvent(old1);
	CloseHandle(old1);
}

bool CHIDQueue::Add(UCHAR* buff, DWORD size)
{
	if (InterlockedCompareExchangePointer(&mutexQueueW, NULL, NULL) == NULL)
	{
		return false;
	}

	CHIDPacket* packet = new CHIDPacket(buff, size);
	if (WAIT_OBJECT_0 != WaitForSingleObject(semQueue, 4000))
	{
		delete packet;
		return false;
	}
	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexQueueW, 5000))
	{
		ReleaseSemaphore(semQueue, 1, NULL);
		delete packet;
		return false;
	}
	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexQueueL, 5000))
	{
		ReleaseSemaphore(semQueue, 1, NULL);
		ReleaseMutex(mutexQueueW);
		delete packet;
		return false;
	}
	queue.push_back(packet);
	SetEvent(evQueue);
	ReleaseMutex(mutexQueueL);
	ReleaseMutex(mutexQueueW);

	return true;
}

CHIDPacket* CHIDQueue::Read()
{
	CHIDPacket* packet = nullptr;
	if (InterlockedCompareExchangePointer(&mutexQueueW, NULL, NULL) == NULL)
	{
		Sleep(500);
		return packet;
	}

	if (WAIT_OBJECT_0 != WaitForSingleObject(evQueue, INFINITE))
	{
		return packet;
	}
	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexQueueL, INFINITE))
	{
		return packet;
	}
	packet = queue.front();
	queue.pop_front();
	ReleaseSemaphore(semQueue, 1, nullptr);
	if (queue.empty())
	{
		ResetEvent(evQueue);
	}
	ReleaseMutex(mutexQueueL);

	return packet;
}
