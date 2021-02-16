#include "../framework.h"
#include "CColaEventos.h"

CColaEventos* CColaEventos::pNotificaciones = nullptr;

CColaEventos::CColaEventos()
{
	mutexCola = CreateMutex(NULL, false, NULL);
	evCola = CreateEvent(NULL, TRUE, FALSE, NULL);
}

CColaEventos::~CColaEventos()
{
	pNotificaciones = nullptr;
	HANDLE old = mutexCola;
	InterlockedExchangePointer(&mutexCola, nullptr);
	WaitForSingleObject(old, INFINITE);
	while (!cola.empty())
	{
		delete cola.front();
		cola.pop_front();
		InterlockedDecrement16(&tamCola);
	}
	CloseHandle(old);
	CloseHandle(evCola);
}

void CColaEventos::Vaciar()
{
	WaitForSingleObject(mutexCola, INFINITE);
	while (!cola.empty())
	{
		delete cola.front();
		cola.pop_front();
		InterlockedDecrement16(&tamCola);
	}
	CloseHandle(mutexCola);
}

void CColaEventos::Añadir(CPaqueteEvento* evento)
{
	bool espera = false;
	if (InterlockedCompareExchange16(&tamCola, 0, 0) < 2)
	{
		espera = true;
		WaitForSingleObject(mutexCola, INFINITE);
	}
	cola.push_back(evento);
	InterlockedIncrement16(&tamCola);
	SetEvent(evCola);
	if (espera)
	{
		ReleaseMutex(mutexCola);
	}
}

CPaqueteEvento* CColaEventos::Leer()
{
	CPaqueteEvento* paq = nullptr;
	bool espera = false;

	if (InterlockedCompareExchangePointer(&mutexCola, NULL, NULL) == NULL)
	{
		Sleep(500);
		return paq;
	}

	if (InterlockedCompareExchange16(&tamCola, 0, 0) < 2)
	{
		espera = true;
		if (WAIT_OBJECT_0 != WaitForSingleObject(mutexCola, INFINITE))
		{
			return paq;
		}
	}

	paq = cola.front();
	cola.pop_front();
	if (cola.empty())
	{
		ResetEvent(evCola);
	}
	InterlockedDecrement16(&tamCola);
	if (espera)
	{
		ReleaseMutex(mutexCola);
	}

	return paq;
}

