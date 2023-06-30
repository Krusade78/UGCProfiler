#pragma once
#include "../../ColaEventos/CPaqueteEventos.h"
#include "../CVirtualHID.h"

class CTeclado
{
public:
	static void Procesar(PEV_COMANDO pComando, CVirtualHID* pVHid);
private:
	static UINT GetExtendida(UCHAR tecla);
};

