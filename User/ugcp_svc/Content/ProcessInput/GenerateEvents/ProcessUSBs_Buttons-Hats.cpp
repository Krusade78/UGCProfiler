#include "../../framework.h"
#include "../../X52/MFDMenu.h"
#include "ProcessUSBs_Buttons-Hats.h"
#include "CGenerateEvents.h"

void CBotonesSetas::PressButton(CProfile* pProfile, UINT32 joyId, UCHAR idx)
{
	UINT16 actionId = 0;
	pProfile->BeginProfileRead();
	{
		pProfile->LockStatus();
		{
			UCHAR mode = pProfile->GetStatus()->Mode | pProfile->GetStatus()->SubMode << 4;

			PROGRAMMING* pdevExt = pProfile->GetProfile();
			UCHAR pos;
			if (pProfile->GetStatus()->Buttons.GetPos(&pos, joyId, mode, idx))
			{
				PROGRAMMING::BUTTONMODEL* actions = pProfile->GetProfile()->ButtonsMap.GetConf(joyId, mode, idx);
				if (actions != nullptr)
				{
					actionId = actions->Actions.at(pos);
					if (actions->Type == 1)
					{
						pProfile->GetStatus()->Buttons.SetPos(1, false, joyId, mode, idx);
						if ((pos + 1) >= pProfile->GetProfile()->ButtonsMap.GetConf(joyId, mode, idx)->Actions.size())
						{
							pProfile->GetStatus()->Buttons.SetPos(0, true, joyId, mode, idx);
						}
					}
				}
				pProfile->GetStatus()->Buttons.SetPressed(1, joyId, idx);
			}
		}
		pProfile->UnlockStatus();
	}
	pProfile->EndProfileRead();

	CGenerateEvents::Command(joyId, actionId, idx, CGenerateEvents::Origin::Button, nullptr);
}

void CBotonesSetas::ReleaseButton(CProfile* pProfile, UINT32 joyId, UCHAR idx)
{
	pProfile->BeginProfileRead();
	{
		UCHAR mode;
		pProfile->LockStatus();
		{
			mode = pProfile->GetStatus()->Mode | pProfile->GetStatus()->SubMode << 4;
			pProfile->GetStatus()->Buttons.SetPressed(0, joyId, idx);
		}
		pProfile->UnlockStatus();

		PROGRAMMING::BUTTONMODEL* actions = pProfile->GetProfile()->ButtonsMap.GetConf(joyId, mode, idx);
		if (actions != nullptr)
		{
			if (actions->Type == 0)
			{
				UINT16 accionId = 0;
				if (actions->Actions.size() == 2)
				{
					accionId = actions->Actions.at(1);
				}
				pProfile->EndProfileRead();
				if (accionId != 0)
				{
					CGenerateEvents::Command(joyId, accionId, idx, CGenerateEvents::Origin::Button, nullptr);
					return;
				}
			}
			else
			{
				pProfile->EndProfileRead();
			}
		}
		else
		{
			pProfile->EndProfileRead();
		}
		CGenerateEvents::CheckHolds();
	}
}

//void CBotonesSetas::PulsarSeta(CProfile* pPerfil, TipoJoy tipo, UCHAR idx)
//{
//	UCHAR joy = static_cast<UCHAR>(tipo);
//	UINT16 accionId;
//
//	pPerfil->InitProfileRead();
//	{
//		pPerfil->LockStatus();
//		{
//			UCHAR pinkie = pPerfil->GetStatus()->SubMode;
//			UCHAR modos = pPerfil->GetStatus()->Mode;
//
//			CProfile::PROGRAMMING* pdevExt = pPerfil->GetProfile();
//			accionId = pdevExt->MapaSetas[joy][pinkie][modos][idx].Indices[pPerfil->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos];
//			if (pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices > 0)
//			{
//				pPerfil->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos++;
//			}
//			if (pPerfil->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos == pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices)
//			{
//				pPerfil->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos = 0;
//			}
//			pPerfil->GetStatus()->DxHats[joy][idx / 8] |= 1 << (idx % 8);
//		}
//		pPerfil->UnlockStatus();
//	}
//	pPerfil->EndProfileRead();
//
//	CGenerarEventos::Comando(tipo, accionId, idx, CGenerarEventos::Origen::Seta, nullptr);
//}
//
//void CBotonesSetas::SoltarSeta(CProfile* pPerfil, TipoJoy tipo, UCHAR idx)
//{
//	UCHAR joy = static_cast<UCHAR>(tipo);
//
//	pPerfil->InitProfileRead();
//	{
//		UCHAR pinkie;
//		UCHAR modos;
//		pPerfil->LockStatus();
//		{
//			pinkie = pPerfil->GetStatus()->SubMode;
//			modos = pPerfil->GetStatus()->Mode;
//			pPerfil->GetStatus()->DxHats[joy][idx / 8] &= ~(1 << (idx % 8));
//		}
//		pPerfil->UnlockStatus();
//
//		CProfile::PROGRAMMING* pdevExt = pPerfil->GetProfile();
//		if (pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices == 0)
//		{
//			UINT16 accionId = pdevExt->MapaSetas[joy][pinkie][modos][idx].Indices[1];
//			pPerfil->EndProfileRead();
//			if (accionId == 0)
//			{
//				CGenerarEventos::CheckHolds();
//			}
//			else
//			{
//				CGenerarEventos::Comando(tipo, accionId, idx, CGenerarEventos::Origen::Seta, nullptr);
//			}
//		}
//		else
//		{
//			pPerfil->EndProfileRead();
//			CGenerarEventos::CheckHolds();
//		}
//	}
//}