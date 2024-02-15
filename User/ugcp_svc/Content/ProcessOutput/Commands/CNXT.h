#pragma once
#include "../../EventQueue/CEventPacket.h"

class CNXT
{
public:
	static bool Process(CEventPacket* commandList);
};

