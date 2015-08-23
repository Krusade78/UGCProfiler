// DirectInput.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include <winioctl.h>
#include "cdirectinput.h"
#include "DirectInput.h"

#ifdef AMD64
	#pragma comment(lib,"dinput8x64.lib")
	#pragma comment(lib,"dxguidx64.lib")
#else
	#pragma comment(lib,"dinput8x86.lib")
	#pragma comment(lib,"dxguidx86.lib")
#endif

HINSTANCE hInst=NULL;

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	hInst = hModule;
    return TRUE;
}

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



#pragma region "LLamadas al driver gameport"

#define IOCTL_X36USR_CALIBRADO		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_X36USR_CALIBRAR		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_READ_ACCESS)
#define IOCTL_X36USR_DESCALIBRAR	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_READ_ACCESS)
typedef	struct _CALIBRADOX36 {
	ULONG i;
	ULONG c;
	ULONG d;
	UCHAR n;
	UCHAR Margen;
	UCHAR Resistencia;
	BOOLEAN cal;
	BOOLEAN antiv;
} CALIBRADOX36;
void ComprobarDatosX36(CALIBRADOX36* datosEje)
{
	if(datosEje->cal) {
		if( (datosEje->d<=(datosEje->c+datosEje->n)) || (datosEje->i>=(datosEje->c-datosEje->n)) ) datosEje->cal=FALSE;
	}
}

__declspec(dllexport) char DescalibrarGameport()
{
	DWORD ret;
	HANDLE driver=CreateFile(
			"\\\\.\\X36KbWrapperInterface",
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE) return 1;
	if(!DeviceIoControl(driver,IOCTL_X36USR_DESCALIBRAR,NULL,0,NULL,0,&ret,NULL)) {
		CloseHandle(driver);
		return 2;
	}

	CloseHandle(driver);
	return 0;
}

__declspec(dllexport) char CalibrarGameport()
{
	DWORD ret;
	HANDLE driver=CreateFile(
			"\\\\.\\X36KbWrapperInterface",
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE) return 1;
	if(!DeviceIoControl(driver,IOCTL_X36USR_CALIBRAR,NULL,0,NULL,0,&ret,NULL)) {
		CloseHandle(driver);
		return 2;
	}

	CloseHandle(driver);
	return 0;
}

void LeerRegistroGameport(CALIBRADOX36* datosEje)
{
	HKEY key;
	DWORD tipo,tam=sizeof(CALIBRADOX36);
	LONG res;

	if (ERROR_SUCCESS != RegOpenKeyEx(HKEY_CURRENT_USER, "SOFTWARE\\XHOTAS\\Calibrado", 0, KEY_READ, &key))
		return;

	res=RegQueryValueEx(key,"GameportEje1",0,&tipo,(BYTE*)&datosEje[0],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOX36)) RtlZeroMemory(&datosEje[0],sizeof(CALIBRADOX36));
	ComprobarDatosX36(&datosEje[0]);
	res=RegQueryValueEx(key,"GameportEje2",0,&tipo,(BYTE*)&datosEje[1],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOX36)) RtlZeroMemory(&datosEje[1],sizeof(CALIBRADOX36));
	ComprobarDatosX36(&datosEje[1]);
	res=RegQueryValueEx(key,"GameportEje3",0,&tipo,(BYTE*)&datosEje[2],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOX36)) RtlZeroMemory(&datosEje[2],sizeof(CALIBRADOX36));
	ComprobarDatosX36(&datosEje[2]);
	res=RegQueryValueEx(key,"GameportEje4",0,&tipo,(BYTE*)&datosEje[3],&tam);
	if(ERROR_SUCCESS!=res || tam!=sizeof(CALIBRADOX36)) RtlZeroMemory(&datosEje[3],sizeof(CALIBRADOX36));
	ComprobarDatosX36(&datosEje[3]);

	RegCloseKey(key);
}
__declspec(dllexport) char EscribirCalibradoGameport()
{
	CALIBRADOX36 ejes[4];
	RtlZeroMemory(ejes,sizeof(CALIBRADOX36)*4);
	LeerRegistroGameport(ejes);
	
	DWORD ret;

	HANDLE driver=CreateFile(
			"\\\\.\\X36KbWrapperInterface",
			GENERIC_WRITE,
			FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE) {
		return 1;
	}
	if(!DeviceIoControl(driver,IOCTL_X36USR_CALIBRADO,ejes,sizeof(CALIBRADOX36)*4,NULL,0,&ret,NULL)) {
		CloseHandle(driver);
		return 2;
	}

	CloseHandle(driver);
	return 0;
}
#pragma endregion

#pragma region "LLamadas al driver HID"

#define IOCTL_USR_CALIBRADO		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_CALIBRAR		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_READ_ACCESS)
#define IOCTL_USR_DESCALIBRAR	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_READ_ACCESS)
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
__declspec(dllexport) char DescalibrarHID()
{
	DWORD ret;
	HANDLE driver=CreateFile(
			"\\\\.\\XHOTASHidInterface",
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE) return 1;
	if(!DeviceIoControl(driver,IOCTL_USR_DESCALIBRAR,NULL,0,NULL,0,&ret,NULL)) {
		CloseHandle(driver);
		return 2;
	}

	CloseHandle(driver);
	return 0;
}

__declspec(dllexport) char CalibrarHID()
{
	DWORD ret;
	HANDLE driver=CreateFile(
			"\\\\.\\XHOTASHidInterface",
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE) return 1;
	if(!DeviceIoControl(driver,IOCTL_USR_CALIBRAR,NULL,0,NULL,0,&ret,NULL)) {
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
			"\\\\.\\XHOTASHidInterface",
			GENERIC_WRITE,
			FILE_SHARE_WRITE,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE) {
		return 1;
	}
	if(!DeviceIoControl(driver,IOCTL_USR_CALIBRADO,ejes,sizeof(CALIBRADOHID)*4,NULL,0,&ret,NULL))
		return 2;

	CloseHandle(driver);

	return 0;
}

#pragma endregion

__declspec(dllexport) void EnviarConfiguracionX52()
{
	HANDLE h = OpenEvent(EVENT_MODIFY_STATE, FALSE, "Global\\eXHOTASCargar");
	if(h != NULL) {
		SetEvent(h);
		CloseHandle(h);
	}
	h = OpenEvent(EVENT_MODIFY_STATE, FALSE, "Global\\eXHOTASTimer");
	if(h != NULL) {
		SetEvent(h);
		CloseHandle(h);
	}
}