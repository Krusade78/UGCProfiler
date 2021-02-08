#pragma once
#include <deque>
#include "CPaqueteEventos.h"

class CColaEventos
{
public:
	CColaEventos();
	~CColaEventos();

	void Vaciar();
	void Añadir(CPaqueteEvento* evento);
	CPaqueteEvento* Leer();
	HANDLE GetEvCola() { return evCola; }
private:
	HANDLE mutexCola = nullptr;
	HANDLE evCola = nullptr;
	short tamCola = 0;
	std::deque<CPaqueteEvento*> cola;
};

