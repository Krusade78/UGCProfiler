#pragma once
#include "../../Profile/CProfile.h"
#include "../../EventQueue/CEventQueue.h"
#include "../../ProcessOutput/CVirtualHID.h"

class CGenerateEvents
{
public:
	enum class Origin : unsigned char
	{
		Button = 0,
		Hat,
		Axis,
		ButtonShort,
		HatShort,
	};

	static void Init(CProfile* pProfile, CEventQueue* pEvQueue);

	static void Mouse(PEV_COMMAND action);
	static void Command(UINT32 joyId, UINT16 actionId, UCHAR origin, Origin originType, PEV_COMMAND axisData);
	static void DirectX(UCHAR vJoyId, UCHAR map, PVHID_INPUT_DATA inputData);
	static void CheckHolds();
private:
	static CProfile* pProfile;
	static CEventQueue* pEvQueue;
};

