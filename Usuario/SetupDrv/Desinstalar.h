#pragma once

class CDesinstalar
{
public:
	CDesinstalar(HWND hWnd,HINSTANCE hInst);
	~CDesinstalar(void);
	bool Iniciar();
	bool VHID();
	bool JoystickUSB();
	bool Base();
private:
	HWND hWnd;
	HINSTANCE hInst;
	void Error(char* tit);
};
