#pragma once
#include "../../EventQueue/CEventPacket.h"
#include "../CVirtualHID.h"

class CKeyboard
{
public:
	static void Processed(PEV_COMMAND pCommand, CVirtualHID* pVHid);
private:
	static UINT GetExtended(UCHAR key);
};

