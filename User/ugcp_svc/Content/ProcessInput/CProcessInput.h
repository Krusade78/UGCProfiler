#pragma once
#include "../Profile/CProfile.h"
#include "../HIDInput/Hid_Input_Data.h"

class CProcessInput
{
public:
	CProcessInput(CProfile* profile);

	void Process(UINT32 joyId, PHID_INPUT_DATA p_hidData);
	//UCHAR ConvertirSeta(UCHAR pos);
private:
	CProfile* pProfile = nullptr;

	std::unordered_map<UINT32, HID_INPUT_DATA> lastStatus;

	void GetOldHidData(UINT32 joyId, PHID_INPUT_DATA data);
	void SetOldHidData(UINT32 joyId, PHID_INPUT_DATA data);
};


