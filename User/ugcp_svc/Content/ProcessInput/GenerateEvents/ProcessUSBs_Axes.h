#pragma once
#include "../../Profile/CProfile.h"
#include "../../EventQueue/CEventPacket.h"
#include "../../ProcessOutput/CVirtualHID.h"
#include "../../HIDInput/Hid_Input_Data.h"

class CAxes
{
public:
	static void SensibilityAndMapping(CProfile* pProfile, UINT32 joyId, PHID_INPUT_DATA pOld, PHID_INPUT_DATA pInput);

	static void MoveAxis(CProfile* pProfile, UINT32 joyId, UCHAR idx, UINT16 _new);

private:
	static void Axis2Mouse(UCHAR axis, CHAR mov);
	static UCHAR TranslateRotary(CProfile* pProfile, UINT32 joyId, UCHAR axis, UINT16 _new, UCHAR mode);
};
