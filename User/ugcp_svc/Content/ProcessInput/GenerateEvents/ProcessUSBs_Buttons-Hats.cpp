#include "../../framework.h"
#include "../../X52/MFDMenu.h"
#include "ProcessUSBs_Buttons-Hats.h"
#include "CGenerateEvents.h"

CButtonsHats* CButtonsHats::mainProcess = nullptr;

CButtonsHats::CButtonsHats(CProfile* pProfile)
{
	this->pProfile = pProfile;
	pStButtonsMap = new std::unordered_map<UINT64, PTP_TIMER>();
	pStHatsMap = new std::unordered_map<UINT64, PTP_TIMER>();
	hMutexStatus = CreateMutex(NULL, FALSE, NULL);
}

CButtonsHats::~CButtonsHats()
{
	if (mainProcess == nullptr)
	{
		WaitForSingleObject(hMutexStatus, INFINITE);
		
		for (std::unordered_map<UINT64, PTP_TIMER>::const_iterator it = pStButtonsMap->begin(); it != pStButtonsMap->end(); ++it)
		{
			SetThreadpoolTimer(it->second, NULL, 0, 0);
			WaitForThreadpoolTimerCallbacks(it->second, TRUE);
			CloseThreadpoolTimer(it->second);
		}
		pStButtonsMap->clear();

		for (std::unordered_map<UINT64, PTP_TIMER>::const_iterator it = pStHatsMap->begin(); it != pStHatsMap->end(); ++it)
		{
			SetThreadpoolTimer(it->second, NULL, 0, 0);
			WaitForThreadpoolTimerCallbacks(it->second, TRUE);
			CloseThreadpoolTimer(it->second);
		}
		pStHatsMap->clear();

		ReleaseMutex(hMutexStatus);
		CloseHandle(hMutexStatus); hMutexStatus = nullptr;

		delete pStButtonsMap; pStButtonsMap = nullptr;
		delete pStHatsMap; pStHatsMap = nullptr;
	}
}

#pragma region static
void CButtonsHats::CreateInstance(CProfile* pProfile)
{
	if (mainProcess == nullptr)
	{
		mainProcess = new CButtonsHats(pProfile);
	}
}

void CButtonsHats::PressButton(CProfile* pProfile, UINT32 joyId, UCHAR idx)
{
	CreateInstance(pProfile);
	mainProcess->PressButton(joyId, idx, false);
}

void CButtonsHats::ReleaseButton(CProfile* pProfile, UINT32 joyId, UCHAR idx)
{
	CreateInstance(pProfile);
	mainProcess->ReleaseButton(joyId, idx, false);
}

void CButtonsHats::PressHat(CProfile* pProfile, UINT32 joyId, UCHAR idx)
{
	CreateInstance(pProfile);
	mainProcess->PressHat(joyId, idx, false);
}

void CButtonsHats::ReleaseHat(CProfile* pProfile, UINT32 joyId, UCHAR idx)
{
	CreateInstance(pProfile);
	mainProcess->ReleaseHat(joyId, idx, false);
}
#pragma endregion

