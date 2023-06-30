#pragma once
#include "Perfil/CPerfil.h"
#include "ColaEventos/CColaEventos.h"
#include "ProcesarSalida/CVirtualHID.h"
#include "ProcesarSalida/CProcesarSalida.h"

class CSalidaHID
{
public:
	CSalidaHID(CPerfil* perfil, CColaEventos* colaEv, CVirtualHID* vhid);
	~CSalidaHID();

	bool Iniciar();
private:
	HANDLE evSalir = nullptr;
	bool salir = false;
	short hiloCerrado = TRUE;

	CPerfil* perfil = nullptr;
	CColaEventos* colaEv = nullptr;
	CVirtualHID* vhid = nullptr;
	CProcesarSalida* salida = nullptr;

	static DWORD WINAPI HiloLectura(LPVOID param);
};

