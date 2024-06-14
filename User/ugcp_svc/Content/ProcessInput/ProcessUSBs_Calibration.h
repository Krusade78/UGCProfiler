#pragma once
#include "../Profile/CProfile.h"
#include "../EventQueue/CEventPacket.h"
#include "../ProcessOutput/CVirtualHID.h"
#include "../HIDInput/Hid_Input_Data.h"

class CCalibration
{
public:
	~CCalibration();
	void Calibrate(CProfile* pProfile, UINT32 joyId, PHID_INPUT_DATA pHidData);
private:
	std::unordered_map<UINT32, std::vector<CALIBRATION::ST_LIMITS>> limitsCache;
	std::unordered_map<UINT32, std::vector<CALIBRATION::ST_JITTER>> jittersCache;
};