void CButtonsHats::PressButton(UINT32 joyId, UCHAR idx, bool longPress)
{
	UINT16 actionId = 0;
	bool shortPress = 0;

	pProfile->BeginProfileRead();
	{
		pProfile->LockStatus();
		{
			UCHAR mode = pProfile->GetStatus()->Mode | pProfile->GetStatus()->SubMode << 4;

			PROGRAMMING* pdevExt = pProfile->GetProfile();
			UCHAR pos;
			if (pProfile->GetStatus()->Buttons.GetPos(&pos, joyId, mode, idx))
			{
				PROGRAMMING::BUTTONMODEL* actions = pProfile->GetProfile()->ButtonsMap.GetConf(joyId, &mode, idx);
				if (actions != nullptr)
				{
					actionId = actions->Actions.at(pos);
					if (actions->Type == 1)
					{
						pProfile->GetStatus()->Buttons.SetPos(1, false, joyId, mode, idx);
						if (static_cast<UCHAR>(pos + 1) >= pProfile->GetProfile()->ButtonsMap.GetConf(joyId, &mode, idx)->Actions.size())
						{
							pProfile->GetStatus()->Buttons.SetPos(0, true, joyId, mode, idx);
						}
					}
					else if (actions->Type == 2)
					{
						if (pos == 2)
						{
							shortPress = true;
							pProfile->GetStatus()->Buttons.SetPos(0, true, joyId, mode, idx);
						}
						if (!longPress)
						{
							CButtonsHats::TIMER_CTX* ctx = new CButtonsHats::TIMER_CTX{};
							PTP_TIMER timerHandle = CreateThreadpoolTimer(EvtLongPress, ctx, NULL);
							if (timerHandle != NULL)
							{
								LARGE_INTEGER t{};
								t.QuadPart = (-10000LL * 1200); //10 * 1000 * 1.5 sec
								FILETIME timeout{};
								timeout.dwHighDateTime = t.HighPart;
								timeout.dwLowDateTime = t.LowPart;

								ctx->Timer = timerHandle;
								ctx->Parent = this;
								ctx->JoyId = joyId;
								ctx->HatButton_Idx = idx;

								WaitForSingleObject(hMutexStatus, INFINITE);
								{
									pStButtonsMap->insert({ (static_cast<UINT64>(joyId) << 8) | idx, timerHandle });
								}
								ReleaseMutex(hMutexStatus);
								SetThreadpoolTimer(timerHandle, &timeout, 0, 0);
							}
							else
							{
								delete ctx;
							}
							pProfile->UnlockStatus();
							pProfile->EndProfileRead();
							return;
						}
					}
				}
				pProfile->GetStatus()->Buttons.SetPressed(1, joyId, idx);
			}
		}
		pProfile->UnlockStatus();
	}
	pProfile->EndProfileRead();

	CGenerateEvents::Command(joyId, actionId, idx, shortPress ? CGenerateEvents::Origin::ButtonShort : CGenerateEvents::Origin::Button, nullptr);
}

