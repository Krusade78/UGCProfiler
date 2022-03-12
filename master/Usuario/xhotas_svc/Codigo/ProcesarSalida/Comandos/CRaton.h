#pragma once
#include "../../ColaEventos/CPaqueteEventos.h"
#include "../CVirtualHID.h"

class CRaton
{
public:
	static bool Procesar(CVirtualHID* pVHid, PEV_COMANDO comando, bool* setTimer);
private:
	static bool EnviarSalida(CVirtualHID* pVHid, TipoComando cmd, bool ejeX, bool ejeY);
};

