#pragma once
#include <deque>
#include "../Perfil/CPerfil.h"
#include "CVirtualHID.h"
#include "../ColaEventos/CPaqueteEventos.h"


class CProcesarSalida
{
public:
	CProcesarSalida(CPerfil* pPerfil, CVirtualHID* pVhid);
	~CProcesarSalida();
	void LimpiarEventos();

	void Procesar(CPaqueteEvento* paquete);
	HANDLE GetEvCola() { return hEvColaVacia_SoloHolds; }

	std::deque<CPaqueteEvento*>* GetColaEventos() { return &colaEventos; }

	typedef struct
	{
		PTP_TIMER Timer;
		CProcesarSalida* Padre;
		CPaqueteEvento* Cola;
	} TIMER_CTX;
	std::deque<TIMER_CTX*>* GetListaTimersDelay() { return &listaTimersDelay; }
	void ProcesarDelay(TIMER_CTX* ctx);
private:
	CPerfil* pPerfil = nullptr;
	CVirtualHID* pVhid = nullptr;
	HANDLE hWaitLockEventos = nullptr;
	std::deque<CPaqueteEvento*> colaEventos;
	HANDLE hEvColaVacia_SoloHolds = nullptr;
	PTP_TIMER hRatonTimer = nullptr;
	std::deque<TIMER_CTX*> listaTimersDelay;

	void ProcesarRequest();
	static void CALLBACK EvtTickRaton(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
};

