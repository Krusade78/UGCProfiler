#pragma once
#include "../../Perfil/CPerfil.h"

class CBotonesSetas
{
public:
	static void PulsarBoton(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx);
	static void SoltarBoton(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx);

	static void PulsarSeta(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx);
	static void SoltarSeta(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx);
};


