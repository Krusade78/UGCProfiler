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

//void CBotonesSetas::PressHat(CProfile* pProfile, UINT32 joyId, UCHAR idx)
//{
//	UINT16 accionId;
//
//	pProfile->BeginProfileRead();
//	{
//		pProfile->LockStatus();
//		{
//			UCHAR mode = pProfile->GetStatus()->Mode | pProfile->GetStatus()->SubMode << 4;
//
//			PROGRAMMING* pdevExt = pProfile->GetProfile();
//			if (pProfile->GetStatus()->Hats)
//			accionId = pdevExt->MapaSetas[joy][pinkie][modos][idx].Indices[pProfile->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos];
//			if (pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices > 0)
//			{
//				pProfile->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos++;
//			}
//			if (pProfile->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos == pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices)
//			{
//				pProfile->GetStatus()->Hats[joy][pinkie][modos][idx].CurrentPos = 0;
//			}
//			pProfile->GetStatus()->DxHats[joy][idx / 8] |= 1 << (idx % 8);
//		}
//		pProfile->UnlockStatus();
//	}
//	pProfile->EndProfileRead();
//
//	CGenerateEvents::Command(joyId, accionId, idx, CGenerateEvents::Origin::Hat, nullptr);
//}
//
//void CBotonesSetas::ReleaseHat(CProfile* pProfile, UINT32 joyId, UCHAR idx)
//{
//	pProfile->InitProfileRead();
//	{
//		UCHAR pinkie;
//		UCHAR modos;
//		pProfile->LockStatus();
//		{
//			pinkie = pProfile->GetStatus()->SubMode;
//			modos = pProfile->GetStatus()->Mode;
//			pProfile->GetStatus()->DxHats[joy][idx / 8] &= ~(1 << (idx % 8));
//		}
//		pProfile->UnlockStatus();
//
//		CProfile::PROGRAMMING* pdevExt = pProfile->GetProfile();
//		if (pdevExt->MapaSetas[joy][pinkie][modos][idx].TamIndices == 0)
//		{
//			UINT16 accionId = pdevExt->MapaSetas[joy][pinkie][modos][idx].Indices[1];
//			pProfile->EndProfileRead();
//			if (accionId == 0)
//			{
//				CGenerateEvents::CheckHolds();
//			}
//			else
//			{
//				CGenerateEvents::Command(joyId, accionId, idx, CGenerateEvents::Origin::Hat, nullptr);
//			}
//		}
//		else
//		{
//			pProfile->EndProfileRead();
//			CGenerateEvents::CheckHolds();
//		}
//	}
//}