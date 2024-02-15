#pragma once
#include "../../EventQueue/CEventPacket.h"

class CX52
{
public:
	static bool Process(CEventPacket* commandList);
};

