#include "../../framework.h"
#include "CNXT.h"
#include "../../NXT/HIDNXTWrite.h"

/// <summary>
/// Gladiator NXT commands
/// </summary>
/// <returns><para>TRUE: processed and continue</para><para>FALSE: not processed</para></returns>
bool CNXT::Process(CEventPacket* queue)
{
	bool processed = true;
	PEV_COMMAND command = queue->GetCommandQueue()->front();

	if (command->Type.Get() == CommandType::NxtLeds)
	{
		UCHAR params[4]{};
		params[0] = command->Basic.Data;

		std::deque<PEV_COMMAND>::iterator pos = queue->GetCommandQueue()->begin();
		params[1] = (*(++pos))->Basic.Data;
		delete (*pos);
		queue->GetCommandQueue()->erase(pos);

		pos = queue->GetCommandQueue()->begin();
		params[2] = (*(++pos))->Basic.Data;
		delete (*pos);
		queue->GetCommandQueue()->erase(pos);

		pos = queue->GetCommandQueue()->begin();
		params[3] = (*(++pos))->Basic.Data;
		delete (*pos);
		queue->GetCommandQueue()->erase(pos);

		if (CNXTWrite::Get() != nullptr) CNXTWrite::Get()->SetLed(params);
	}
	else
	{
		processed = false;
	}

	return processed;
}