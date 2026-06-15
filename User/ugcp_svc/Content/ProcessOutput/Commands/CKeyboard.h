#pragma once
#include "../../EventQueue/CEventPacket.h"

class CKeyboard
{
public:
	static void Processed(PEV_COMMAND pCommand);
private:
	static UINT GetExtended(UCHAR key);
};

