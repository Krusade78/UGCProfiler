#pragma once
#include "Perfil/CPerfil.h"
#include "ColaEventos/CColaEventos.h"
#include "ProcesarEntrada/CPreprocesar.h"
#include "Pedales/CWinUSBPedales.h"
#include "X52/CWinUSBX52.h"
#include "NXT/LeerHIDNXT.h"



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

	HWND pnpHWnd = NULL;
	HDEVNOTIFY pnpHdn = NULL;

	short hiloCerrado[3] = { TRUE, TRUE, TRUE };
	bool salir = false;

	CPedalesEntrada* pedales = nullptr;
	CX52Entrada* x52 = nullptr;
	CNXTEntrada* nxt = nullptr;

	bool PnpNotification(HINSTANCE hInst);
	static LRESULT CALLBACK PnpMsjProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

	static DWORD WINAPI HiloLectura(LPVOID param);
};
