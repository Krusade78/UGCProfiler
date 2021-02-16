#include "../framework.h"
#include "CProcesarSalida.h"
#include "../ProcesarEntrada/GenerarEventos/CGenerarEventos.h"
#include "Comandos/CDirectX.h"
#include "Comandos/CTeclado.h"
#include "Comandos/CRaton.h"
#include "Comandos/CX52.h"
#include "Comandos/CEspeciales.h"

CProcesarSalida* CProcesarSalida::pNotificaciones = nullptr;

CProcesarSalida::CProcesarSalida(CPerfil* pPerfil, CVirtualHID* pVhid)
{
	pNotificaciones = this;
	this->pPerfil = pPerfil;
	this->pVhid = pVhid;
	hEvColaVacia_SoloHolds = CreateEvent(NULL, TRUE, FALSE, NULL);
	hWaitLockEventos = CreateSemaphore(NULL, 1, 1, NULL);
	hRatonTimer = CreateThreadpoolTimer(EvtTickRaton, this, NULL);
}

CProcesarSalida::~CProcesarSalida()
{
	pNotificaciones = nullptr;
	LimpiarEventos();
	CloseThreadpoolTimer(hRatonTimer);
	CloseHandle(hWaitLockEventos);
	CloseHandle(hEvColaVacia_SoloHolds);
}

void CProcesarSalida::LimpiarEventos()
{
	WaitForSingleObject(hWaitLockEventos, INFINITE);
	{
		SetThreadpoolTimer(hRatonTimer, NULL, 0, 0);
		WaitForThreadpoolTimerCallbacks(hRatonTimer, TRUE);
		while (!colaEventos.empty())
		{
			delete colaEventos.front();
			colaEventos.pop_front();
		}
		ResetEvent(hEvColaVacia_SoloHolds);

		while (!listaTimersDelay.empty())
		{
			TIMER_CTX* timerCtx = listaTimersDelay.front();
			SetThreadpoolTimer(timerCtx->Timer, NULL, 0, 0);
			if (!IsThreadpoolTimerSet(timerCtx->Timer))
			{
				delete timerCtx->Cola;
				CloseThreadpoolTimer(timerCtx->Timer);
				delete timerCtx;
			}
			else
			{
				if (timerCtx->Cola != nullptr)
				{
					delete timerCtx->Cola;
					timerCtx->Cola = nullptr;
				}
			}
			listaTimersDelay.pop_front();
		}

		RtlZeroMemory(pVhid->Estado.Teclado, sizeof(pVhid->Estado.Teclado));
		RtlZeroMemory(&pVhid->Estado.Raton, sizeof(pVhid->Estado.Raton)); //no hace falta lock aquí
		RtlZeroMemory(pVhid->Estado.DirectX, sizeof(VHID_INPUT_DATA));
	}
	ReleaseSemaphore(hWaitLockEventos, 1, NULL);
}

void CProcesarSalida::Procesar(CPaqueteEvento* paq)
{
	if (paq != nullptr)
	{
		WaitForSingleObject(hWaitLockEventos, INFINITE);
		colaEventos.push_back(paq);
		SetEvent(hEvColaVacia_SoloHolds);
		ReleaseSemaphore(hWaitLockEventos, 1, NULL);
	}
	ProcesarRequest();
}

