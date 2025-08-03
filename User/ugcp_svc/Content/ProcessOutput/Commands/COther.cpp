#include "../../framework.h"
#include "COther.h"
#include "../CProcessOutput.h"

/// <summary>
/// Hold, Delay and Repeats
/// </summary>
/// <returns><para>false: delete command[0] normal</para>
/// <para>true: reprocess/skip action</para>
/// </returns>
bool COther::Process(CProfile* pProfile, std::deque<CEventPacket*>::iterator* posEvent, void* parent)
{
	CEventPacket* commandQueue = **posEvent;
	CProcessOutput* local = static_cast<CProcessOutput*>(parent);
	PEV_COMMAND command = commandQueue->GetCommandQueue()->front();

	if (command->Type == CommandType::Delay) // Delay
	{
		CProcessOutput::TIMER_CTX* ctx = new CProcessOutput::TIMER_CTX{};
		PTP_TIMER timerHandle = CreateThreadpoolTimer(EvtDelay, ctx, NULL);
		if (timerHandle != NULL)
		{
			LARGE_INTEGER t{};
			t.QuadPart = (-1000000LL * command->Basic.Data1); //10 * 1000 * 100
			FILETIME timeout{};
			timeout.dwHighDateTime = t.HighPart;
			timeout.dwLowDateTime = t.LowPart;

			delete command;
			commandQueue->GetCommandQueue()->pop_front();

			ctx->Timer = timerHandle;
			ctx->Parent = local;
			ctx->Queue = commandQueue;
			local->GetTimersDelayList()->push_back(ctx); //inside eventMutex

			SetThreadpoolTimer(timerHandle, &timeout, 0, 0);

			*posEvent = local->GetEventQueue()->erase(*posEvent);
		}
		else
		{
			delete ctx;
			return false;
		}
	}
	else if (command->Type == CommandType::Hold) // Autorepeat hold
	{
		if (!IsHoldOn(pProfile, command))
		{
			delete command;
			commandQueue->GetCommandQueue()->pop_front();
		}
		else
		{
			(*posEvent)++; //go to next action
		}
	}
	else if (command->Type == CommandType::Repeat)
	{
		if (!IsHoldOn(pProfile, command)) // end infinite autorepeat
		{
			DeleteRepeatBlock(commandQueue, CommandType::Repeat);
		}
		else
		{
			CopyQueueWithRepeat(commandQueue, CommandType::Repeat);
		}
	}
	else if (command->Type == CommandType::RepeatN)
	{
		if (command->Basic.Data1 == 0)
		{
			DeleteRepeatBlock(commandQueue, CommandType::RepeatN);
		}
		else
		{
			command->Basic.Data1--;
			CopyQueueWithRepeat(commandQueue, CommandType::RepeatN);
		}
	}

	return true;
}

void CALLBACK COther::EvtDelay(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CProcessOutput::TIMER_CTX* ctx = static_cast<CProcessOutput::TIMER_CTX*>(Context);
		ctx->Parent->ProcessDelay(ctx);
	}
}

bool COther::IsHoldOn(CProfile* pProfile, PEV_COMMAND command)
{
	bool pressed = false;

	pProfile->LockStatus();
	{
		
		if ((command->Extended.Origin & 128) == 128)  //axis or hat
		{
			if (command->Extended.Mode == 255) //hat
			{
				UCHAR hatPressed;
				if (pProfile->GetStatus()->Hats.GetPressed(&hatPressed, command->Extended.InputJoy, command->Extended.Origin & 127))
				{
					pressed = hatPressed == 1;
				}
			}
			else //axis
			{
				UCHAR mode = pProfile->GetStatus()->Mode;
				UCHAR submode = pProfile->GetStatus()->SubMode;
				UCHAR cmode = mode | static_cast<UCHAR>(submode << 4);
				STATUS::ST_AXIS* stAxis;
				if (pProfile->GetStatus()->Axes.GetStatus(&stAxis, command->Extended.InputJoy, mode | static_cast<UCHAR>(submode << 4), command->Extended.Origin & 127))
				{
					pressed = !((command->Extended.Mode == mode) && (command->Extended.Submode == submode) && ((command->Extended.Incremental != stAxis->IncrementalPos) || (command->Extended.Band != stAxis->Band)));
				}
			}
		}
		else //button
		{
			UCHAR btPressed;
			if (pProfile->GetStatus()->Buttons.GetPressed(&btPressed, command->Extended.InputJoy, command->Extended.Origin))
			{
				pressed = btPressed == 1;
			}
		}
	}
	pProfile->UnlockStatus();

	return pressed;
}

void COther::DeleteRepeatBlock(CEventPacket* queue, UCHAR commandType)
{
	std::deque<PEV_COMMAND>::iterator pos = queue->GetCommandQueue()->begin();
	pos++;;
	UCHAR nested = (commandType == CommandType::RepeatN) ? 1 : 0;;
	while (pos < queue->GetCommandQueue()->end())
	{
		PEV_COMMAND end = *pos;
		if ((end->Type & 0x7f) == CommandType::RepeatN)
		{
			if ((end->Type & CommandType::Release) == CommandType::Release)
				nested--;
			else
				nested++;
		}
		if (((end->Type & 0x7f) == commandType) && ((end->Type & CommandType::Release) == CommandType::Release) && (nested == 0))
		{
			delete end;
			queue->GetCommandQueue()->erase(pos);
			break;
		}
		pos++;
	}

	delete queue->GetCommandQueue()->front();
	queue->GetCommandQueue()->pop_front();
}

void COther::CopyQueueWithRepeat(CEventPacket* queue, UCHAR commandType)
{
	std::deque<PEV_COMMAND> auxQueue;
	std::deque<PEV_COMMAND>::iterator posCopy = queue->GetCommandQueue()->begin();
	posCopy++;
	UCHAR nested = (commandType == CommandType::RepeatN) ? 1 : 0;

	while (posCopy != queue->GetCommandQueue()->end())
	{
		PEV_COMMAND comOrigin = *posCopy;
		if ((comOrigin->Type & 0x7f) == CommandType::RepeatN)
		{
			if ((comOrigin->Type & CommandType::Release) == CommandType::Release)
				nested--;
			else
				nested++;
		}
		if (((comOrigin->Type & 0x7f) == commandType) && ((comOrigin->Type & CommandType::Release) == CommandType::Release) && (nested == 0))
		{
			break;
		}
		else
		{
			PEV_COMMAND mem = new EV_COMMAND;
			RtlCopyMemory(mem, comOrigin, sizeof(EV_COMMAND));
			auxQueue.push_back(mem);
		}
		posCopy++;
	}

	while (!auxQueue.empty())
	{
		queue->GetCommandQueue()->push_front(auxQueue.back());
		auxQueue.pop_back();
	}
}

