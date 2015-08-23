#pragma once
class CMenuLauncher
{
public:
	CMenuLauncher(HINSTANCE hInst, HWND hWnd);
	~CMenuLauncher(void);

	BOOL AddNotificationIcon();
	BOOL DeleteNotificationIcon();
	void MenuProc(HWND hwnd, UINT message, WPARAM wParam, LPARAM lParam);

private:
	static const UINT WMAPP_NOTIFYCALLBACK = WM_APP + 1;
	HINSTANCE g_hInst;
	HWND hWnd;
	HMENU hMenu;
	HMENU mu1;
	HMENU mu2;
	HANDLE bmps[3];

	void CargarPerfilesMenu(HMENU menu1, HMENU menu2);
	void ShowContextMenu(HWND hwnd, POINT pt);
	void Cargar(wchar_t* nombre);
	bool CambiarPedales(bool activar);
};

