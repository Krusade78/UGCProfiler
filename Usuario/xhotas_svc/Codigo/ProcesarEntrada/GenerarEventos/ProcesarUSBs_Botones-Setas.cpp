#include "../../framework.h"
#include "../../X52/MenuMFD.h"
#include "ProcesarUSBs_Botones-setas.h"
#include "CGenerarEventos.h"

void CBotonesSetas::PulsarBoton(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx)
{
	UCHAR joy = static_cast<UCHAR>(tipo);
	UINT16 accionId;
	pPerfil->InicioLecturaPr();
	{
		pPerfil->LockEstado();
		{
			UCHAR pinkie = pPerfil->GetEstado()->Pinkie;
			UCHAR modos = pPerfil->GetEstado()->Modos;

			CPerfil::PROGRAMADO* pdevExt = pPerfil->GetPr();
			accionId = pdevExt->MapaBotones[joy][pinkie][modos][idx].Indices[pPerfil->GetEstado()->Botones[joy][pinkie][modos][idx].PosActual];
			if (pdevExt->MapaBotones[joy][pinkie][modos][idx].TamIndices > 0)
			{
				pPerfil->GetEstado()->Botones[joy][pinkie][modos][idx].PosActual++;
			}
			if (pPerfil->GetEstado()->Botones[joy][pinkie][modos][idx].PosActual == pdevExt->MapaBotones[joy][pinkie][modos][idx].TamIndices)
			{
				pPerfil->GetEstado()->Botones[joy][pinkie][modos][idx].PosActual = 0;
			}
			pPerfil->GetEstado()->BotonesDx[joy][idx / 8] |= 1 << (idx % 8);
		}
		pPerfil->UnlockEstado();
	}
	pPerfil->FinLecturaPr();

	CGenerarEventos::Comando(tipo, accionId, idx, CGenerarEventos::Origen::Boton, nullptr);
}

void CBotonesSetas::SoltarBoton(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx)
{
	UCHAR joy = static_cast<UCHAR>(tipo);

	pPerfil->InicioLecturaPr();
	{
		UCHAR pinkie;
		UCHAR modos;
		pPerfil->LockEstado();
		{
			pinkie = pPerfil->GetEstado()->Pinkie;
			modos = pPerfil->GetEstado()->Modos;
			pPerfil->GetEstado()->BotonesDx[joy][idx / 8] &= ~(1 << (idx % 8));
		}
		pPerfil->UnlockEstado();

		CPerfil::PROGRAMADO* pdevExt = pPerfil->GetPr();
		if (pdevExt->MapaBotones[joy][pinkie][modos][idx].TamIndices == 0)
		{
			UINT16 accionId = pdevExt->MapaBotones[joy][pinkie][modos][idx].Indices[1];
			pPerfil->FinLecturaPr();

			CGenerarEventos::Comando(tipo, accionId, idx, CGenerarEventos::Origen::Boton, nullptr);
		}
		else
		{
			pPerfil->FinLecturaPr();
		}
	}
}

void CBotonesSetas::PulsarSeta(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx)
{
	UCHAR joy = static_cast<UCHAR>(tipo);
	UINT16 accionId;

	pPerfil->InicioLecturaPr();
	{
		pPerfil->LockEstado();
		{
			UCHAR pinkie = pPerfil->GetEstado()->Pinkie;
			UCHAR modos = pPerfil->GetEstado()->Modos;

			CPerfil::PROGRAMADO* pdevExt = pPerfil->GetPr();
			accionId = pdevExt->MapaSetas[joy][pinkie][modos][idx].Indices[pPerfil->GetEstado()->Setas[joy][pinkie][modos][idx].PosActual];
			if (pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices > 0)
			{
				pPerfil->GetEstado()->Setas[joy][pinkie][modos][idx].PosActual++;
			}
			if (pPerfil->GetEstado()->Setas[joy][pinkie][modos][idx].PosActual == pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices)
			{
				pPerfil->GetEstado()->Setas[joy][pinkie][modos][idx].PosActual = 0;
			}
			pPerfil->GetEstado()->SetasDx[joy][idx / 8] |= 1 << (idx % 8);
		}
		pPerfil->UnlockEstado();
	}
	pPerfil->FinLecturaPr();

	CGenerarEventos::Comando(tipo, accionId, idx, CGenerarEventos::Origen::Seta, nullptr);
}

void CBotonesSetas::SoltarSeta(CPerfil* pPerfil, TipoJoy tipo, UCHAR idx)
{
	UCHAR joy = static_cast<UCHAR>(tipo);

	pPerfil->InicioLecturaPr();
	{
		UCHAR pinkie;
		UCHAR modos;
		pPerfil->LockEstado();
		{
			pinkie = pPerfil->GetEstado()->Pinkie;
			modos = pPerfil->GetEstado()->Modos;
			pPerfil->GetEstado()->SetasDx[joy][idx / 8] &= ~(1 << (idx % 8));
		}
		pPerfil->UnlockEstado();

		CPerfil::PROGRAMADO* pdevExt = pPerfil->GetPr();
		if (pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices == 0)
		{
			UINT16 accionId = pdevExt->MapaSetas[joy][pinkie][modos][idx].Indices[1];
			pPerfil->FinLecturaPr();

			CGenerarEventos::Comando(tipo, accionId, idx, CGenerarEventos::Origen::Seta, nullptr);
		}
		else
		{
			pPerfil->FinLecturaPr();
		}
	}
}