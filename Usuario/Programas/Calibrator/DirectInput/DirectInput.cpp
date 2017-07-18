// DirectInput.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include <winioctl.h>
#include "cdirectinput.h"

#pragma comment(lib,"dinput8x64.lib")
#pragma comment(lib,"dxguidx64.lib")

HINSTANCE hInst=NULL;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	hInst = hModule;
    return TRUE;
}

#pragma region "DirectInput"

CDirectInput* di = NULL;
HANDLE salir = NULL;

__declspec(dllexport) BYTE GetTipoDirectInput(HWND hwnd)
{
	CDirectInput dinp;
	return dinp.GetTipo(hInst);
}

__declspec(dllexport) BYTE AbrirDirectInput(HWND hwnd)
{
	di = new CDirectInput();
	if( di->Abrir(hwnd, hInst) ) {
		salir = CreateEvent(NULL,FALSE,FALSE,NULL);
		WaitForSingleObject(salir,INFINITE);
		CloseHandle(salir);
		return true;
	} else {
		delete di; di = NULL;
		return false;
	}
}

__declspec(dllexport) void CerrarDirectInput()
{
	if(di!=NULL) {
		delete di; di = NULL;
		SetEvent(salir);
	}
}

__declspec(dllexport) BYTE PollDirectInput(BYTE* joystick)
{
	if(di!=NULL) {
		return di->GetEstado(joystick);
	} else {
		return false;
	}
}

__declspec(dllexport) BOOLEAN CalibrarDirectInput(BYTE tipo)
{
	if(di!=NULL) {
		return di->Calibrar((BOOLEAN)tipo);
	} else {
		return false;
	}
}
#pragma endregion

#pragma region "LLamadas al driver HID"

#define IOCTL_USR_CALIBRADO	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0109, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_RAW		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0108, METHOD_BUFFERED, FILE_WRITE_ACCESS)
typedef	struct _CALIBRADOHID {
	UINT16 i;
	UINT16 c;
	UINT16 d;
	UCHAR n;
	UCHAR Margen;
	UCHAR Resistencia;
	BOOLEAN cal;
	BOOLEAN antiv;
} CALIBRADOHID;
void ComprobarDatosHID(CALIBRADOHID* datosEje)
{
	if(datosEje->cal) {
		if( (datosEje->d<=(datosEje->c+datosEje->n)) || (datosEje->i>=(datosEje->c-datosEje->n)) ) datosEje->cal=FALSE;
	}
}
__declspec(dllexport) char SetModoRawHID(bool onoff)
{
	DWORD ret;
	HANDLE driver=CreateFile(
			"\\\\.\\XUSBInterface",
			GENERIC_WRITE,
			FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE)
		return 1;
	UCHAR on = (onoff) ? 1 : 0;
	if(!DeviceIoControl(driver, IOCTL_USR_RAW, &on, 1,NULL,0,&ret,NULL))
	{
		CloseHandle(driver);
		return 2;
	}

	CloseHandle(driver);
	return 0;
}

void LeerRegistroHID(CALIBRADOHID* datosEje)
{
	HKEY key;
	DWORD tipo,tam=sizeof(CALIBRADOHID);
	LONG res;

	if(ERROR_SUCCESS!=RegOpenKeyEx(HKEY_CURRENT_USER,"SOFTWARE\\XHOTAS\\Calibrado",0,KEY_READ,&key))
		return;

	res=RegQueryValueEx(key,"Eje1",0,&tipo,(BYTE*)&datosEje[0],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOHID)) RtlZeroMemory(&datosEje[0],sizeof(CALIBRADOHID));
	ComprobarDatosHID(&datosEje[0]);
	res=RegQueryValueEx(key,"Eje2",0,&tipo,(BYTE*)&datosEje[1],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOHID)) RtlZeroMemory(&datosEje[1],sizeof(CALIBRADOHID));
	ComprobarDatosHID(&datosEje[1]);
	res=RegQueryValueEx(key,"Eje3",0,&tipo,(BYTE*)&datosEje[2],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOHID)) RtlZeroMemory(&datosEje[2],sizeof(CALIBRADOHID));
	ComprobarDatosHID(&datosEje[2]);
	res=RegQueryValueEx(key,"Eje4",0,&tipo,(BYTE*)&datosEje[3],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOHID)) RtlZeroMemory(&datosEje[3],sizeof(CALIBRADOHID));
	ComprobarDatosHID(&datosEje[3]);

	RegCloseKey(key);
}
__declspec(dllexport) char EscribirCalibradoHID()
{
	CALIBRADOHID ejes[4];
	RtlZeroMemory(ejes,sizeof(CALIBRADOHID)*4);
	LeerRegistroHID(ejes);
	
	DWORD ret;

	HANDLE driver=CreateFile(
			"\\\\.\\XUSBInterface",
			GENERIC_WRITE,
			FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE)
		return 1;

	if (!DeviceIoControl(driver, IOCTL_USR_CALIBRADO, ejes, sizeof(CALIBRADOHID) * 4, NULL, 0, &ret, NULL))
	{
		CloseHandle(driver);
		return 2;
	}

	CloseHandle(driver);
	return 0;
}

#pragma endregion