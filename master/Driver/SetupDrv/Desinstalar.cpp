// Desinstalar.cpp: Instalación del X36 Driver
//

#include "stdafx.h"
#include <setupapi.h>
#include <newdev.h>
#include "desinstalar.h"
//#include "newdev.h"


CDesinstalar::CDesinstalar(HWND hWnd,HINSTANCE hInst)
{
	this->hWnd = hWnd;
	this->hInst = hInst;
}

CDesinstalar::~CDesinstalar(void){}

bool CDesinstalar::Iniciar()
{
	if(!JoystickUSB()) return false;
	if(!VHID()) return false;

	return true;
}

bool CDesinstalar::VHID()
{
	//Borrar del Pnp
	{
		CONST GUID guidHid={0x745a17a0,0x74d3,0x11d0,{0xb6,0xfe,0x00,0xa0,0xc9,0x0f,0x57,0xda}};
		SP_DEVINFO_DATA dev;
			dev.cbSize = sizeof(SP_DEVINFO_DATA);
		DWORD idx=0;
		
		// Mini driver
		HDEVINFO di=SetupDiGetClassDevs(&guidHid,NULL,NULL,DIGCF_PRESENT);
		if(di==INVALID_HANDLE_VALUE)
		{
			Error("Un-VHID[0]");
			return false;
		}

		while (SetupDiEnumDeviceInfo(di, idx, &dev))
		{
			idx++;
			DWORD tam = 0;
			BYTE* desc = NULL;
			SetupDiGetDeviceRegistryProperty(di, &dev, SPDRP_HARDWAREID, NULL, NULL, 0, &tam);
			if (tam != 0) {
				desc = new BYTE[tam];
				if (!SetupDiGetDeviceRegistryProperty(di, &dev, SPDRP_HARDWAREID, NULL, desc, tam, NULL))
				{
					Error("Un-VHID[1]");
					delete[]desc; desc = NULL;
					SetupDiDestroyDeviceInfoList(di);
					return false;
				}
				if (_stricmp((char*)desc, "VHID\\XHOTASVirtualHID") == 0)
				{
					if (!Borrar(di, &dev))
					{
						delete[]desc; desc = NULL;
						SetupDiDestroyDeviceInfoList(di);
						return false;
					}
				}
			}

			delete[]desc; desc = NULL;
		}

		SetupDiDestroyDeviceInfoList(di);
	}

	return true;
}

bool CDesinstalar::JoystickUSB()
{
	//Borrar del Pnp
	GUID guidHid={0x745a17a0,0x74d3,0x11d0,{0xb6,0xfe,0x00,0xa0,0xc9,0x0f,0x57,0xda}};
	SP_DEVINFO_DATA dev;
		dev.cbSize = sizeof(SP_DEVINFO_DATA);
	DWORD idx = 0;
	HDEVINFO di=SetupDiGetClassDevs(&guidHid,NULL,NULL,0);//DIGCF_PRESENT);
	if(di==INVALID_HANDLE_VALUE)
	{
		Error("Un-JoystickUSB[0]");
		return false;
	}
			
	while (SetupDiEnumDeviceInfo(di, idx, &dev))
	{
		idx++;
		DWORD tam = 0;
		BYTE* desc = NULL;
		SetupDiGetDeviceRegistryProperty(di, &dev, SPDRP_HARDWAREID, NULL, NULL, 0, &tam);
		if (tam != 0) {
			desc = new BYTE[tam];
			if (!SetupDiGetDeviceRegistryProperty(di, &dev, SPDRP_HARDWAREID, NULL, desc, tam, NULL))
			{
				Error("Un-JoystickUSB[1]");
				delete[]desc; desc = NULL;
				SetupDiDestroyDeviceInfoList(di);
				return false;
			}
			if (tam >= 21) desc[21] = 0;
			if ((_stricmp((char*)desc, "USB\\Vid_06a3&Pid_0255") == 0)/* || (_stricmp((char*)desc, "HID\\Vid_06a3&Pid_0255") == 0)*/)
			{
				if (!Borrar(di, &dev))
				{
					delete[]desc; desc = NULL;
					SetupDiDestroyDeviceInfoList(di);
					return false;
				}
				//Sleep(2000);UpdateDriverForPlugAndPlayDevices(NULL,(char*)desc,"input.inf",0,NULL);
			}
		}

		delete[]desc; desc = NULL;
	}

	SetupDiDestroyDeviceInfoList(di);

	return true;
}

bool CDesinstalar::Borrar(HDEVINFO di, SP_DEVINFO_DATA* dev)
{
	DWORD tam = 0;

	if (!SetupDiBuildDriverInfoList(di, dev, SPDIT_COMPATDRIVER))
	{
		Error("Un-Inf[1]");
		return false;
	}

	SP_DRVINFO_DATA drvid;
	drvid.cbSize = sizeof(SP_DRVINFO_DATA);
	if (!SetupDiEnumDriverInfo(di, dev, SPDIT_COMPATDRIVER, 0, &drvid))
	{
		Error("Un-Inf[2]");
		return false;
	}

	SetupDiGetDriverInfoDetail(di, dev, &drvid, NULL, 0, &tam);

	BYTE* info = new BYTE[tam];
	SP_DRVINFO_DETAIL_DATA* drvdid = (SP_DRVINFO_DETAIL_DATA*)info;
	drvdid->cbSize = sizeof(SP_DRVINFO_DETAIL_DATA);
	if (SetupDiGetDriverInfoDetail(di, dev, &drvid, drvdid, tam, &tam))
	{
		if (!DiUninstallDevice(NULL, di, dev, 0, NULL))
		{
			Error("Un-Inf[3]");
			delete[] info;
			return false;
		}

		char cmdline[65];
		RtlZeroMemory(cmdline, 65);
		GetSystemDirectory(cmdline, 20);
		strncat_s(cmdline, "\\pnputil.exe /delete-driver ", 29);
		strncat_s(cmdline, &drvdid->InfFileName[15], strlen(&drvdid->InfFileName[15]) - 15);
		strncat_s(cmdline, " /force", 7);

		STARTUPINFO si;
		PROCESS_INFORMATION pi;
		RtlZeroMemory(&si, sizeof(si));
		si.cb = sizeof(si);
		RtlZeroMemory(&pi, sizeof(pi));
		if (CreateProcess(NULL, cmdline, NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi))
		{
			WaitForSingleObject(pi.hProcess, INFINITE);
			CloseHandle(pi.hProcess);
			CloseHandle(pi.hThread);
		}
		else
		{
			Error("Un-Inf[4]");
			delete[] info;
			return false;
		}
	}
	delete[] info;

	return true;
}

void CDesinstalar::Error(char* tit)
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