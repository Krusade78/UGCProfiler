// xhotas_svc.cpp : Define el punto de entrada de la aplicación.
//

#include "Codigo/framework.h"
#include "xhotas_svc.h"
#include "Codigo/CalibradoDx/CDirectInput.h"
#include "Codigo/Perfil/CPerfil.h"
#include "Codigo/CComs.h"
#include "Codigo/ColaEventos/CColaEventos.h"
#include "Codigo/CSalidaHID.h"
#include "Codigo/CEntradaHID.h"
#include "Codigo/X52/EscribirUSBX52.h"
#include "Codigo/X52/MenuMFD.h"
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

	//CDirectInput* di = new CDirectInput();
	//if (!di->Calibrar(/*hwnd,*/ hInstance))
	//{
	//	delete di;
	//	return 1;
	//}
	//delete di;

	//Iniciar objetos

	CX52Salida* x52Drv = new CX52Salida();
	CMenuMFD* mfd = new CMenuMFD();
	CPerfil* perfil = new CPerfil();
	CComs* coms = new CComs(perfil);
	if (coms->Iniciar())
	{
		CColaEventos* colaEv = new CColaEventos();
		CSalidaHID* salida = new CSalidaHID(perfil, colaEv);
		if (salida->Iniciar())
		{
			CEntradaHID* entrada = new CEntradaHID(perfil, colaEv);
			if (entrada->Iniciar(hInstance))
			{
				coms->SetHwnd(entrada->GetHwnd());
				mfd->SetTextoInicio();
				entrada->LoopWnd();
			}
			delete entrada;
		}
		delete coms;
		delete salida;
		delete colaEv;
	}
	else
	{
		delete coms;
	}
	delete perfil;
	delete mfd;
	delete x52Drv;

	ReleaseSemaphore(mtx, 1, NULL);
	CloseHandle(mtx);

	return 0;
}
