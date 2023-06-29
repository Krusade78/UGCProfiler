#include "../framework.h"
#include "CPaqueteEventos.h"

CPaqueteEvento::CPaqueteEvento()
{
	cola = new std::deque<PEV_COMANDO>();
}

CPaqueteEvento::~CPaqueteEvento()
{
	if (cola != nullptr)
	{
		while (!cola->empty())
		{
			delete cola->front();
			cola->pop_front();
		}
	}
	delete cola; cola = nullptr;
}

void CPaqueteEvento::AñadirComando(PEV_COMANDO comando)
{
	cola->push_back(comando);
}
std::deque<PEV_COMANDO>* CPaqueteEvento::GetColaComandos()
{
	return cola;
}