void CProcesarSalida::ProcesarRequest()
{
	bool vacia;
	WaitForSingleObject(hWaitLockEventos, INFINITE);
	{
		vacia = colaEventos.empty();
		if (!vacia)
		{
			bool soloHolds = true;
			bool cmds = false, ncmds = false, x52 =false, nxt = false, md = false, pk = false;
			std::deque<CPaqueteEvento*>::iterator posEvento = colaEventos.begin();

			while (posEvento != colaEventos.end())
			{
				if (cmds && ncmds)
				{
					break;
				}
				CPaqueteEvento* colaComandos = *posEvento;
				bool borrado = false;
				PEV_COMANDO comando = colaComandos->GetColaComandos()->front();
				if (comando->Tipo != TipoComando::Hold)
				{
					soloHolds = false;
				}

				if ((comando->Tipo == TipoComando::Delay) || (comando->Tipo == TipoComando::Hold) || ((comando->Tipo & 0x7f) == TipoComando::Repeat) || ((comando->Tipo & 0x7f) == TipoComando::RepeatN))
				{
					if (!cmds || !ncmds)
					{
						if (CEspeciales::Procesar(pPerfil, &posEvento, this))
						{
							if (colaComandos->GetColaComandos()->empty())
							{
								delete colaComandos;
								posEvento = colaEventos.erase(posEvento);
							}
							continue;
						}
						else
						{
							cmds = true;
							borrado = true;
						}
					}
				}
				else if (!ncmds)
				{
					if (comando->Tipo == TipoComando::Reservado_DxPosicion)
					{
						ncmds = true;
						CDirectX::Posicion(comando, pVhid);
						borrado = true;
					}
					if (comando->Tipo == TipoComando::Reservado_CheckHold)
					{
						borrado = true;
					}
					else if ((comando->Tipo & 0x7f) == TipoComando::DxBoton)
					{
						ncmds = true;
						CDirectX::Botones_Setas(comando, pVhid);
						borrado = true;
					}
					else if ((comando->Tipo & 0x7f) == TipoComando::DxSeta)
					{
						ncmds = true;
						CDirectX::Botones_Setas(comando, pVhid);
						borrado = true;
					}
					else if ((comando->Tipo & 0x7f) == TipoComando::Tecla)
					{
						ncmds = true;
						CTeclado::Procesar(comando, pVhid);
						borrado = true;
					}
					else if (comando->Tipo == TipoComando::Modo) //Cambio modo
					{
						pPerfil->LockEstado();
						{
							pPerfil->GetEstado()->Modos = comando->Dato;
						}
						pPerfil->UnlockEstado();
						borrado = true;
					}
					else if (comando->Tipo == TipoComando::Pinkie) //Cambio modo Pinkie
					{
						pPerfil->LockEstado();
						{
							pPerfil->GetEstado()->Pinkie = comando->Dato;
						}
						pPerfil->UnlockEstado();
						borrado = true;
					}
					else if (CX52::Procesar(colaComandos))
					{
						ncmds = true;
						borrado = true;
					}
					else
					{
						bool setTimer = false;
						if (CRaton::Procesar(pVhid, comando, &setTimer))
						{
							if (setTimer)
							{
								UCHAR tick = 100;
								pPerfil->InicioLecturaPr();
								{
									tick = pPerfil->GetPr()->TickRaton;
								}
								pPerfil->FinLecturaPr();
								LARGE_INTEGER lit{};
								lit.QuadPart = -10000LL * tick;
								FILETIME ft{};
								ft.dwHighDateTime = lit.HighPart;
								ft.dwLowDateTime = lit.LowPart;
								SetThreadpoolTimer(hRatonTimer, &ft, 0, 0);
							}
							else
							{
								SetThreadpoolTimer(hRatonTimer, NULL, 0, 0);
							}
							ncmds = true;
							borrado = true;
						}
					}
				}

				if (borrado)
				{
					delete comando;
					colaComandos->GetColaComandos()->pop_front();
				}

				if (colaComandos->GetColaComandos()->empty())
				{
					delete colaComandos;
					posEvento = colaEventos.erase(posEvento);
					continue;
				}
				posEvento++;
			}

			vacia = soloHolds;
		}
	}
	ReleaseSemaphore(hWaitLockEventos, 1, NULL);

	if (vacia)
	{
		ResetEvent(hEvColaVacia_SoloHolds);
	}
}

void APIENTRY CProcesarSalida::EvtTickRaton(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context == NULL)
	{
		return;
	}

	CProcesarSalida* local = static_cast<CProcesarSalida*>(Context);
	bool enviar = false;
	EV_COMANDO comando;
	RtlZeroMemory(&comando, sizeof(EV_COMANDO));
	
	local->pVhid->LockRaton();
	{
		if (local->pVhid->Estado.Raton.X != 0)
		{
			if (local->pVhid->Estado.Raton.X < 0)
			{
				comando.Tipo = TipoComando::RatonIzq;
				comando.Dato = -local->pVhid->Estado.Raton.X;
			}
			else
			{
				comando.Tipo = TipoComando::RatonDer;
				comando.Dato = local->pVhid->Estado.Raton.X;
			}
			enviar = true;
		}
	}
	local->pVhid->UnlockRaton();
	if (enviar)
	{
		CGenerarEventos::Raton(&comando);
	}

	enviar = false;
	local->pVhid->LockRaton();
	{
		if (local->pVhid->Estado.Raton.Y != 0)
		{
			if (local->pVhid->Estado.Raton.Y < 0)
			{
				comando.Tipo = TipoComando::RatonArr;
				comando.Dato = -local->pVhid->Estado.Raton.Y;
			}
			else
			{
				comando.Tipo = TipoComando::RatonAba;
				comando.Dato = local->pVhid->Estado.Raton.Y;
			}
			enviar = true;
		}
	}
	local->pVhid->UnlockRaton();
	if (enviar)
	{
		CGenerarEventos::Raton(&comando);
	}
}

void CProcesarSalida::ProcesarDelay(TIMER_CTX* ctx)
{
	WaitForSingleObject(hWaitLockEventos, INFINITE);
	{
		if (ctx->Cola != nullptr)
		{
			colaEventos.push_back(ctx->Cola);
			ctx->Cola = nullptr;
		}
		SetEvent(hEvColaVacia_SoloHolds);

		std::deque<TIMER_CTX*>::iterator pos = listaTimersDelay.begin();
		while (pos != listaTimersDelay.end())
		{
			if (*pos == ctx)
			{
				listaTimersDelay.erase(pos);
				break;
			}
			pos++;
		}
	}
	ReleaseSemaphore(hWaitLockEventos, 1, NULL);
	CloseThreadpoolTimer(ctx->Timer);
	delete ctx;
}

