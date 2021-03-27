#pragma once
#include <setupapi.h>
#include "Perfil/CPerfil.h"
#include "ColaEventos/CColaEventos.h"
#include "ColaEntrada/CColaHID.h"
#include "ProcesarEntrada/CPreprocesar.h"

#define HARDWARE_ID_PEDALES  L"\\\\?\\HID#VID_06A3&PID_0763"
#define HARDWARE_ID_X52 L"\\\\?\\HID#VID_06A3&PID_0255"
#define HARDWARE_ID_NXT L"\\\\?\\HID#VID_231d&PID_0200"

class CEntradaHID
{
public:
	CEntradaHID(CPerfil* perfil, CColaEventos* colaEv);
	~CEntradaHID();

	bool Iniciar(HINSTANCE hInst);
	HWND GetHwnd() { return pnpHWnd; }
	void LoopWnd();

private:
	CPreprocesar* procesarHID = nullptr;
	wchar_t* rutaPedales = nullptr;
	wchar_t* rutaX52 = nullptr;
	wchar_t* rutaNXT = nullptr;
	PVOID hdevPedales = NULL;
	PVOID hdevX52 = NULL;
	PVOID hdevNXT = NULL;

	HWND pnpHWnd = NULL;
	HDEVNOTIFY pnpHdn = NULL;

	short hiloCerrado[3] = { TRUE, TRUE, TRUE };
	bool salir = false;

	bool GetRutasConectados();
	bool CompararHardwareId(wchar_t* path, const wchar_t* hardwareId);
	bool PnpNotification(HINSTANCE hInst);
	static LRESULT CALLBACK PnpMsjProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

	bool AbrirDevices();
	void CerrarDevices();
	static DWORD WINAPI HiloLectura(LPVOID param);
};
