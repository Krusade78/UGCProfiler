#include "StdAfx.h"
#include "MenuLauncher.h"
#include "resource.h"
#include "Traduce.h"
#include "../comsbn/comsbn.h"
#include <winioctl.h>
#include <shellapi.h>
#include <strsafe.h>

#pragma comment(lib, "comsbn.lib")

CMenuLauncher::CMenuLauncher(HINSTANCE hInst, HWND hWnd)
{
	this->g_hInst = hInst;
	this->hWnd = hWnd;
	this->hMenu = LoadMenu(hInst, MAKEINTRESOURCE(IDC_LAUNCHER));
	HMENU hSubMenu = GetSubMenu(this->hMenu, 0);
	MENUITEMINFO mi;
	ZeroMemory(&mi, sizeof(mi));
		mi.cbSize=sizeof(MENUITEMINFO);
		mi.fMask=MIIM_STRING;
		mi.dwTypeData = CTraduce::Txt(L"load_profile");
	SetMenuItemInfo(hSubMenu, 0, TRUE, &mi);
		mi.dwTypeData = CTraduce::Txt(L"new_profile");
	SetMenuItemInfo(hSubMenu, 2, TRUE, &mi);
		mi.dwTypeData = CTraduce::Txt(L"edit_profile");
	SetMenuItemInfo(hSubMenu, 3, TRUE, &mi);
		mi.dwTypeData = CTraduce::Txt(L"calibrator");
	SetMenuItemInfo(hSubMenu, 5, TRUE, &mi);
		mi.dwTypeData = L"Pedales";
	SetMenuItemInfo(hSubMenu, 7, TRUE, &mi);
		mi.dwTypeData = CTraduce::Txt(L"exit");
	SetMenuItemInfo(hSubMenu, 9, TRUE, &mi);
	this->mu1 = NULL;
	this->mu2 = NULL;

	bmps[0] = LoadImage(hInst, MAKEINTRESOURCE(IDB_BITMAP1), 0, 0, 0, LR_CREATEDIBSECTION);
	bmps[1] = LoadImage(hInst, MAKEINTRESOURCE(IDB_BITMAP2), 0, 0, 0, LR_CREATEDIBSECTION);
	bmps[2] = LoadImage(hInst, MAKEINTRESOURCE(IDB_BITMAP3), 0, 0, 0, LR_CREATEDIBSECTION);
}

CMenuLauncher::~CMenuLauncher(void)
{
	if(hMenu != NULL) {DestroyMenu(hMenu); hMenu = NULL;}
	if(mu1 != NULL) {DestroyMenu(mu1); mu1 = NULL;}
	if(mu2 != NULL) {DestroyMenu(hMenu); mu2 = NULL;}

	DeleteObject(bmps[0]);
	DeleteObject(bmps[1]);
	DeleteObject(bmps[2]);
}

#pragma region "Icono"
BOOL CMenuLauncher::AddNotificationIcon()
{
    NOTIFYICONDATA nid = {sizeof(nid)};
    nid.hWnd = hWnd;
    nid.uFlags = NIF_ICON | NIF_TIP | NIF_MESSAGE | NIF_SHOWTIP;
	nid.uID = 1;
    nid.uCallbackMessage = WMAPP_NOTIFYCALLBACK;
    nid.hIcon = LoadIcon(g_hInst, MAKEINTRESOURCE(IDI_LAUNCHER));
    StringCchCopy(nid.szTip,128,L"XLauncher");
    Shell_NotifyIcon(NIM_ADD, &nid);

    // NOTIFYICON_VERSION_4 is prefered
    nid.uVersion = NOTIFYICON_VERSION_4;
    return Shell_NotifyIcon(NIM_SETVERSION, &nid);
}

BOOL CMenuLauncher::DeleteNotificationIcon()
{
    NOTIFYICONDATA nid = {sizeof(nid)};
	nid.hWnd = hWnd;
    nid.uID = 1;
    return Shell_NotifyIcon(NIM_DELETE, &nid);
}
#pragma endregion

