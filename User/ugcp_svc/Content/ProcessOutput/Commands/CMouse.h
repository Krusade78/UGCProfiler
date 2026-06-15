#pragma once
#include "../../EventQueue/CEventPacket.h"
#include "../CVirtualHID.h"

class CMouse
{
public:
	static bool Process(CVirtualHID& pVHid, PEV_COMMAND command, bool* setTimer);

	static std::mutex& GetLock()
	{
		static std::mutex mtx;
		return mtx;
	}

private:
	static bool SendOutput(CVirtualHID& pVHid, CommandType cmd, bool axisX, bool axisY);
};

