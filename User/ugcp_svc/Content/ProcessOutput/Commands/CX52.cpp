#include "../../framework.h"
#include "CX52.h"
#include "../../X52/USBX52Write.h"

/// <summary>
/// Comandos del X52
/// </summary>
/// <returns><para>TRUE: procesado y continuar</para><para>FALSE: no procesado</para></returns>
bool CX52::Process(CEventPacket* queue)
{
	bool processed = true;
	PEV_COMMAND command = queue->GetCommandQueue()->front();

	switch (command->Type.Get())
	{
		case CommandType::X52MfdLight:
		{
			UCHAR params = command->Basic.Data;
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_MFD(&params);
			break;
		}
		case CommandType::X52Light:
		{
			UCHAR params = command->Basic.Data;
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_Global(&params);
			break;
		}
		case CommandType::X52InfoLight:
		{
			UCHAR params = command->Basic.Data;
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_Info(&params);
			break;
		}
		case CommandType::X52MfdPinkie:
		{
			UCHAR params = command->Basic.Data;
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Pinkie(&params);
			break;
		}
		case CommandType::X52MfdTextIni:
		{
			UCHAR text[17];
			UCHAR size = 1;
			RtlZeroMemory(text, 17);

			text[0] = command->Basic.Data; //line
			while (queue->GetCommandQueue()->size() != 1)
			{
				std::deque<PEV_COMMAND>::iterator pos = queue->GetCommandQueue()->begin();
				PEV_COMMAND comTxt = *(++pos);
				if (comTxt->Type == CommandType::X52MfdTextEnd)
				{
					delete comTxt;
					queue->GetCommandQueue()->erase(pos);
					break;
				}
				if (size == 17)
				{
					throw new std::exception("Error text buffer");
				}
				text[size++] = comTxt->Basic.Data;
				delete comTxt;
				queue->GetCommandQueue()->erase(pos);
			}
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(text, size);
			break;
		}
		case CommandType::X52MfdHour:
		{
			UCHAR params[3]{};
			params[0] = command->Basic.Data;

			std::deque<PEV_COMMAND>::iterator pos = queue->GetCommandQueue()->begin();
			params[1] = (*(++pos))->Basic.Data;
			delete (*pos);
			queue->GetCommandQueue()->erase(pos);

			pos = queue->GetCommandQueue()->begin();
			params[2] = (*(++pos))->Basic.Data;
			delete (*pos);
			queue->GetCommandQueue()->erase(pos);

			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour(params);
			break;
		}
		case CommandType::X52MfdHour24:
		{
			UCHAR params[3]{};
			params[0] = command->Basic.Data;

			std::deque<PEV_COMMAND>::iterator pos = queue->GetCommandQueue()->begin();
			params[1] = (*(++pos))->Basic.Data;
			delete (*pos);
			queue->GetCommandQueue()->erase(pos);

			pos = queue->GetCommandQueue()->begin();
			params[2] = (*(++pos))->Basic.Data;
			delete (*pos);
			queue->GetCommandQueue()->erase(pos);

			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour24(params);
			break;
		}
		case CommandType::MfdDate:
		{
			UCHAR params[2]{};
			params[0] = command->Basic.Data;

			std::deque<PEV_COMMAND>::iterator pos = queue->GetCommandQueue()->begin();
			params[1] = (*(++pos))->Basic.Data;
			delete (*pos);
			queue->GetCommandQueue()->erase(pos);

			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Date(params);
			break;
		}
		default:
			processed = false;
			break;
	}

	return processed;
}
