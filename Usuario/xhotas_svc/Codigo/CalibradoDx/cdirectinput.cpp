#include "../framework.h"
#include "CDirectInput.h"

//#pragma comment(lib,"dinput8.lib")
//#pragma comment(lib,"dxguid.lib")
//
//CDirectInput::~CDirectInput()
//{
//	for (char i = 0; i < 3; i++)
//	{
//		if (g_pJoystick[i] != nullptr) {
//			g_pJoystick[i]->Release();
//			g_pJoystick[i] = nullptr;
//		}
//	}
//	if (g_pDI != nullptr) {
//		g_pDI->Release();
//		g_pDI = nullptr;
//	}
//}
//
//bool CDirectInput::Calibrar(/*HWND hwnd, */HINSTANCE hInst)
//{
//	const GUID IID_IDI= {0xBF798030,0x483A,0x4DA2,{0xAA,0x99,0x5D,0x64,0xED,0x36,0x97,0x00}};
//	if (FAILED(DirectInput8Create(hInst, DIRECTINPUT_VERSION, IID_IDI, (LPVOID*)&g_pDI, NULL)))
//	{
//		return false;
//	}
//
//	if (FAILED(g_pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, DIEnumDevicesCallback, this, DIEDFL_ATTACHEDONLY))) { return false; }
//	for (char i = 0; i < 3; i++)
//	{
//		if (nullptr != g_pJoystick[i])
//		{
//			if (SUCCEEDED(g_pJoystick[i]->SetDataFormat(&c_dfDIJoystick)))
//			{
//				DIPROPDWORD pw
//				{
//					pw.diph.dwHeaderSize = sizeof(DIPROPHEADER),
//					pw.diph.dwSize = sizeof(DIPROPDWORD),
//					pw.diph.dwObj = 0,
//					pw.diph.dwHow = DIPH_DEVICE,
//					pw.dwData = DIPROPCALIBRATIONMODE_RAW
//				};
//				if (SUCCEEDED(g_pJoystick[i]->SetProperty(DIPROP_CALIBRATIONMODE, &pw.diph)))
//				{
//					//g_pJoystick[i]->SetCooperativeLevel(hwnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
//					return CalibrarDx(i);
//				}
//			}
//		}
//	}
//	return false;
//}
//
//int CALLBACK CDirectInput::DIEnumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef) noexcept
//{
//	CDirectInput* di = static_cast<CDirectInput*>(pvRef);
//
//	const GUID guid = { 0x00000000, 0x0000, 0x0000, {0x00, 0x00, 0x50, 0x49, 0x44, 0x56, 0x49, 0x44} };
//	if (IsEqualGUID(lpddi->guidProduct, guid))
//	{
//		for (char i = 0; i < 3; i++)
//		{
//			if (di->g_pJoystick[i] == nullptr)
//			{
//				if (FAILED(di->g_pDI->CreateDevice(lpddi->guidInstance, &di->g_pJoystick[i], NULL)))
//				{
//					return DIENUM_STOP;
//				}
//				break;
//			}
//		}
//	}
//
//	return DIENUM_CONTINUE;
//}
//
//
//bool CDirectInput::CalibrarDx(char i)
//{
//	if (g_pJoystick[i] == nullptr)
//	{
//		return false;
//	}
//
//	struct {
//			DIPROPHEADER diph;
//			LONG data[3];
//	} dipdw;
//	RtlZeroMemory(&dipdw, sizeof(dipdw));
//	dipdw.diph.dwSize       = sizeof(dipdw); 
//	dipdw.diph.dwHeaderSize = sizeof(DIPROPHEADER); 
//	dipdw.diph.dwHow        = DIPH_BYOFFSET;
//
//	dipdw.data[0] = -32767;
//	dipdw.data[1] = 0;
//	dipdw.data[2] = 32767;
//
//	// Eje x
//	dipdw.diph.dwObj=0;
//
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//	// Eje y
//	dipdw.diph.dwObj=4;
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Eje z
//	dipdw.diph.dwObj=8;
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//			return false;
//	// Eje rx
//	dipdw.diph.dwObj=12;
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Eje ry
//	dipdw.diph.dwObj=16;
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Eje rz
//	dipdw.diph.dwObj=20;
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Slider1
//	dipdw.data[0] = -127;
//	dipdw.data[1] = 0;
//	dipdw.data[2] = 127;
//	dipdw.diph.dwObj=24;
//	if( FAILED(g_pJoystick[i]->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Slider2
//	dipdw.data[0] = -127;
//	dipdw.data[1] = 0;
//	dipdw.data[2] = 127;
//	dipdw.diph.dwObj = 28;
//	if (FAILED(g_pJoystick[i]->SetProperty(DIPROP_CALIBRATION, &dipdw.diph)))
//		return false;
//
//	return true;
//}
