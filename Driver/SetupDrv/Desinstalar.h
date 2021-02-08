#pragma once

class CDesinstalar
{
public:
	CDesinstalar(HWND hWnd,HINSTANCE hInst);
	~CDesinstalar(void);
	bool Iniciar();
	bool VHID();
	bool JoystickUSB();
private:
	HWND hWnd;
	HINSTANCE hInst;
	bool Borrar(HDEVINFO di, SP_DEVINFO_DATA* dev);
	void Error(char* tit);
};
