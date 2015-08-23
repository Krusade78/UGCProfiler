// DrvAPI.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include <SetupAPI.h>
#include "DrvAPI.h"
#include "resource.h"
#include "Desinstalar.h"
#include "instalar.h"

#pragma comment(linker,"/manifestdependency:\"type='win32' name='Microsoft.Windows.Common-Controls' version='6.0.0.0' processorArchitecture='*' publicKeyToken='6595b64144ccf1df' language='*'\"")

int CALLBACK WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	DrvAPI main;
	return main.Iniciar(hInstance);
}



DrvAPI::DrvAPI() {}
DrvAPI::~DrvAPI() {}


int DrvAPI::Iniciar(HINSTANCE hInstance)
{
	this->hinst = hInstance;
	this->tipo = GetInstalado();

	if (this->tipo == -1)
		return 0;

	this->mutex = CreateEvent(NULL,FALSE,TRUE,NULL);
	this->hWnd = CreateDialog(hInstance, MAKEINTRESOURCE(IDD_FORMVIEW), NULL, DialogProc);
	SetWindowLongPtr(this->hWnd, GWLP_USERDATA, (LONG_PTR)this);
	ShowWindow(this->hWnd, SW_SHOW);

	SendMessage(this->hWnd, WM_USER + 1, 0, 0);

	// Main message loop:
	MSG msg;
	while(GetMessage(&msg, NULL, 0, 0) != 0)
	{
		if(msg.message == WM_QUIT) break;
		DispatchMessage(&msg); 
	}

	CloseHandle(mutex);
	return resultado;
}

int DrvAPI::GetInstalado()
{
	CONST GUID guidHid = { 0x745a17a0, 0x74d3, 0x11d0, { 0xb6, 0xfe, 0x00, 0xa0, 0xc9, 0x0f, 0x57, 0xda } };	// Mini driver

	HDEVINFO di = SetupDiGetClassDevs(&guidHid, NULL, NULL, DIGCF_PRESENT);
	if (di == INVALID_HANDLE_VALUE)
	{
		CInstalar::Error("GetInstalado[1]");
		return -1;
	}

	DWORD idx = 0;
	SP_DEVINFO_DATA dev;
		dev.cbSize = sizeof(SP_DEVINFO_DATA);

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
				CInstalar::Error("GetInstalado[2]");
				delete[]desc; desc = NULL;
				SetupDiDestroyDeviceInfoList(di);
				return -1;
			}
			if (_stricmp((char*)desc, "VHID\\XHOTASVirtualMinidriver") == 0 || _stricmp((char*)desc, "HID\\NullVirtualHidDevice") == 0)
			{
				delete[]desc; desc = NULL;
				SetupDiDestroyDeviceInfoList(di);
				return 0;
			}
		}

		delete[]desc; desc = NULL;
	}

	SetupDiDestroyDeviceInfoList(di);
	return 1;
}

INT_PTR CALLBACK DrvAPI::DialogProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	switch (uMsg)
	{
		case (WM_USER + 1) :
		{
			DrvAPI* main = (DrvAPI*)GetWindowLongPtr(hWnd, GWLP_USERDATA);
			if (WaitForSingleObject(main->mutex, 0) != WAIT_OBJECT_0)
				return TRUE;
			CreateThread(0, 0, Procesar, (void*)main, 0, 0);
			return TRUE;
		}
		case WM_DESTROY:
		{
			DrvAPI* main = (DrvAPI*)GetWindowLongPtr(hWnd, GWLP_USERDATA);
			WaitForSingleObject(main->mutex, INFINITE);
			// Perform cleanup tasks. 
			PostQuitMessage(0);
			return FALSE;
		}
	}
	return DefWindowProc(hWnd, uMsg, wParam, lParam);
}

DWORD WINAPI DrvAPI::Procesar(LPVOID param)
{
	DrvAPI* main=(DrvAPI*)param;
	if(main->tipo == 0)
	{
		SendMessage(GetDlgItem(main->hWnd, IDC_LABEL), WM_SETTEXT, 0, (LPARAM)"Desinstalando drivers...");
		UpdateWindow(main->hWnd);
		CDesinstalar unins(main->hWnd, main->hinst);
		if (unins.Iniciar())
			main->resultado = 1;
	}
	else
	{
		SendMessage(GetDlgItem(main->hWnd, IDC_LABEL), WM_SETTEXT, 0, (LPARAM)"Instalando drivers...");
		UpdateWindow(main->hWnd);
		char r = 0;
		CInstalar inst;
		if (inst.InstalarVHID(main->hWnd)) {
			r++;
			if(inst.InstalarJoystickUSB()) r++;
		}
		main->resultado = r;
	}

	SetEvent(main->mutex);
	SendMessage(main->hWnd, WM_CLOSE, 0, 0);
	return 0;
}