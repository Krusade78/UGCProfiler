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
	void Añadir(CPaqueteEvento* evento);
	CPaqueteEvento* Leer();
	HANDLE GetEvCola() { return evCola; }
private:
	static CColaEventos* pNotificaciones;

	HANDLE mutexCola = nullptr;
	HANDLE evCola = nullptr;
	HANDLE evLeido = nullptr;
	std::deque<CPaqueteEvento*> cola;

	short prioridad = 0;
};

