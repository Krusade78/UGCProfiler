// xhotas_svc.cpp : Define el punto de entrada de la aplicación.
//

#include "Codigo/framework.h"
#include "ugcp_svc.h"
#include "Codigo/CalibradoDx/CDirectInput.h"
#include "Codigo/Perfil/CPerfil.h"
#include "Codigo/CComs.h"
#include "Codigo/ColaEventos/CColaEventos.h"
#include "Codigo/CSalidaHID.h"
#include "Codigo/CEntradaHID.h"
#include "Codigo/X52/EscribirUSBX52.h"
#include "Codigo/X52/MenuMFD.h"
#include "Codigo/NXT/EscribirHIDNXT.h"
#include <Dbt.h>
#include <sas.h>
#define DLLIMPORT
#include "CExportados.h"
#pragma comment(lib, "Sas.lib")
DWORD WINAPI Menu(LPVOID param);

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

	CExportados* pExp = new CExportados();
	pExp->Iniciar();

	//Iniciar objetos

	CX52Salida* x52Drv = new CX52Salida();
	CNXTSalida* nxtDrv = new CNXTSalida();
	CMenuMFD* mfd = new CMenuMFD();
	CVirtualHID* vhid = new CVirtualHID();
	if (vhid->Iniciar())
	{
		CPerfil* perfil = new CPerfil(vhid);
		CComs* coms = new CComs(perfil);
		if (coms->Iniciar())
		{
			CColaEventos* colaEv = new CColaEventos();
			CSalidaHID* salida = new CSalidaHID(perfil, colaEv, vhid);
			if (salida->Iniciar())
			{
				CEntradaHID* entrada = new CEntradaHID(perfil, colaEv);
				if (entrada->Iniciar(hInstance))
				{
					coms->SetHwnd(entrada->GetHwnd());
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
	}
	delete vhid;
	delete mfd;
	delete nxtDrv;
	delete x52Drv;

	delete pExp;

	return 0;
}

