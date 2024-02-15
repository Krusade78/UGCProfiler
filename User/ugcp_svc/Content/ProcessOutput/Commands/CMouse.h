#pragma once
#include "../../EventQueue/CEventPacket.h"
#include "../CVirtualHID.h"

class CMouse
{
public:
	static bool Process(CVirtualHID* pVHid, PEV_COMMAND command, bool* setTimer);
private:
	static bool SendOutput(CVirtualHID* pVHid, CommandType cmd, bool axisX, bool axisY);
};

