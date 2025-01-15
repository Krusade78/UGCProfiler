#include "../framework.h"
#include "CProcessOutput.h"
#include "../ProcessInput/GenerateEvents/CGenerateEvents.h"
#include "Commands/CDirectX.h"
#include "Commands/CKeyboard.h"
#include "Commands/CMouse.h"
#include "Commands/CX52.h"
#include "Commands/CNXT.h"
#include "Commands/COther.h"

CProcessOutput* CProcessOutput::pNotifications = nullptr;

CProcessOutput::CProcessOutput(CProfile* pProfile, CVirtualHID* pVhid)
{
	pNotifications = this;
	this->pProfile = pProfile;
	this->pVhid = pVhid;
	hEvEmptyQueue_OnlyHolds = CreateEvent(NULL, TRUE, FALSE, NULL);
	hWaitLockEvents = CreateSemaphore(NULL, 1, 1, NULL);
	hMouseTimer = CreateThreadpoolTimer(EvtMouseTick, this, NULL);
}

CProcessOutput::~CProcessOutput()
{
	pNotifications = nullptr;
	ClearEvents();
	CloseThreadpoolTimer(hMouseTimer);
	CloseHandle(hWaitLockEvents);
	CloseHandle(hEvEmptyQueue_OnlyHolds);
}

void CProcessOutput::ClearEvents()
{
	WaitForSingleObject(hWaitLockEvents, INFINITE);
	{
		SetThreadpoolTimer(hMouseTimer, NULL, 0, 0);
		WaitForThreadpoolTimerCallbacks(hMouseTimer, TRUE);
		while (!eventsQueue.empty())
		{
			delete eventsQueue.front();
			eventsQueue.pop_front();
		}
		ResetEvent(hEvEmptyQueue_OnlyHolds);

		while (!timersDelayList.empty())
		{
			TIMER_CTX* timerCtx = timersDelayList.front();
			SetThreadpoolTimer(timerCtx->Timer, NULL, 0, 0);
			if (!IsThreadpoolTimerSet(timerCtx->Timer))
			{
				delete timerCtx->Queue;
				CloseThreadpoolTimer(timerCtx->Timer);
				delete timerCtx;
			}
			else
			{
				if (timerCtx->Queue != nullptr)
				{
					delete timerCtx->Queue;
					timerCtx->Queue = nullptr;
				}
			}
			timersDelayList.pop_front();
		}

		RtlZeroMemory(&pVhid->GetStatus()->Keyboard, sizeof(pVhid->GetStatus()->Keyboard));
		RtlZeroMemory(&pVhid->GetStatus()->Mouse, sizeof(pVhid->GetStatus()->Mouse)); //lock is not required here
		RtlZeroMemory(&pVhid->GetStatus()->DirectX, sizeof(VHID_INPUT_DATA));
	}
	ReleaseSemaphore(hWaitLockEvents, 1, NULL);
}

void CProcessOutput::Process(CEventPacket* packet)
{
	if (packet != nullptr)
	{
		WaitForSingleObject(hWaitLockEvents, INFINITE);
		eventsQueue.push_back(packet);
		SetEvent(hEvEmptyQueue_OnlyHolds);
		ReleaseSemaphore(hWaitLockEvents, 1, NULL);
	}
	ProcessRequest();
}