void CButtonsHats::ReleaseButton(UINT32 joyId, UCHAR idx, bool shortRelease)
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

		PROGRAMMING::BUTTONMODEL* actions = pProfile->GetProfile()->ButtonsMap.GetConf(joyId, &mode, idx);
		if (actions != nullptr)
		{
			bool longRelease = false;
			if (actions->Type == 2)
			{
				if (shortRelease)
				{
					UINT16 accionId = 0;
					if (actions->Actions.size() == 4)
					{
						accionId = actions->Actions.at(3);
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
					WaitForSingleObject(hMutexStatus, INFINITE);
					{
						UINT64 id = (static_cast<UINT64>(joyId) << 8) | idx;
						if (pStButtonsMap->contains(id)) //is waiting for longpress so do a short press/release
						{
							pStButtonsMap->erase(id);
							pProfile->LockStatus();
							{
								pProfile->GetStatus()->Buttons.SetPos(2, true, joyId, mode, idx);
							}
							pProfile->UnlockStatus();
							ReleaseMutex(hMutexStatus);
							PressButton(joyId, idx, true);

							CButtonsHats::TIMER_CTX* ctx = new CButtonsHats::TIMER_CTX{};
							PTP_TIMER timerHandle = CreateThreadpoolTimer(EvtShortRelease, ctx, NULL);
							if (timerHandle != NULL)
							{
								LARGE_INTEGER t{};
								t.QuadPart = (-10000LL * 300); //10 * 1000 * 100 * 1 sec
								FILETIME timeout{};
								timeout.dwHighDateTime = t.HighPart;
								timeout.dwLowDateTime = t.LowPart;

								ctx->Timer = timerHandle;
								ctx->Parent = this;
								ctx->JoyId = joyId;
								ctx->HatButton_Idx = idx;

								WaitForSingleObject(hMutexStatus, INFINITE);
								{
									pStButtonsMap->insert({ (1ull << 48) | (static_cast<UINT64>(joyId) << 8) | idx, timerHandle });
								}
								ReleaseMutex(hMutexStatus);
								SetThreadpoolTimer(timerHandle, &timeout, 0, 0);
								pProfile->EndProfileRead();
								return;
							}
						}
						else
						{
							ReleaseMutex(hMutexStatus);
						}
					}
					longRelease = true;
				}
			}
			if ((actions->Type == 0) || ((actions->Type == 2) && longRelease))
			{
				UINT16 accionId = 0;
				if (actions->Actions.size() > 1)
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

void CButtonsHats::PressHat(UINT32 joyId, UCHAR idx, bool longPress)
{
	UINT16 actionId = 0;
	bool shortPress = 0;

	pProfile->BeginProfileRead();
	{
		pProfile->LockStatus();
		{
			UCHAR mode = pProfile->GetStatus()->Mode | pProfile->GetStatus()->SubMode << 4;

			PROGRAMMING* pdevExt = pProfile->GetProfile();
			UCHAR pos;
			if (pProfile->GetStatus()->Hats.GetPos(&pos, joyId, mode, idx))
			{
				PROGRAMMING::BUTTONMODEL* actions = pProfile->GetProfile()->HatsMap.GetConf(joyId, &mode, idx);
				if (actions != nullptr)
				{
					actionId = actions->Actions.at(pos);
					if (actions->Type == 1)
					{
						pProfile->GetStatus()->Hats.SetPos(1, false, joyId, mode, idx);
						if (static_cast<UCHAR>(pos + 1) >= pProfile->GetProfile()->HatsMap.GetConf(joyId, &mode, idx)->Actions.size())
						{
							pProfile->GetStatus()->Buttons.SetPos(0, true, joyId, mode, idx);
						}
					}
					else if (actions->Type == 2)
					{
						if (pos == 2)
						{
							shortPress = true;
							pProfile->GetStatus()->Hats.SetPos(0, true, joyId, mode, idx);
						}
						if (!longPress)
						{
							CButtonsHats::TIMER_CTX* ctx = new CButtonsHats::TIMER_CTX{};
							PTP_TIMER timerHandle = CreateThreadpoolTimer(EvtLongPress, ctx, NULL);
							if (timerHandle != NULL)
							{
								LARGE_INTEGER t{};
								t.QuadPart = (-10000LL * 1200); //10 * 1000 * 1.5 sec
								FILETIME timeout{};
								timeout.dwHighDateTime = t.HighPart;
								timeout.dwLowDateTime = t.LowPart;

								ctx->Timer = timerHandle;
								ctx->Parent = this;
								ctx->JoyId = joyId;
								ctx->HatButton_Idx = 256 | idx;

								WaitForSingleObject(hMutexStatus, INFINITE);
								{
									pStHatsMap->insert({ (static_cast<UINT64>(joyId) << 8) | idx, timerHandle });
								}
								ReleaseMutex(hMutexStatus);
								SetThreadpoolTimer(timerHandle, &timeout, 0, 0);
							}
							else
							{
								delete ctx;
							}
							pProfile->UnlockStatus();
							pProfile->EndProfileRead();
							return;
						}
					}
				}
				pProfile->GetStatus()->Hats.SetPressed(1, joyId, idx);
			}
		}
		pProfile->UnlockStatus();
	}
	pProfile->EndProfileRead();

	CGenerateEvents::Command(joyId, actionId, idx, shortPress ? CGenerateEvents::Origin::HatShort : CGenerateEvents::Origin::Hat, nullptr);
}

void CButtonsHats::ReleaseHat(UINT32 joyId, UCHAR idx, bool shortRelease)
{
	pProfile->BeginProfileRead();
	{
		UCHAR mode;
		pProfile->LockStatus();
		{
			mode = pProfile->GetStatus()->Mode | pProfile->GetStatus()->SubMode << 4;
			pProfile->GetStatus()->Hats.SetPressed(0, joyId, idx);
		}
		pProfile->UnlockStatus();

		PROGRAMMING::BUTTONMODEL* actions = pProfile->GetProfile()->HatsMap.GetConf(joyId, &mode, idx);
		if (actions != nullptr)
		{
			bool longRelease = false;
			if (actions->Type == 2)
			{
				if (shortRelease)
				{
					UINT16 accionId = 0;
					if (actions->Actions.size() == 4)
					{
						accionId = actions->Actions.at(3);
					}
					pProfile->EndProfileRead();
					if (accionId != 0)
					{
						CGenerateEvents::Command(joyId, accionId, idx, CGenerateEvents::Origin::Hat, nullptr);
						return;
					}
				}
				else
				{
					WaitForSingleObject(hMutexStatus, INFINITE);
					{
						UINT64 id = (static_cast<UINT64>(joyId) << 8) | idx;
						if (pStHatsMap->contains(id)) //is waiting for longpress so do a short press/release
						{
							pStHatsMap->erase(id);
							pProfile->LockStatus();
							{
								pProfile->GetStatus()->Hats.SetPos(2, true, joyId, mode, idx);
							}
							pProfile->UnlockStatus();
							ReleaseMutex(hMutexStatus);
							PressButton(joyId, idx, true);

							CButtonsHats::TIMER_CTX* ctx = new CButtonsHats::TIMER_CTX{};
							PTP_TIMER timerHandle = CreateThreadpoolTimer(EvtShortRelease, ctx, NULL);
							if (timerHandle != NULL)
							{
								LARGE_INTEGER t{};
								t.QuadPart = (-10000LL * 300); //10 * 1000 * 100 * 1 sec
								FILETIME timeout{};
								timeout.dwHighDateTime = t.HighPart;
								timeout.dwLowDateTime = t.LowPart;

								ctx->Timer = timerHandle;
								ctx->Parent = this;
								ctx->JoyId = joyId;
								ctx->HatButton_Idx = 256 | idx;

								WaitForSingleObject(hMutexStatus, INFINITE);
								{
									pStHatsMap->insert({ (1ull << 48) | (static_cast<UINT64>(joyId) << 8) | idx, timerHandle });
								}
								ReleaseMutex(hMutexStatus);
								SetThreadpoolTimer(timerHandle, &timeout, 0, 0);
								pProfile->EndProfileRead();
								return;
							}
						}
						else
						{
							ReleaseMutex(hMutexStatus);
						}
					}
					longRelease = true;
				}
			}
			if ((actions->Type == 0) || ((actions->Type == 2) && longRelease))
			{
				UINT16 accionId = 0;
				if (actions->Actions.size() > 1)
				{
					accionId = actions->Actions.at(1);
				}
				pProfile->EndProfileRead();
				if (accionId != 0)
				{
					CGenerateEvents::Command(joyId, accionId, idx, CGenerateEvents::Origin::Hat, nullptr);
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

void CALLBACK CButtonsHats::EvtLongPress(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CButtonsHats::TIMER_CTX* ctx = static_cast<CButtonsHats::TIMER_CTX*>(Context);
		UINT64 id = (static_cast<UINT64>(ctx->JoyId) << 8) | (ctx->HatButton_Idx & 0xff);

		if (ctx->HatButton_Idx >> 8) // hat
		{
			WaitForSingleObject(ctx->Parent->hMutexStatus, INFINITE);
			if (ctx->Parent->pStHatsMap->contains(id))
			{
				ctx->Parent->pStHatsMap->erase(id);
				ReleaseMutex(ctx->Parent->hMutexStatus);
				ctx->Parent->PressHat(ctx->JoyId, (ctx->HatButton_Idx & 0xff), true);
			}
			else
			{
				ReleaseMutex(ctx->Parent->hMutexStatus);
			}
		}
		else
		{
			WaitForSingleObject(ctx->Parent->hMutexStatus, INFINITE);
			if (ctx->Parent->pStButtonsMap->contains(id))
			{
				ctx->Parent->pStButtonsMap->erase(id);
				ReleaseMutex(ctx->Parent->hMutexStatus);
				ctx->Parent->PressButton(ctx->JoyId, (ctx->HatButton_Idx & 0xff), true);
			}
			else
			{
				ReleaseMutex(ctx->Parent->hMutexStatus);
			}
		}
		CloseThreadpoolTimer(ctx->Timer);
	}
}

void CALLBACK CButtonsHats::EvtShortRelease(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CButtonsHats::TIMER_CTX* ctx = static_cast<CButtonsHats::TIMER_CTX*>(Context);
		UINT64 id = (1ull << 48) | (static_cast<UINT64>(ctx->JoyId) << 8) | (ctx->HatButton_Idx & 0xff);

		if (ctx->HatButton_Idx >> 8) // hat
		{
			WaitForSingleObject(ctx->Parent->hMutexStatus, INFINITE);
			if (ctx->Parent->pStHatsMap->contains(id))
			{
				ctx->Parent->pStHatsMap->erase(id);
				ReleaseMutex(ctx->Parent->hMutexStatus);
				ctx->Parent->ReleaseHat(ctx->JoyId, (ctx->HatButton_Idx & 0xff), true);
			}
			else
			{
				ReleaseMutex(ctx->Parent->hMutexStatus);
			}
		}
		else
		{
			WaitForSingleObject(ctx->Parent->hMutexStatus, INFINITE);
			if (ctx->Parent->pStButtonsMap->contains(id))
			{
				ctx->Parent->pStButtonsMap->erase(id);
				ReleaseMutex(ctx->Parent->hMutexStatus);
				ctx->Parent->ReleaseButton(ctx->JoyId, (ctx->HatButton_Idx & 0xff), true);
			}
			else
			{
				ReleaseMutex(ctx->Parent->hMutexStatus);
			}
		}
		CloseThreadpoolTimer(ctx->Timer);
	}
}