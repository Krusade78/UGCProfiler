#include "../../framework.h"
#include <deque>
#include "CGenerateEvents.h"

CProfile* CGenerateEvents::pProfile = nullptr;
CEventQueue* CGenerateEvents::pEvQueue = nullptr;

void CGenerateEvents::Init(CProfile* pProfile, CEventQueue* pEvQueue)
{
	CGenerateEvents::pProfile = pProfile;
	CGenerateEvents::pEvQueue = pEvQueue;
}

void CGenerateEvents::Mouse(PEV_COMMAND pev_command)
{
	CEventPacket* pEvento = new CEventPacket();
	PEV_COMMAND command = new EV_COMMAND;
	RtlCopyMemory(command, pev_command, sizeof(EV_COMMAND));
	pEvento->AddCommand(command);
	pEvQueue->Add(pEvento);
}

void CGenerateEvents::Command(UINT32 idJoy, UINT16 actionId, UCHAR origin, Origin originType, PEV_COMMAND pAxisData)
{
	if ((originType == Origin::Hat) || (originType == Origin::HatShort))
	{
		origin += 64;
	}
	else if (originType == Origin::Axis)
	{
		origin += 128;
	}

	if (actionId != 0)
	{
		CEventPacket* pEvent = new CEventPacket();
		pProfile->BeginProfileRead();
		{
			if (pProfile->GetProfile()->Actions->empty())
			{
				delete pEvent;
				pProfile->EndProfileRead();
				return;
			}
			else
			{
				CEventPacket*& commands = pProfile->GetProfile()->Actions->at(actionId - 1);
				std::deque<PEV_COMMAND>::iterator idx = commands->GetCommandQueue()->begin();
				while(idx != commands->GetCommandQueue()->end())
				{
					PEV_COMMAND pEvt = new EV_COMMAND;
					RtlCopyMemory(pEvt, *idx, sizeof(EV_COMMAND));
					if ((pEvt->Type == CommandType::Hold) && ((originType == Origin::ButtonShort) || (originType == Origin::HatShort)))
					{
						pEvt->Type == CommandType::Delay;
						pEvt->Basic.Data1 = 2;
					}
					if ((pEvt->Type == CommandType::Hold) || ((pEvt->Type & 0x7f) == CommandType::Repeat))
					{
						pEvt->Extended.Origin = origin;
						pEvt->Extended.InputJoy = idJoy;
						if (originType == Origin::Axis)
						{
							pEvt->Extended.Submode = pAxisData->Extended.Submode;
							pEvt->Extended.Mode = pAxisData->Extended.Mode;
							pEvt->Extended.Incremental = pAxisData->Extended.Incremental;
							pEvt->Extended.Band = pAxisData->Extended.Band;
						}
					}
					pEvent->AddCommand(pEvt);
					idx++;
				}
			}
		}
		pProfile->EndProfileRead();

		pEvQueue->Add(pEvent);
	}
}

void CGenerateEvents::DirectX(UCHAR vJoyId, UCHAR map, PVHID_INPUT_DATA inputData)
{
	CEventPacket* pEvent = new CEventPacket();
	PEV_COMMAND command = new EV_COMMAND;
	RtlZeroMemory(command, sizeof(EV_COMMAND));
	command->Type = CommandType::Reserved_DxPosition;
	command->VHid.OutputJoyId = vJoyId;
	command->VHid.Map = map;
	RtlCopyMemory(&command->VHid.Data, inputData, sizeof(VHID_INPUT_DATA));

	pEvent->AddCommand(command);
	pEvQueue->Add(pEvent);
}

void CGenerateEvents::CheckHolds()
{
	CEventPacket* pEvent = new CEventPacket();
	PEV_COMMAND command = new EV_COMMAND;
	RtlZeroMemory(command, sizeof(EV_COMMAND));
	command->Type = CommandType::Reserved_CheckHold;
	pEvent->AddCommand(command);
	pEvQueue->Add(pEvent);
}
