#pragma once
#include "../Perfil/CPerfil.h"
#include "../ColaEventos/CPaqueteEventos.h"
#include "../ProcesarSalida/CVirtualHID.h"

class CCalibrado
{
public:
	static void Calibrar(CPerfil* pPerfil, TipoJoy tipo, PVHID_INPUT_DATA datosHID);
};

