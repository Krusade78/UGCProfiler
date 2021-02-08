#pragma once
#include <deque>
#include "CPaqueteHID.h"

class CColaHID
{
public:
	CColaHID();
	~CColaHID();
	bool Añadir(UCHAR* buff, DWORD tam);
	CPaqueteHID* Leer();

private:
	HANDLE mutexColaW = nullptr;
	HANDLE mutexColaL = nullptr;
	HANDLE semCola = nullptr;
	HANDLE evCola = nullptr;
	std::deque<CPaqueteHID*> cola;
};

