#include "StdAfx.h"
#include "CDirectInput.h"

CDirectInput::CDirectInput()
{
	g_pDI = NULL;
	g_pJoystick = NULL;
}

CDirectInput::~CDirectInput(void)
{
	this->Cerrar();
}

BYTE CDirectInput::GetTipo(HINSTANCE hInst)
{
	HRESULT         hr;

	//hr = CoInitializeEx(NULL,COINIT_MULTITHREADED);
	//if(FAILED(hr)) return 0;

	const GUID IID_IDI= {0xBF798030,0x483A,0x4DA2,{0xAA,0x99,0x5D,0x64,0xED,0x36,0x97,0x00}};
	hr = DirectInput8Create(hInst,DIRECTINPUT_VERSION,IID_IDI,(LPVOID*)&g_pDI,NULL);
	if(FAILED(hr)) {
		return 0;
	}
	BYTE tipo=0;
	hr =g_pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, DIEnumTiposCallback, &tipo, DIEDFL_ATTACHEDONLY);
	if(FAILED(hr)) return 0;

	g_pDI->Release();
	g_pDI=NULL;
	//CoUninitialize();

	return tipo;
}
int CALLBACK CDirectInput::DIEnumTiposCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef)
{
	BYTE* tipo = (BYTE*)pvRef;
	if(strcmp(lpddi->tszProductName,"Saitek X36+X35 gameport")==0) {
		*tipo = 2;
		return DIENUM_STOP;
	} else if(strcmp(lpddi->tszProductName,"Saitek X36 gameport")==0) {
		*tipo = 1;
		return DIENUM_STOP;
	} else if(strcmp(lpddi->tszProductName,"Saitek X36 USB")==0) {
		*tipo = 3;
		return DIENUM_STOP;
	} else if(strcmp(lpddi->tszProductName,"Controlador virtual XHOTAS")==0) {//Saitek X52 Flight Control System")==0) {
		*tipo = 5;
		return DIENUM_STOP;
	} else if(strcmp(lpddi->tszProductName,"Saitek X45")==0) {
		*tipo = 4;
		return DIENUM_STOP;
	//} else if(strcmp(lpddi->tszProductName,"Saitek X52")==0) {
	//	*tipo = 5;
	//	return DIENUM_STOP;
	} else {
		return DIENUM_CONTINUE;
	}
}

bool CDirectInput::Abrir(HWND hwnd, HINSTANCE hInst)
{
	HRESULT         hr;

	//hr = CoInitializeEx(NULL,COINIT_MULTITHREADED);
	//if(FAILED(hr)) return false;

	const GUID IID_IDI= {0xBF798030,0x483A,0x4DA2,{0xAA,0x99,0x5D,0x64,0xED,0x36,0x97,0x00}};
	hr = DirectInput8Create(hInst,DIRECTINPUT_VERSION,IID_IDI,(LPVOID*)&g_pDI,NULL);
	if(FAILED(hr)) {
		return false;
	}

	hr =g_pDI->EnumDevices(DI8DEVCLASS_GAMECTRL, DIEnumDevicesCallback, this, DIEDFL_ATTACHEDONLY);
	if(FAILED(hr)) return false;
    if( NULL == g_pJoystick ) return false;
	if (FAILED(g_pJoystick->SetDataFormat(&c_dfDIJoystick2))) return false;
	DIPROPDWORD pw;
	pw.diph.dwHeaderSize=sizeof(DIPROPHEADER);
	pw.diph.dwSize=sizeof(DIPROPDWORD);
	pw.diph.dwObj=0;
	pw.diph.dwHow=DIPH_DEVICE;
	pw.dwData=DIPROPCALIBRATIONMODE_RAW;
	if( FAILED(g_pJoystick->SetProperty(DIPROP_CALIBRATIONMODE,&pw.diph ))) return false;
	if(FAILED(g_pJoystick->SetCooperativeLevel(hwnd,DISCL_NONEXCLUSIVE | DISCL_BACKGROUND))) return false;
	return true;
}

