#include "../../framework.h"
#include <deque>
#include "CGenerarEventos.h"

#ifdef _DEBUG
CPerfil* CGenerarEventos::pPerfil;
CColaEventos* CGenerarEventos::pColaEv;
#endif

void CGenerarEventos::Iniciar(CPerfil* pPerfil, CColaEventos* pColaEv)
{
	CGenerarEventos::pPerfil = pPerfil;
	CGenerarEventos::pColaEv = pColaEv;
}

void CGenerarEventos::Raton(PEV_COMANDO pev_comando)
{
	CPaqueteEvento* pEvento = new CPaqueteEvento();
	PEV_COMANDO comando = new EV_COMANDO;
	RtlCopyMemory(comando, pev_comando, sizeof(EV_COMANDO));
	pEvento->AñadirComando(comando);
	pColaEv->Añadir(pEvento);
}

void CGenerarEventos::Comando(TipoJoy idJoy, UINT16 accionId, UCHAR origen, Origen tipoOrigen, PEV_COMANDO datosEje)
{
	if (tipoOrigen == Origen::Seta)
	{
		origen += 64;
	}
	else if (tipoOrigen == Origen::Eje)
	{
		origen += 128;
	}

	if (accionId != 0)
	{
		CPaqueteEvento* pEvento = new CPaqueteEvento();
		pPerfil->InicioLecturaPr();
		{
			if (pPerfil->GetPr()->Acciones->empty())
			{
				delete pEvento;
				pPerfil->FinLecturaPr();
				return;
			}
			else
			{
				std::deque<PEV_COMANDO>*& comandos = pPerfil->GetPr()->Acciones->at(accionId - 1);
				std::deque<PEV_COMANDO>::iterator idx = comandos->begin();
				while(idx != comandos->end())
				{
					PEV_COMANDO pEvt = new EV_COMANDO;
					RtlCopyMemory(pEvt, *idx, sizeof(EV_COMANDO));
					if ((pEvt->Tipo == TipoComando::Hold) || ((pEvt->Tipo & 0x7f) == TipoComando::Repeat))
					{
						pEvt->Extendido.Origen = origen;
						pEvt->Extendido.JoyId = static_cast<UCHAR>(idJoy);
						if (tipoOrigen == Origen::Eje)
						{
							pEvt->Extendido.Pinkie = datosEje->Extendido.Pinkie;
							pEvt->Extendido.Modo = datosEje->Extendido.Modo;
							pEvt->Extendido.Incremental = datosEje->Extendido.Incremental;
							pEvt->Extendido.Banda = datosEje->Extendido.Banda;
						}
					}
					pEvento->AñadirComando(pEvt);
					idx++;
				}
			}
		}
		pPerfil->FinLecturaPr();

		pColaEv->Añadir(pEvento);
	}
}

void CGenerarEventos::DirectX(UCHAR joyId, UCHAR mapa, PVHID_INPUT_DATA inputData)
{
	CPaqueteEvento* pEvento = new CPaqueteEvento();
	PEV_COMANDO comando = new EV_COMANDO;
	RtlZeroMemory(comando, sizeof(EV_COMANDO));
	comando->Tipo = TipoComando::Reservado_DxPosicion;
	comando->VHid.JoyId = joyId;
	comando->VHid.Mapa = mapa;
	RtlCopyMemory(&comando->VHid.Datos, inputData, sizeof(VHID_INPUT_DATA));
	pEvento->AñadirComando(comando);
	pColaEv->Añadir(pEvento);
}
