// Instalar.cpp: Instalación del X36 Driver
//


#include "stdafx.h"
#include <setupapi.h>
#include "newdev.h"
#include "instalar.h"
#include "desinstalar.h"

CInstalar::CInstalar()//,bool manual)
{
}

CInstalar::~CInstalar(void){}

// Instalación

bool CInstalar::InstalarVHID(HWND hwnd)
{
	if(!VHID(hwnd)) {
		CDesinstalar des(NULL,NULL);
		des.VHID();
		return false;
	}

	return true;
}
bool CInstalar::InstalarJoystickUSB()
{
	if(!JoystickUSB()) {
		CDesinstalar des(NULL,NULL);
		des.JoystickUSB();
		des.VHID();
		return false;
	}

	return true;
}

bool CInstalar::VHID(HWND hwnd)
{
	GUID guidHid={0x745a17a0,0x74d3,0x11d0,{0xb6,0xfe,0x00,0xa0,0xc9,0x0f,0x57,0xdb}};
	const char hardwareId[]="USB\\Vid_0100&Pid_0001\0";
	HDEVINFO di=SetupDiCreateDeviceInfoList(&guidHid,NULL);
	if(di==INVALID_HANDLE_VALUE)
	{
		Error("VHID[0]");
		return false;
	}

	SP_DEVINFO_DATA dev;
	dev.cbSize=sizeof(SP_DEVINFO_DATA);
	if (!SetupDiCreateDeviceInfo(di, "XHOTAS_VHID", &guidHid, NULL, NULL, DICD_GENERATE_ID, &dev))
	{
		Error("VHID[1]");
		SetupDiDestroyDeviceInfoList(di);
		return false;
	}
	if (!SetupDiSetDeviceRegistryProperty(di, &dev, SPDRP_HARDWAREID, (BYTE*)hardwareId, 22)) {
		Error("VHID[2]");
		SetupDiDestroyDeviceInfoList(di);
		return false;
	}

	if (!SetupDiCallClassInstaller(DIF_REGISTERDEVICE,di,&dev))
    {
		Error("VHID[4]");
		SetupDiDestroyDeviceInfoList(di);
		return false;
	}

	BOOL boot;
	if(!UpdateDriverForPlugAndPlayDevices(NULL,hardwareId,"xhotas_vhid.inf",INSTALLFLAG_FORCE,&boot))
	{
		Error("VHID[5]");
		SetupDiDestroyDeviceInfoList(di);
		return false;
	}

	SetupDiDestroyDeviceInfoList(di);

	return true;
}

bool CInstalar::JoystickUSB()
{
	const char hardwareId6[]="USB\\Vid_06a3&Pid_0255\0";
	BOOL boot;

	if(!UpdateDriverForPlugAndPlayDevices(NULL,hardwareId6,".\\xhotas_usb.inf",INSTALLFLAG_FORCE, &boot)) {
		Error("JoystickUSB[0]");
		return false;
	}

	return true;
}

// Errores

void CInstalar::Error(char* tit)
{
	LPCTSTR msg;
	FormatMessage( 
	    FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		GetLastError(),
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR) &msg,
		0, NULL );
	MessageBox(NULL,msg,tit,MB_ICONERROR);
}
void CInstalar::Error(char* tit, HRESULT res)
{
	LPCTSTR msg;
	FormatMessage( 
	    FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		res,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR) &msg,
		0, NULL );
	MessageBox(NULL,msg,tit,MB_ICONERROR);
}
