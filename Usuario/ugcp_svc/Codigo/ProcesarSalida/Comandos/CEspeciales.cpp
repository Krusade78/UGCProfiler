#include "../../framework.h"
#include "CEspeciales.h"
#include "../CProcesarSalida.h"

/// <summary>
/// Hold, Delay y Repeticiones
/// </summary>
/// <returns><para>false: borra comando[0] normal</para>
/// <para>true: reprocesa/salta la acción</para>
/// </returns>
bool CEspeciales::Procesar(CPerfil* pPerfil, std::deque<CPaqueteEvento*>::iterator* posEvento, void* padre)
{
	CPaqueteEvento* colaComandos = **posEvento;
	CProcesarSalida* local = static_cast<CProcesarSalida*>(padre);
	PEV_COMANDO comando = colaComandos->GetColaComandos()->front();

	if (comando->Tipo == TipoComando::Delay) // Delay
	{
		CProcesarSalida::TIMER_CTX* ctx = new CProcesarSalida::TIMER_CTX{};
		PTP_TIMER timerHandle = CreateThreadpoolTimer(EvtDelay, ctx, NULL);
		if (timerHandle != NULL)
		{
			LARGE_INTEGER t{};
			t.QuadPart = (-1000000LL * comando->Dato); //10 * 1000 * 100
			FILETIME timeout{};
			timeout.dwHighDateTime = t.HighPart;
			timeout.dwLowDateTime = t.LowPart;

			delete comando;
			colaComandos->GetColaComandos()->pop_front();

			ctx->Timer = timerHandle;
			ctx->Padre = local;
			ctx->Cola = colaComandos;
			local->GetListaTimersDelay()->push_back(ctx); //dentro de mutexEventos

			SetThreadpoolTimer(timerHandle, &timeout, 0, 0);

			*posEvento = local->GetColaEventos()->erase(*posEvento);
		}
		else
		{
			delete ctx;
			return false;
		}
	}
	else if (comando->Tipo == TipoComando::Hold) // Autorepeat hold
	{
		if (!EstaHold(pPerfil, comando))
		{
			delete comando;
			colaComandos->GetColaComandos()->pop_front();
		}
		else
		{
			(*posEvento)++; //pasa a la siguiente acción
		}
	}
	else if (comando->Tipo == TipoComando::Repeat)
	{
		if (!EstaHold(pPerfil, comando)) // fin autorepeat infinito
		{
			BorrarBloqueRepeat(colaComandos, TipoComando::Repeat);
		}
		else
		{
			CopiarColaConRepeticion(colaComandos, TipoComando::Repeat);
		}
	}
	else if (comando->Tipo == TipoComando::RepeatN)
	{
		if (comando->Dato == 0)
		{
			BorrarBloqueRepeat(colaComandos, TipoComando::RepeatN);
		}
		else
		{
			comando->Dato--;
			CopiarColaConRepeticion(colaComandos, TipoComando::RepeatN);
		}
	}

	return true;
}

void CALLBACK CEspeciales::EvtDelay(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CProcesarSalida::TIMER_CTX* ctx = static_cast<CProcesarSalida::TIMER_CTX*>(Context);
		ctx->Padre->ProcesarDelay(ctx);
	}
}

bool CEspeciales::EstaHold(CPerfil* pPerfil, PEV_COMANDO comando)
{
	bool pulsado;

	pPerfil->LockEstado(); 
	{
		if ((comando->Extendido.Origen & 128) == 128) //eje
		{
			UCHAR modo = pPerfil->GetEstado()->Modos;
			UCHAR pinkie = pPerfil->GetEstado()->Pinkie;
			USHORT posIncremental = pPerfil->GetEstado()->Ejes[comando->Extendido.JoyId][pinkie][modo][(comando->Extendido.Origen & 127)].PosIncremental;
			UCHAR banda = pPerfil->GetEstado()->Ejes[comando->Extendido.JoyId][pinkie][modo][(comando->Extendido.Origen & 127)].Banda;
			pulsado = !((comando->Extendido.Modo == modo) && (comando->Extendido.Pinkie == pinkie) && ((comando->Extendido.Incremental != posIncremental) || (comando->Extendido.Banda != banda)));
		}
		else
		{
			if ((comando->Extendido.Origen & 64) == 64) //seta
			{
				pulsado = pPerfil->GetEstado()->SetasDx[comando->Extendido.JoyId][(comando->Extendido.Origen & 63) / 8] != 0;
			}
			else //botón
			{
				pulsado = (pPerfil->GetEstado()->BotonesDx[comando->Extendido.JoyId][comando->Extendido.Origen / 8] >> (comando->Extendido.Origen % 8)) & 1;
			}
		}
	}
	pPerfil->UnlockEstado();

	return pulsado;
}

void CEspeciales::BorrarBloqueRepeat(CPaqueteEvento* cola, UCHAR tipoComando)
{
	std::deque<PEV_COMANDO>::iterator pos = cola->GetColaComandos()->begin();
	pos++;;
	UCHAR anidados = (tipoComando == TipoComando::RepeatN) ? 1 : 0;;
	while (pos < cola->GetColaComandos()->end())
	{
		PEV_COMANDO fin = *pos;
		if ((fin->Tipo & 0x7f) == TipoComando::RepeatN)
		{
			if ((fin->Tipo & TipoComando::Soltar) == TipoComando::Soltar)
				anidados--;
			else
				anidados++;
		}
		if (((fin->Tipo & 0x7f) == tipoComando) && ((fin->Tipo & TipoComando::Soltar) == TipoComando::Soltar) && (anidados == 0))
		{
			delete fin;
			cola->GetColaComandos()->erase(pos);
			break;
		}
		pos++;
	}

	delete cola->GetColaComandos()->front();
	cola->GetColaComandos()->pop_front();
}

void CEspeciales::CopiarColaConRepeticion(CPaqueteEvento* cola, UCHAR tipoComando)
{
	std::deque<PEV_COMANDO> colaAux;
	std::deque<PEV_COMANDO>::iterator posCopia = cola->GetColaComandos()->begin();
	posCopia++;
	UCHAR anidados = (tipoComando == TipoComando::RepeatN) ? 1 : 0;

	while (posCopia != cola->GetColaComandos()->end())
	{
		PEV_COMANDO comOrigen = *posCopia;
		if ((comOrigen->Tipo & 0x7f) == TipoComando::RepeatN)
		{
			if ((comOrigen->Tipo & TipoComando::Soltar) == TipoComando::Soltar)
				anidados--;
			else
				anidados++;
		}
		if (((comOrigen->Tipo & 0x7f) == tipoComando) && ((comOrigen->Tipo & TipoComando::Soltar) == TipoComando::Soltar) && (anidados == 0))
		{
			break;
		}
		else
		{
			PEV_COMANDO mem = new EV_COMANDO;
			RtlCopyMemory(mem, comOrigen, sizeof(EV_COMANDO));
			colaAux.push_back(mem);
		}
		posCopia++;
	}

	while (!colaAux.empty())
	{
		cola->GetColaComandos()->push_front(colaAux.back());
		colaAux.pop_back();
	}
}

