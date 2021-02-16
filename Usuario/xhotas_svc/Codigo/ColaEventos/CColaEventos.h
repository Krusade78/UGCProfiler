#pragma once
#include <deque>
#include "CPaqueteEventos.h"

class CColaEventos
{
public:
	CColaEventos();
	~CColaEventos();

	static CColaEventos* Get() { return pNotificaciones; }

	void Vaciar();
	void A�adir(CPaqueteEvento* evento);
	CPaqueteEvento* Leer();
	HANDLE GetEvCola() { return evCola; }
private:
	static CColaEventos* pNotificaciones;

	HANDLE mutexCola = nullptr;
	HANDLE evCola = nullptr;
	short tamCola = 0;
	std::deque<CPaqueteEvento*> cola;
};

