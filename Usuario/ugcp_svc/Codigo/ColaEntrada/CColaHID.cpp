#include "../framework.h"
#include "CColaHID.h"

CColaHID::CColaHID()
{
	mutexColaW = CreateMutex(NULL, false, NULL);
	mutexColaL = CreateMutex(NULL, false, NULL);
	semCola = CreateSemaphore(NULL, 100, 100, NULL);
	evCola = CreateEvent(NULL, TRUE, FALSE, NULL);
}

CColaHID::~CColaHID()
{
	HANDLE old1 = mutexColaW;
	InterlockedExchangePointer(&mutexColaW, nullptr);
	WaitForSingleObject(old1, INFINITE);
	WaitForSingleObject(mutexColaL, INFINITE);
	HANDLE old2 = mutexColaL;
	mutexColaL = nullptr;
	while (!cola.empty())
	{
		delete cola.front();
		cola.pop_front();
	}
	ReleaseMutex(old1);
	ReleaseMutex(old2);
	CloseHandle(old1);
	CloseHandle(old2);
	WaitForSingleObject(semCola, INFINITE);
	old1 = semCola;
	semCola = nullptr;
	CloseHandle(old1);
	old1 = evCola;
	evCola = nullptr;
	SetEvent(old1);
	CloseHandle(old1);
}

bool CColaHID::A�adir(UCHAR* buff, DWORD tam)
{
	if (InterlockedCompareExchangePointer(&mutexColaW, NULL, NULL) == NULL)
	{
		return false;
	}

	CPaqueteHID* paquete = new CPaqueteHID(buff, tam);
	if (WAIT_OBJECT_0 != WaitForSingleObject(semCola, 4000))
	{
		delete paquete;
		return false;
	}
	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexColaW, 5000))
	{
		ReleaseSemaphore(semCola, 1, NULL);
		delete paquete;
		return false;
	}
	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexColaL, 5000))
	{
		ReleaseSemaphore(semCola, 1, NULL);
		ReleaseMutex(mutexColaW);
		delete paquete;
		return false;
	}
	cola.push_back(paquete);
	SetEvent(evCola);
	ReleaseMutex(mutexColaL);
	ReleaseMutex(mutexColaW);

	return true;
}

CPaqueteHID* CColaHID::Leer()
{
	CPaqueteHID* paq = nullptr;
	if (InterlockedCompareExchangePointer(&mutexColaW, NULL, NULL) == NULL)
	{
		Sleep(500);
		return paq;
	}

	if (WAIT_OBJECT_0 != WaitForSingleObject(evCola, INFINITE))
	{
		return paq;
	}
	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexColaL, INFINITE))
	{
		return paq;
	}
	paq = cola.front();
	cola.pop_front();
	ReleaseSemaphore(semCola, 1, nullptr);
	if (cola.empty())
	{
		ResetEvent(evCola);
	}
	ReleaseMutex(mutexColaL);

	return paq;
}
