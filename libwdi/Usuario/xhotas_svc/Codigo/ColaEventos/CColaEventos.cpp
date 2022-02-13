#include "../framework.h"
#include "CColaEventos.h"

CColaEventos* CColaEventos::pNotificaciones = nullptr;

CColaEventos::CColaEventos()
{
	mutexCola = CreateSemaphore(NULL, 1, 1, NULL);
	evCola = CreateEvent(NULL, TRUE, FALSE, NULL);
	evLeido = CreateEvent(NULL, FALSE, TRUE, NULL);
}

CColaEventos::~CColaEventos()
{
	pNotificaciones = nullptr;
	while (!cola.empty())
	{
		delete cola.front();
		cola.pop_front();
	}
	CloseHandle(mutexCola);
	CloseHandle(evCola);
	CloseHandle(evLeido);
}

void CColaEventos::Vaciar()
{
	WaitForSingleObject(mutexCola, INFINITE);
	while (!cola.empty())
	{
		delete cola.front();
		cola.pop_front();

	}
	CloseHandle(mutexCola);
}

void CColaEventos::Añadir(CPaqueteEvento* evento)
{
	if (InterlockedCompareExchange16(&prioridad, 0, 0) == 1)
	{
		WaitForSingleObject(evLeido, INFINITE);
	}
	WaitForSingleObject(mutexCola, INFINITE);
	cola.push_back(evento);
	SetEvent(evCola);
	ReleaseSemaphore(mutexCola, 1, NULL);
}

CPaqueteEvento* CColaEventos::Leer()
{
	CPaqueteEvento* paq = nullptr;
	bool espera = false;
	
	InterlockedIncrement16(&prioridad);

	if (WAIT_OBJECT_0 != WaitForSingleObject(mutexCola, INFINITE))
	{
		return paq;
	}

	paq = cola.front();
	cola.pop_front();
	if (cola.empty())
	{
		ResetEvent(evCola);
		InterlockedDecrement16(&prioridad);
		SetEvent(evLeido);
	}
	ReleaseSemaphore(mutexCola, 1, NULL);

	return paq;
}

