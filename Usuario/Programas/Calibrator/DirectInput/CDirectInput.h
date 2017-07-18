#pragma once
#include "dinput.h"

class CDirectInput
{
public:
	CDirectInput(void);
	~CDirectInput(void);

	LPDIRECTINPUT8  g_pDI;
	LPDIRECTINPUTDEVICE8 g_pJoystick; 

	BYTE GetTipo(HINSTANCE hInst);
	bool Abrir(HWND hwnd, HINSTANCE hInst);
	bool GetEstado(BYTE* joystick );
	bool Calibrar(BOOLEAN esX52);
private:
	HWND hWnd;
	static int CALLBACK DIEnumDevicesCallback( LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef );
	static int CALLBACK DIEnumTiposCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef);
	void Cerrar();
};
