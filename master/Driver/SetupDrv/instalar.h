#pragma once

class CInstalar
{
public:
	CInstalar();
	~CInstalar(void);
	bool InstalarVHID(HWND hwnd);
	bool InstalarJoystickUSB();

	static void Error(char* tit);
	static void Error(char* tit, HRESULT res);
private:
	bool VHID(HWND hwnd);
	bool JoystickUSB();
};