void CProcessOutput::ProcessRequest()
{
	bool empty;
	WaitForSingleObject(hWaitLockEvents, INFINITE);
	{
		empty = eventsQueue.empty();
		if (!empty)
		{
			bool onlyHolds = true;
			bool cmds = false, ncmds = false, x52 =false, nxt = false, md = false, pk = false;
			std::deque<CEventPacket*>::iterator posEvent = eventsQueue.begin();

			while (posEvent != eventsQueue.end())
			{
				if (cmds && ncmds)
				{
					break;
				}
				CEventPacket* commandQueue = *posEvent;
				bool deleted = false;
				PEV_COMMAND command = commandQueue->GetCommandQueue()->front();
				if (command->Type != CommandType::Hold)
				{
					onlyHolds = false;
				}

				if ((command->Type == CommandType::Delay) || (command->Type == CommandType::Hold) || ((command->Type & 0x7f) == CommandType::Repeat) || ((command->Type & 0x7f) == CommandType::RepeatN))
				{
					if (!cmds || !ncmds)
					{
						if (COther::Process(pProfile, &posEvent, this))
						{
							if (commandQueue->GetCommandQueue()->empty())
							{
								delete commandQueue;
								posEvent = eventsQueue.erase(posEvent);
							}
							continue;
						}
						else
						{
							cmds = true;
							deleted = true;
						}
					}
				}
				else if (!ncmds)
				{
					if (command->Type == CommandType::Reserved_DxPosition)
					{
						ncmds = true;
						CDirectX::Position(command, pVhid);
						deleted = true;
					}
					if (command->Type == CommandType::Reserved_CheckHold)
					{
						deleted = true;
					}
					else if ((command->Type & 0x7f) == CommandType::DxButton)
					{
						ncmds = true;
						CDirectX::Buttons_Hats(command, pVhid);
						deleted = true;
					}
					else if ((command->Type & 0x7f) == CommandType::DxHat)
					{
						ncmds = true;
						CDirectX::Buttons_Hats(command, pVhid);
						deleted = true;
					}
					else if ((command->Type & 0x7f) == CommandType::DxAxis)
					{
						ncmds = true;
						CDirectX::Axis(command, pVhid);
						deleted = true;
					}
					else if ((command->Type & 0x7f) == CommandType::Key)
					{
						ncmds = true;
						CKeyboard::Processed(command, pVhid);
						deleted = true;
					}
					else if ((command->Type & 0x7f) == CommandType::PreciseMode)
					{
						pProfile->LockStatus();
						{
							if ((command->Type & 0x80) == CommandType::Release)
							{
								pProfile->GetStatus()->AxisPreciseMode.SetStatus(0, command->AxisPrecise.InputJoy, command->AxisPrecise.Axis);
							}
							else
							{
								pProfile->GetStatus()->AxisPreciseMode.SetStatus(1, command->AxisPrecise.InputJoy, command->AxisPrecise.Axis);
							}
						}
						pProfile->UnlockStatus();
						deleted = true;
					}
					else if (command->Type == CommandType::Mode)
					{
						pProfile->LockStatus();
						{
							pProfile->GetStatus()->Mode = command->Basic.Data1;
						}
						pProfile->UnlockStatus();
						deleted = true;
					}
					else if (command->Type == CommandType::Submode)
					{
						pProfile->LockStatus();
						{
							pProfile->GetStatus()->SubMode = command->Basic.Data1;
						}
						pProfile->UnlockStatus();
						deleted = true;
					}
					else if (CX52::Process(commandQueue))
					{
						ncmds = true;
						deleted = true;
					}
					else if (CNXT::Process(commandQueue))
					{
						ncmds = true;
						deleted = true;
					}
					else
					{
						bool setTimer = false;
						if (CMouse::Process(pVhid, command, &setTimer))
						{
							if (setTimer)
							{
								UCHAR tick = 100;
								pProfile->BeginProfileRead();
								{
									tick = pProfile->GetProfile()->MouseTick;
								}
								pProfile->EndProfileRead();
								LARGE_INTEGER lit{};
								lit.QuadPart = -10000LL * tick;
								FILETIME ft{};
								ft.dwHighDateTime = lit.HighPart;
								ft.dwLowDateTime = lit.LowPart;
								SetThreadpoolTimer(hMouseTimer, &ft, 0, 0);
							}
							else
							{
								SetThreadpoolTimer(hMouseTimer, NULL, 0, 0);
							}
							ncmds = true;
							deleted = true;
						}
					}
				}

				if (deleted)
				{
					delete command;
					commandQueue->GetCommandQueue()->pop_front();
				}

				if (commandQueue->GetCommandQueue()->empty())
				{
					delete commandQueue;
					posEvent = eventsQueue.erase(posEvent);
					continue;
				}
				posEvent++;
			}

			empty = onlyHolds;
		}
	}
	ReleaseSemaphore(hWaitLockEvents, 1, NULL);

	if (empty)
	{
		ResetEvent(hEvEmptyQueue_OnlyHolds);
	}
}

void APIENTRY CProcessOutput::EvtMouseTick(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context == NULL)
	{
		return;
	}

	CProcessOutput* local = static_cast<CProcessOutput*>(Context);
	bool send = false;
	EV_COMMAND command;
	RtlZeroMemory(&command, sizeof(EV_COMMAND));
	
	local->pVhid->LockMouse();
	{
		if (local->pVhid->GetStatus()->Mouse.X != 0)
		{
			if (local->pVhid->GetStatus()->Mouse.X < 0)
			{
				command.Type = CommandType::MouseLeft;
				command.Basic.Data1 = -local->pVhid->GetStatus()->Mouse.X;
			}
			else
			{
				command.Type = CommandType::MouseRight;
				command.Basic.Data1 = local->pVhid->GetStatus()->Mouse.X;
			}
			send = true;
		}
	}
	local->pVhid->UnlockMouse();
	if (send)
	{
		CGenerateEvents::Mouse(&command);
	}

	send = false;
	local->pVhid->LockMouse();
	{
		if (local->pVhid->GetStatus()->Mouse.Y != 0)
		{
			if (local->pVhid->GetStatus()->Mouse.Y < 0)
			{
				command.Type = CommandType::MouseUp;
				command.Basic.Data1 = -local->pVhid->GetStatus()->Mouse.Y;
			}
			else
			{
				command.Type = CommandType::MouseDown;
				command.Basic.Data1 = local->pVhid->GetStatus()->Mouse.Y;
			}
			send = true;
		}
	}
	local->pVhid->UnlockMouse();
	if (send)
	{
		CGenerateEvents::Mouse(&command);
	}
}

void CProcessOutput::ProcessDelay(TIMER_CTX* ctx)
{
	WaitForSingleObject(hWaitLockEvents, INFINITE);
	{
		if (ctx->Queue != nullptr)
		{
			eventsQueue.push_back(ctx->Queue);
			ctx->Queue = nullptr;
		}
		SetEvent(hEvEmptyQueue_OnlyHolds);

		std::deque<TIMER_CTX*>::iterator pos = timersDelayList.begin();
		while (pos != timersDelayList.end())
		{
			if (*pos == ctx)
			{
				timersDelayList.erase(pos);
				break;
			}
			pos++;
		}
	}
	ReleaseSemaphore(hWaitLockEvents, 1, NULL);
	CloseThreadpoolTimer(ctx->Timer);
	delete ctx;
}

