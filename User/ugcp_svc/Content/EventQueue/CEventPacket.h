#pragma once
#include <deque>
#include "../ProcessOutput/CVirtualHID.h"

class CommandType
{
public:
	static const unsigned char Key = 1;

	static const unsigned char DxButton = 2;
	static const unsigned char DxHat = 3;

	static const unsigned char MouseBt1 = 4;
	static const unsigned char MouseBt2 = 5;
	static const unsigned char MouseBt3 = 6;
	static const unsigned char MouseLeft = 7;
	static const unsigned char MouseRight = 8;
	static const unsigned char MouseUp = 9;
	static const unsigned char MouseDown = 10;
	static const unsigned char MouseWhUp = 11;
	static const unsigned char MouseWhDown = 12;

	static const unsigned char Delay = 20;
	static const unsigned char Hold = 21;
	static const unsigned char Repeat = 22;
	static const unsigned char RepeatN = 23;

	static const unsigned char Mode = 30;
	static const unsigned char Submode = 31;
	static const unsigned char PreciseMode = 32;

	static const unsigned char X52MfdLight = 40;
	static const unsigned char X52Light = 41;
	static const unsigned char X52InfoLight = 42;
	static const unsigned char X52MfdPinkie = 43;
	static const unsigned char X52MfdTextIni = 44;
	static const unsigned char X52MfdText = 45;
	static const unsigned char X52MfdTextEnd = 46;
	static const unsigned char X52MfdHour = 47;
	static const unsigned char X52MfdHour24 = 48;
	static const unsigned char MfdDate = 49;

	static const unsigned char NxtLeds = 50;

	static const unsigned char Reserved_DxPosition = 100;
	static const unsigned char Reserved_CheckHold = 101;
	//Reserved_RepeatIni;

	static const unsigned char Release = 128;

	CommandType& operator=(unsigned char v) { value = v; return *this; }
	bool operator==(unsigned char v) { return value == v; }
	bool operator!=(unsigned char v) { return value != v; }
	unsigned char operator&(unsigned char op2) { return (value & op2); }
	unsigned char Get() const { return value; }
private:
	unsigned char value = 0;
};

#pragma warning (disable: 26495)
typedef struct
{
	CommandType Type;
	union
	{
		struct
		{
			UCHAR Data;
			UCHAR OutputJoy;
		} Basic;
		struct
		{
			UCHAR OnOff;
			UCHAR Axis;
			UINT32 InputJoy;
		} AxisPrecise;
		struct
		{
			UCHAR OutputJoyId;
			UINT16 Map;
			VHID_INPUT_DATA Data;
		} VHid;
		struct
		{
			UCHAR Origin;
			UCHAR Mode;
			UCHAR Submode;
			UCHAR Band;
			UINT16 Incremental;
			UINT32 InputJoy;
		} Extended;
	};
} EV_COMMAND, *PEV_COMMAND;
#pragma warning (default: 26495)

class CEventPacket
{
public:
	CEventPacket();
	~CEventPacket();

	void AddCommand(PEV_COMMAND command);
	std::deque<PEV_COMMAND>* GetCommandQueue();
private:
	std::deque<PEV_COMMAND>* queue = nullptr;
};
