#pragma once
#include <winusb.h>
#include "../HIDInput/CHIDDevices.h"

//constexpr auto HARDWARE_ID_X52 = L"\\\\?\\USB#VID_06A3&PID_0255";
constexpr UINT32 HARDWARE_ID_X52 = 0x063a0255;

class CWinUSBX52 : public CHIDDevices
{
public:
	CWinUSBX52() : CHIDDevices(HARDWARE_ID_X52) {}

	//IHIDInput overrides
	virtual unsigned short Read(void* buff) override;

	void SetPause(bool onoff);

private:
	GUID guidInterface = { 0xA57C1168, 0x7717, 0x4AF0, { 0xB3, 0x0E, 0x6A, 0x4C, 0x62, 0x30, 0xBB, 0x10 } };

	WINUSB_INTERFACE_HANDLE hwusb = nullptr;
	WINUSB_PIPE_INFORMATION pipe{};
	char paused = 0;

	//IHIDInput overrides
	virtual bool Prepare() override;
	virtual bool Open() override;
	virtual void Close(bool exit) override;
};

