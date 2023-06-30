#pragma once
#include "../../ColaEventos/CPaqueteEventos.h"
#include "../CVirtualHID.h"

class CDirectX
{
public:
	static void Posicion(PEV_COMANDO pComando, CVirtualHID* pVHid);
	static void Botones_Setas(PEV_COMANDO comando, CVirtualHID* pVHid);
private:

};

