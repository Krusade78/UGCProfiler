#include "../framework.h"
#include "CEventPacket.h"

CEventPacket::CEventPacket()
{
	queue = new std::deque<PEV_COMMAND>();
}

CEventPacket::~CEventPacket()
{
	if (queue != nullptr)
	{
		while (!queue->empty())
		{
			delete queue->front();
			queue->pop_front();
		}
	}
	delete queue; queue = nullptr;
}

void CEventPacket::AddCommand(PEV_COMMAND command)
{
	queue->push_back(command);
}
std::deque<PEV_COMMAND>* CEventPacket::GetCommandQueue()
{
	return queue;
}

