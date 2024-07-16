#pragma once
#include "../../Profile/CProfile.h"

class CBotonesSetas
{
public:
	static void PressButton(CProfile* pProfile, UINT32 joyId, UCHAR idx);
	static void ReleaseButton(CProfile* pProfile, UINT32 joyId, UCHAR idx);

	//static void PressHat(CProfile* pProfile, UINT32 joyId, UCHAR idx);
	//static void ReleaseHat(CProfile* pProfile, UINT32 joyId, UCHAR idx);
};