int CALLBACK CDirectInput::DIEnumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef)
{
	CDirectInput* di = (CDirectInput*)pvRef;
	if(strcmp(lpddi->tszProductName,"Saitek X36+X35 gameport")==0 || strcmp(lpddi->tszProductName,"Saitek X36 gameport")==0 ||
	   strcmp(lpddi->tszProductName,"Saitek X36 USB")==0 || strcmp(lpddi->tszProductName,"Controlador virtual XHOTAS")==0 || strcmp(lpddi->tszProductName,"Saitek X45")==0)// || strcmp(lpddi->tszProductName,"Saitek X52")==0 )
	{
		di->g_pDI->CreateDevice(lpddi->guidInstance,&di->g_pJoystick, NULL);
		return DIENUM_STOP;
	} else {
		return DIENUM_CONTINUE;
	}
}

void CDirectInput::Cerrar()
{
	if(g_pJoystick!=NULL) {
		g_pJoystick->Release();
		g_pJoystick=NULL;
	}
	if(g_pDI!=NULL) {
		g_pDI->Release();
		g_pDI=NULL;
	}
	//CoUninitialize();
}

bool CDirectInput::GetEstado(BYTE *joystick)
{
	if(g_pJoystick==NULL) return false;

	HRESULT hr = g_pJoystick->Poll(); 
	if (FAILED(hr)) 
	{
		hr = g_pJoystick->Acquire();
		if( FAILED(hr) ) return false;

		hr = g_pJoystick->Poll();
		if( FAILED(hr) ) return false;

		DIJOYSTATE2 js; 
		hr = g_pJoystick->GetDeviceState(sizeof(DIJOYSTATE2), &js);
		if (FAILED(hr)) return false; 

		RtlCopyMemory(joystick,&js,sizeof(DIJOYSTATE2));
	} else {
		DIJOYSTATE2 js; 
		hr = g_pJoystick->GetDeviceState(sizeof(DIJOYSTATE2), &js);
		if (FAILED(hr)) return false; 

		RtlCopyMemory(joystick,&js,sizeof(DIJOYSTATE2));
	}


	return true; 
}

//BOOL CDirectInput::DIEnumDeviceObjectsCallback(
//         LPCDIDEVICEOBJECTINSTANCE lpddoi,
//         LPVOID pvRef)
//{
//	return  DIENUM_CONTINUE;
//}

bool CDirectInput::Calibrar(BOOLEAN esX52)
{
	//HRESULT hr = g_pJoystick->EnumObjects(DIEnumDeviceObjectsCallback, NULL, DIDFT_AXIS);

	LONG maxxy=4095,maxr=255;
	if(esX52) { maxxy=2047; maxr=1023; }

	struct {
			DIPROPHEADER diph;
			LONG data[3];
	} dipdw;
	dipdw.diph.dwSize       = sizeof(dipdw); 
	dipdw.diph.dwHeaderSize = sizeof(DIPROPHEADER); 
	dipdw.diph.dwHow        = DIPH_BYID; 
	dipdw.data[0]=0;

	// Eje x
	dipdw.diph.dwObj=2;
	dipdw.data[1]=maxxy/2;
	dipdw.data[2]=maxxy;
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;
	// Eje y
	dipdw.diph.dwObj=0x102;
	dipdw.data[1]=maxxy/2;
	dipdw.data[2]=maxxy;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;

	// Eje z
	dipdw.diph.dwObj=0x802;
	dipdw.data[1]=127;
	dipdw.data[2]=255;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
			return false;
	// Eje r
	dipdw.diph.dwObj=0x502;
	dipdw.data[1]=maxr/2;	
	dipdw.data[2]=maxr;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;

	// Rotatorio 1
	dipdw.diph.dwObj=0x302;
	dipdw.data[1]=127;	
	dipdw.data[2]=255;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;
	// Rotatorio 2
	dipdw.diph.dwObj=0x402;
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;
	// Slider
	dipdw.diph.dwObj=0x202;
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;

	// ministickx
	dipdw.diph.dwObj=0xa02;
	dipdw.data[1]=8;	
	dipdw.data[2]=15;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;
	// ministicky
	dipdw.diph.dwObj=0x902;
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;

	//Aux 1
	dipdw.diph.dwObj=0x702;
	dipdw.data[1]=maxr/2;	
	dipdw.data[2]=maxr;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;
	// Aux 2
	dipdw.diph.dwObj=0x602;
	dipdw.data[1]=127;	
	dipdw.data[2]=255;	
	if( FAILED(g_pJoystick->SetProperty( DIPROP_CALIBRATION, &dipdw.diph ) ) ) 
		return false;
	return true;
}
