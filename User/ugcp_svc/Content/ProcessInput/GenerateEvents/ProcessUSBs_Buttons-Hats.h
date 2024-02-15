#pragma once
#include "../../Profile/CProfile.h"

class CBotonesSetas
{
public:
	static void PressButton(CProfile* pProfile, UINT32 joyId, UCHAR idx);
	static void ReleaseButton(CProfile* pProfile, UINT32 joyId, UCHAR idx);

	//static void PulsarSeta(CProfile* pPerfil, TipoJoy tipo, UCHAR idx);
	//static void SoltarSeta(CProfile* pPerfil, TipoJoy tipo, UCHAR idx);
};


