#include "Content/framework.h"
#include "ugcp_svc.h"
#include "Content/Profile/CProfile.h"
#include "Content/CComs.h"
#include "Content/EventQueue/CEventQueue.h"
#include "Content/CHIDOutput.h"
#include "Content/HIDInput/CHIDInput.h"
#include "Content/X52/USBX52Write.h"
#include "Content/X52/MFDMenu.h"
#include "Content/NXT/HIDNXTWrite.h"
#include <Dbt.h>
#include <sas.h>
#define DLLIMPORT
#include "../CPP2CS/CExported.h"
#pragma comment(lib, "Sas.lib")

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

	CExported* pExp = new CExported();

	//Init

	CX52Write* x52Drv = new CX52Write();
	CNXTWrite* nxtDrv = new CNXTWrite();
	CMFDMenu* mfd = new CMFDMenu();
	CVirtualHID* vhid = new CVirtualHID();
	if (vhid->Init())
	{
		CProfile* profile = new CProfile(vhid);
		CComs* coms = new CComs(profile);
		if (coms->Init())
		{
			CEventQueue* evQueue = new CEventQueue();
			CHIDOutput* output = new CHIDOutput(profile, evQueue, vhid);
			if (output->Init())
			{
				CHIDInput* input = new CHIDInput(profile, evQueue);
				if (input->Init(hInstance))
				{
					coms->SetHwnd(input->GetHwnd());
					pExp->LoadDefault();
					input->LoopWnd();
				}
				delete input;
			}
			delete coms;
			delete output;
			delete evQueue;
		}
		else
		{
			delete coms;
		}
		delete profile;
	}
	delete vhid;
	delete mfd;
	delete nxtDrv;
	delete x52Drv;

	delete pExp;

	return 0;
}

