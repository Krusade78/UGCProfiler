// xhotas_svc.cpp : Define el punto de entrada de la aplicación.
//

#include "Codigo/framework.h"
#include "xhotas_svc.h"
#include "Codigo/Perfil/CPerfil.h"
#include "Codigo/CComs.h"
#include "Codigo/ColaEventos/CColaEventos.h"
#include "Codigo/CSalidaHID.h"
#include "Codigo/CEntradaHID.h"
#include <Dbt.h>

int APIENTRY wWinMain(_In_ HINSTANCE hInstance,
					 _In_opt_ HINSTANCE hPrevInstance,
					 _In_ LPWSTR    lpCmdLine,
					 _In_ int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(lpCmdLine);
	UNREFERENCED_PARAMETER(nCmdShow);

	HANDLE mtx = CreateSemaphore(NULL, 0, 1, L"mtx_xhotas_svc");
	if ((GetLastError() == ERROR_ALREADY_EXISTS) || (mtx == NULL))
	{
		if (mtx != NULL)
		{
			ReleaseSemaphore(mtx, 1, NULL);
			CloseHandle(mtx);
		}
		return 1;
	}

	//Iniciar objetos
	CPerfil* perfil = new CPerfil();
	CComs* coms = new CComs(perfil);
	if (coms->Iniciar())
	{
		CColaEventos* colaEv = new CColaEventos();
		perfil->RegistarNotificacion(colaEv, true);
		CSalidaHID* salida = new CSalidaHID(perfil, colaEv);
		if (salida->Iniciar())
		{
			perfil->RegistarNotificacion(salida, false);
			CEntradaHID* entrada = new CEntradaHID(perfil, colaEv);
			if (entrada->Iniciar(hInstance))
			{
				coms->SetHwnd(entrada->GetHwnd());
				entrada->LoopWnd();
			}
			delete entrada;
		}
		//delete coms;
		delete salida;
		perfil->RegistarNotificacion(nullptr, false);
		delete colaEv;
		perfil->RegistarNotificacion(nullptr, true);
	}
	else
	{
		delete coms;
	}
	delete perfil;

	ReleaseSemaphore(mtx, 1, NULL);
	CloseHandle(mtx);

	return 0;
}
