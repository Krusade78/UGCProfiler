#pragma once

class DrvAPI
{
public:
	DrvAPI();
	~DrvAPI();

	int Iniciar(HINSTANCE hInstance);
private:
	char		tipo = -1;
	int			resultado = 0;
	HINSTANCE	hinst = NULL;
	HANDLE		mutex = NULL;
	HWND		hWnd = NULL;

	int GetInstalado();
	static DWORD WINAPI Procesar(LPVOID param);
	static INT_PTR CALLBACK DialogProc(HWND hWnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
};