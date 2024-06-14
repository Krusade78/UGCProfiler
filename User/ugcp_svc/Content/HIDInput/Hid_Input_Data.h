#pragma once
#include "../framework.h"

typedef struct
{
	UINT16 Axis[24];
	UCHAR Hats[4];
	UINT64 Buttons[2];
} HID_INPUT_DATA, * PHID_INPUT_DATA;