void CMenuLauncher::CargarPerfilesMenu(HMENU menu1, HMENU menu2)
{
	UINT_PTR id = 0x64;
	WIN32_FIND_DATA FindFileData;
	HANDLE hFind = FindFirstFile(L".\\*.xhp", &FindFileData);
	if (hFind != INVALID_HANDLE_VALUE) 
	{
		
		FindFileData.cFileName[wcslen(FindFileData.cFileName) -4] = L'\0';
		AppendMenu(menu1, MF_STRING, id, FindFileData.cFileName);
		AppendMenu(menu2, MF_STRING, id << 16, FindFileData.cFileName);
		id++;
		while (FindNextFile(hFind, &FindFileData) != 0)
		{
			FindFileData.cFileName[wcslen(FindFileData.cFileName) -4] = L'\0';
			AppendMenu(menu1, MF_STRING, id, FindFileData.cFileName);
			AppendMenu(menu2, MF_STRING, id << 16, FindFileData.cFileName);
			id++;
		}
		FindClose(hFind);
	}
}

void CMenuLauncher::ShowContextMenu(HWND hwnd, POINT pt)
{
    if (hMenu)
    {	
		if(mu1 != NULL) DestroyMenu(mu1);
		mu1 = CreatePopupMenu();
		if(mu2 != NULL) DestroyMenu(mu2);
		mu2 = CreatePopupMenu();
		CargarPerfilesMenu(mu1, mu2);

        HMENU hSubMenu = GetSubMenu(hMenu, 0);
        if (hSubMenu)
        {
			MENUITEMINFO mi;
				ZeroMemory(&mi,sizeof(mi));
				mi.cbSize=sizeof(MENUITEMINFO);
				mi.fMask=MIIM_SUBMENU|MIIM_BITMAP;

				mi.hbmpItem = (HBITMAP)bmps[2];
				mi.hSubMenu = mu1;
			SetMenuItemInfo(hSubMenu, 0, TRUE, &mi);
				mi.hbmpItem = (HBITMAP)bmps[1];
				mi.hSubMenu = mu2;
			SetMenuItemInfo(hSubMenu, 3, TRUE, &mi);

				mi.fMask=MIIM_BITMAP;
				mi.hbmpItem = (HBITMAP)bmps[0];
			SetMenuItemInfo(hSubMenu, 2, TRUE, &mi);

				mi.hbmpItem = HBMMENU_MBAR_CLOSE;
			SetMenuItemInfo(hSubMenu, 9, TRUE, &mi);
			DrawMenuBar(hwnd);

			// our window must be foreground before calling TrackPopupMenu or the menu will not disappear when the user clicks away
            SetForegroundWindow(hwnd);

            // respect menu drop alignment
            UINT uFlags = TPM_RIGHTBUTTON;
            if (GetSystemMetrics(SM_MENUDROPALIGNMENT) != 0)
            {
                uFlags |= TPM_RIGHTALIGN;
            }
            else
            {
                uFlags |= TPM_LEFTALIGN;
            }

            TrackPopupMenuEx(hSubMenu, uFlags, pt.x, pt.y, hwnd, NULL);
        }
    }
}

