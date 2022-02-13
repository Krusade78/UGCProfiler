//#include "../framework.h"
//#include "CDirectInput.h"
//#include "../ColaEntrada/CPaqueteHID.h"
//
////#pragma comment(lib,"dinput8.lib")
////#pragma comment(lib,"dxguid.lib")
////
//CDirectInput::~CDirectInput()
//{
//	if (g_pJoystick != nullptr) {
//		g_pJoystick->Release();
//		g_pJoystick = nullptr;
//	}
//
//	if (g_pDI != nullptr) {
//		g_pDI->Release();
//		g_pDI = nullptr;
//	}
//}
//
//bool CDirectInput::Calibrar(/*HWND hwnd*/UCHAR joy)
//{
//	const GUID IID_IDI= {0xBF798030,0x483A,0x4DA2,{0xAA,0x99,0x5D,0x64,0xED,0x36,0x97,0x00}};
//	if (FAILED(DirectInput8Create(GetModuleHandle(NULL), DIRECTINPUT_VERSION, IID_IDI, (LPVOID*)&g_pDI, NULL)))
//	{
//		return false;
//	}
//
//	switch (static_cast<TipoPaquete>(joy))
//	{
//		case TipoPaquete::Pedales:
//			guidJoy = { 0x00000000, 0x0000, 0x0000, {0x00, 0x00, 0x50, 0x49, 0x44, 0x56, 0x49, 0x44} };
//			break;
//		case TipoPaquete::X52:
//			guidJoy = { 0x00000000, 0x0000, 0x0000, {0x00, 0x00, 0x50, 0x49, 0x44, 0x56, 0x49, 0x44} };
//			break;
//		case TipoPaquete::NXT:
//			guidJoy = { 0x00000000, 0x0000, 0x0000, {0x00, 0x00, 0x50, 0x49, 0x44, 0x56, 0x49, 0x44} };
//			break;
//	}
//
//	if (FAILED(g_pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, DIEnumDevicesCallback, this, DIEDFL_ATTACHEDONLY))) { return false; }
//	if (nullptr != g_pJoystick)
//	{
//		if (SUCCEEDED(g_pJoystick->SetDataFormat(&c_dfDIJoystick)))
//		{
//			DIPROPDWORD pw
//			{
//				pw.diph.dwHeaderSize = sizeof(DIPROPHEADER),
//				pw.diph.dwSize = sizeof(DIPROPDWORD),
//				pw.diph.dwObj = 0,
//				pw.diph.dwHow = DIPH_DEVICE,
//				pw.dwData = DIPROPCALIBRATIONMODE_RAW
//			};
//			if (SUCCEEDED(g_pJoystick->SetProperty(DIPROP_CALIBRATIONMODE, &pw.diph)))
//			{
//				//g_pJoystick[i]->SetCooperativeLevel(hwnd, DISCL_NONEXCLUSIVE | DISCL_BACKGROUND);
//				switch (static_cast<TipoPaquete>(joy))
//				{
//				case TipoPaquete::Pedales:
//					return CalibrarPedales();
//				case TipoPaquete::X52:
//					return CalibrarX52();
//				case TipoPaquete::NXT:
//					return CalibrarNXT();
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
//	if (IsEqualGUID(lpddi->guidProduct, di->guidJoy))
//	{
//		if (di->g_pJoystick == nullptr)
//		{
//			if (FAILED(di->g_pDI->CreateDevice(lpddi->guidInstance, &di->g_pJoystick, NULL)))
//			{
//				return DIENUM_STOP;
//			}
//		}
//	}
//
//	return DIENUM_CONTINUE;
//}
//
//bool CDirectInput::CalibrarNXT()
//{
//	return true;
//}
//
//bool CDirectInput::CalibrarPedales()
//{
//	return true;
//}
//
//bool CDirectInput::CalibrarX52()
//{
//	if (g_pJoystick == nullptr)
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
//	dipdw.data[0] = 0;
//	dipdw.data[1] = 1023;
//	dipdw.data[2] = 2047;
//
//	// Eje x
//	dipdw.diph.dwObj=0;
//
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//	// Eje y
//	dipdw.diph.dwObj=4;
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Eje z
//	dipdw.diph.dwObj=8;
//	dipdw.data[1] = 127;
//	dipdw.data[2] = 255;
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//			return false;
//	// Eje rx
//	dipdw.diph.dwObj=12;
//	dipdw.data[1] = 511;
//	dipdw.data[2] = 1023;
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Eje ry
//	dipdw.diph.dwObj=16;
//	dipdw.data[1] = 127;
//	dipdw.data[2] = 255;
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Eje rz
//	dipdw.diph.dwObj=20;
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Slider1
//	dipdw.data[0] = 0;
//	dipdw.data[1] = 7;
//	dipdw.data[2] = 15;
//	dipdw.diph.dwObj=24;
//	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
//		return false;
//
//	// Slider2
//	dipdw.diph.dwObj = 28;
//	if (FAILED(g_pJoystick->SetProperty(DIPROP_CALIBRATION, &dipdw.diph)))
//		return false;
//
//	return true;
//}
