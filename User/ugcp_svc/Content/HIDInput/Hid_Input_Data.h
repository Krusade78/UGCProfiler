#pragma once

typedef struct
{
	std::uint16_t Axis[24];
	std::uint8_t Hats[4];
	std::uint64_t Buttons[2];
} HID_INPUT_DATA, * PHID_INPUT_DATA;
