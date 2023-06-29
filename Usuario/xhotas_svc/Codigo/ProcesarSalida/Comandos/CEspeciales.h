#pragma once
#include <deque>
#include "../../Perfil/CPerfil.h"
#include "../../ColaEventos/CPaqueteEventos.h"

class CEspeciales
{
public:
	static bool Procesar(CPerfil* pPerfil, std::deque<CPaqueteEvento*>::iterator* posEvento, void* padre);
private:
	static void CALLBACK EvtDelay(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
	static bool EstaHold(CPerfil* pPerfil, PEV_COMANDO comando);
	static void BorrarBloqueRepeat(CPaqueteEvento* cola, UCHAR tipoComando);
	static void CopiarColaConRepeticion(CPaqueteEvento* cola, UCHAR tipoComando);
};



