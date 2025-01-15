#pragma once
#include "../../EventQueue/CEventPacket.h"
#include "../CVirtualHID.h"

class CDirectX
{
public:
	static void Position(PEV_COMMAND pCommand, CVirtualHID* pVHid);
	static void Buttons_Hats(PEV_COMMAND pCommand, CVirtualHID* pVHid);
	static void Axis(PEV_COMMAND pCommand, CVirtualHID* pVHid);
private:

};

