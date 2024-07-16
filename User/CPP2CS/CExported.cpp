#include "CExported.h"
#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#pragma managed
#include <msclr\auto_gcroot.h>

class InternalCS2Cpp
{
public:
	msclr::auto_gcroot<LauncherWrapper::Passthrough^> launcher;
};

CExported::CExported()
{
	pImported = new InternalCS2Cpp;
	pImported->launcher = gcnew LauncherWrapper::Passthrough;
	pImported->launcher->Init();
}

CExported::~CExported()
{
	if (pImported != nullptr) { delete pImported; }
	pImported = nullptr;
}

void CExported::LoadDefault()
{
	pImported->launcher->LoadDefault();
}
