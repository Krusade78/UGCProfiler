#pragma once
typedef struct
{
	std::uint16_t  Axes[15]; //X,Y,Z,Rx,Ry,Rz,Sl1,Sl2,Sl3,X,Y,Z,Rx,Ry,Rz
	std::uint8_t	Hats[4];
	std::uint8_t	Buttons[16];
} VHID_INPUT_DATA, *PVHID_INPUT_DATA;

class CVirtualHID
{
private:
	typedef struct
	{
		struct
		{
			std::uint8_t Buttons;
			char X;
			char Y;
			char Wheel;
		} Mouse;
		std::uint8_t Keyboard[29];
		VHID_INPUT_DATA	DirectX[3];
	} ST_STATUS;
public:
	CVirtualHID();
	~CVirtualHID();

	bool Init();
	ST_STATUS* GetStatus() { return &status; }
	void SendRequestToJoystick(std::uint8_t joyId);
private:
	bool initialized = false;
	bool available[16];

	ST_STATUS status{};

	std::uint32_t Hat2Switch(std::uint8_t pos);
};

