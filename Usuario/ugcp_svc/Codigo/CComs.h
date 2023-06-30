#pragma once
#include "Perfil/CPerfil.h"

class CComs
{
public:
	CComs(CPerfil* pPerfil);
	~CComs();

	bool Iniciar();
	void SetHwnd(HWND hWnd) { hWndMensajes = hWnd; };
private:
	enum class TipoMsj : BYTE { ModoRaw, ModoCalibrado, Calibrado, Antiv, Mapa, Comandos};

	CPerfil* pPerfil = nullptr;
	HANDLE hPipe = nullptr;
	HWND hWndMensajes = nullptr;
	bool salir = false;
	short hiloCerrado = TRUE;

	static DWORD WINAPI HiloLectura(LPVOID param);
	bool ProcesarMensaje(BYTE* msj, DWORD tam);
};

