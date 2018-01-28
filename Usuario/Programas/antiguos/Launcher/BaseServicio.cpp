#include "StdAfx.h"
#include "BaseServicio.h"
#include "servicio.h"

HANDLE CBaseServicio::hPower = NULL;

CBaseServicio::CBaseServicio()
{
	CBaseServicio::hPower = CreateEvent(NULL, FALSE, FALSE, L"Global\\eXHOTASResume");
	if(GetLastError() == ERROR_ALREADY_EXISTS) { CloseHandle(CBaseServicio::hPower); CBaseServicio::hPower = NULL;}
	hSalir = CreateEvent(NULL, FALSE, FALSE, NULL);
	CreateThread(NULL, 0, Hilo, this, 0, NULL);
}

CBaseServicio::~CBaseServicio()
{
	Cerrar();
}

void CBaseServicio::Cerrar()
{
	if(CBaseServicio::hPower != NULL) { CloseHandle(CBaseServicio::hPower); CBaseServicio::hPower = NULL;}
	if(hSalir != NULL)
	{
		SetEvent(hSalir);
		while(hSalir != NULL) Sleep(150);
	}
}

void CBaseServicio::ResumePower()
{
	if(CBaseServicio::hPower != NULL) SetEvent(CBaseServicio::hPower);
}

DWORD WINAPI CBaseServicio::Hilo(LPVOID lpParaMeter)
{
	CBaseServicio* base = (CBaseServicio*)lpParaMeter;
	CServicio serv;
	serv.IniciarServicio();

	bool operar = false;
	HANDLE hSemaforo = CreateSemaphore(NULL, 1, 1, L"Global\\eXHOTASRef"); //Para que no se mezclen órdenes de distintas instancias
	HANDLE hTimer = CreateEvent(NULL, FALSE, FALSE, L"Global\\eXHOTASTimer");
	HANDLE hHora = CreateEvent(NULL, TRUE, TRUE, L"Global\\eXHOTASHora");
	HANDLE hFecha = CreateEvent(NULL, TRUE, TRUE, L"Global\\eXHOTASFecha");
	HANDLE hConfig = CreateEvent(NULL, FALSE, FALSE, L"Global\\eXHOTASCargar");

	while(true)
	{
		if(!operar)
			if( WaitForSingleObject(hSemaforo, 1) == WAIT_OBJECT_0)
				operar = true;

		if( WaitForSingleObject(base->hSalir, 1) == WAIT_OBJECT_0 )
			break;
		if( operar && (WaitForSingleObject(CBaseServicio::hPower, 1) == WAIT_OBJECT_0) )
			serv.ClearX52();
		if( WaitForSingleObject(hHora, 1) == WAIT_OBJECT_0 )
			serv.horaActiva =	true;
		else
			serv.horaActiva = false;
		if( WaitForSingleObject(hFecha, 1) == WAIT_OBJECT_0 )
			serv.fechaActiva = true;
		else
			serv.fechaActiva = false;
		if( operar && (WaitForSingleObject(hConfig, 1) == WAIT_OBJECT_0) )
			serv.CargarConfiguracion();
		if( WaitForSingleObject(hTimer,2000) == WAIT_TIMEOUT )
			if(operar)
				serv.Tick();
	}

	if(operar) ReleaseSemaphore(hSemaforo, 1, NULL);
	CloseHandle(hSemaforo);
	CloseHandle(hTimer);
	CloseHandle(hHora);
	CloseHandle(hFecha);
	CloseHandle(hConfig);
	CloseHandle(base->hSalir); base->hSalir = NULL;

	return 0;
}
