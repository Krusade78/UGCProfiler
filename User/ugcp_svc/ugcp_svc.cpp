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
#include <memory>
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

	unique_handle mtx(CreateMutexW(nullptr, TRUE, L"mtx_xhotas_svc"));
	if ((GetLastError() == ERROR_ALREADY_EXISTS) || !mtx.valid())
	{
		return 1;
	}

	auto pExp = std::make_unique<CExported>();

	//Init

	auto x52Drv = std::make_unique<CX52Write>();
	auto nxtDrv = std::make_unique<CNXTWrite>();
	auto mfd = std::make_unique<CMFDMenu>();
	auto vhid = std::make_unique<CVirtualHID>();
	if (vhid->Init())
	{
		auto profile = std::make_unique<CProfile>();
		auto coms = std::make_unique<CComs>(*profile);
		if (coms->Init())
		{
			auto evQueue = std::make_unique<CEventQueue>();
			auto output = std::make_unique<CHIDOutput>(*profile, *evQueue, *vhid);
			if (output->Init())
			{
				auto input = std::make_unique<CHIDInput>(*profile, *evQueue);
				if (input->Init(hInstance))
				{
					coms->SetHwnd(input->GetHwnd());
					pExp->LoadDefault();
					input->LoopWnd();
				}
			}
		}
	}

	return 0;
}