void CMenuLauncher::MenuProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam)
{
    switch (message)
    {
		case WM_COMMAND:
			switch(wParam)
			{
			case IDM_NUEVO:
				ShellExecute(NULL, L"open", L"Editor.exe", NULL, NULL, SW_SHOWNORMAL);
				break;
			case IDM_CALIBRATOR:
				ShellExecute(NULL, L"open", L"calibrator.exe", NULL, NULL, SW_SHOWNORMAL);
				break;
			case IDM_BYPASSPEDALES:
				{
					HMENU hSubMenu = GetSubMenu(hMenu, 0);						
					MENUITEMINFO mi;
						ZeroMemory(&mi, sizeof(MENUITEMINFO));
						mi.cbSize = sizeof(MENUITEMINFO);
						mi.fMask = MIIM_STATE;
					HMENU smenu = GetSubMenu(hMenu, 0);
					GetMenuItemInfo(smenu, 7, TRUE, &mi);
					if((mi.fState & MFS_CHECKED) == MFS_CHECKED)
					{
						if(CambiarPedales(false)) {}
							mi.fState -= MFS_CHECKED;
					}
					else
					{
						if(CambiarPedales(true)) {}
							mi.fState |= MFS_CHECKED;
					}
					SetMenuItemInfo(smenu, 7, TRUE, &mi);
				}
				break;
			case IDM_CARGAR:
			case IDM_EDITAR:
			case IDM_SALIR:
				break;
			default:
				if(hMenu != NULL)
				{
					MENUITEMINFO mi;
						ZeroMemory(&mi, sizeof(MENUITEMINFO));
						mi.cbSize = sizeof(MENUITEMINFO);
						mi.fMask = MIIM_SUBMENU;
					HMENU smenu = GetSubMenu(hMenu, 0);

					GetMenuItemInfo(smenu, ((wParam >> 16) > 0) ? 3 : 0, TRUE, &mi);
					smenu = mi.hSubMenu;
					if(smenu != NULL)
					{
						mi.hSubMenu = 0;
						mi.fMask = MIIM_STRING;
						if(GetMenuItemInfo(smenu, (UINT)wParam, FALSE, &mi))
						{
							wchar_t* nombre = new wchar_t[mi.cch + 7]; // +1 +4 del '.xhp' + 2 comillas
							nombre[0] = L'"';
							mi.dwTypeData = &nombre[1];
							mi.cch++;
							if(GetMenuItemInfo(smenu, (UINT)wParam, FALSE, &mi))
							{
								wcscat(nombre, L".xhp");
								if((wParam >> 16) == 0)
									Cargar(&nombre[1]);
								else
								{
									wcscat(nombre, L"\"");
									ShellExecute(NULL, L"open", L"Editor.exe", nombre, NULL,SW_SHOWNORMAL);
								}
							}
							delete[] nombre; nombre = NULL;
						}
					}
				}
			}
			break;
		case WMAPP_NOTIFYCALLBACK:
			switch (LOWORD(lParam))
			{
			case WM_RBUTTONUP:
			case NIN_SELECT:
			case WM_CONTEXTMENU:
				{
					POINT const pt = { LOWORD(wParam), HIWORD(wParam) };
					ShowContextMenu(hwnd, pt);
				}
				break;
			}
			break;
		default:
			break;
    }
}

void CMenuLauncher::Cargar(wchar_t* nombre)
{
	switch(CargarMapa(nombre))
	{
		case 1:
			CTraduce::Msg(L"error_open_xhp", MB_ICONEXCLAMATION);
			break;
		case 2:
			CTraduce::Msg(L"error_reading_file", MB_ICONEXCLAMATION);
			break;
		case 3:
			CTraduce::Msg(L"error_opening_device", MB_ICONEXCLAMATION);
			break;
		case 4:
			CTraduce::Msg(L"error_accessing_device", MB_ICONEXCLAMATION);
			break;
		default:
			CTraduce::Msg(L"load_ok", MB_ICONINFORMATION);
	}
}

#define IOCTL_USR_CONPEDALES	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_READ_ACCESS)
#define IOCTL_USR_SINPEDALES	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_READ_ACCESS)
bool CMenuLauncher::CambiarPedales(bool activar)
{
	DWORD ret;

	HANDLE driver=CreateFile(
			L"\\\\.\\XHOTASHidInterface",
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			0,
			NULL);
	if(driver==INVALID_HANDLE_VALUE)
	{
		MessageBox(NULL,L"Error opening device", L"[X52-CambiarPedales][1]", MB_ICONWARNING);
		return false;
	}
	if(activar)
	{
		if(!DeviceIoControl(driver, IOCTL_USR_CONPEDALES, NULL, 0, NULL, 0, &ret, NULL))
		{
			CloseHandle(driver);
			MessageBox(NULL,L"Error accesing device", L"[CambiarPedales][2]", MB_ICONWARNING);
			return false;
		}
	}
	else
	{
		if(!DeviceIoControl(driver, IOCTL_USR_SINPEDALES, NULL, 0, NULL, 0, &ret, NULL))
		{
			CloseHandle(driver);
			MessageBox(NULL,L"Error accesing device", L"[CambiarPedales][3]", MB_ICONWARNING);
			return false;
		}
	}

	CloseHandle(driver);
	return true;
}