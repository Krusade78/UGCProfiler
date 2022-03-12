#include "CExportados.h"
#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#pragma managed
#include <msclr\auto_gcroot.h>

class CSaCppInterno
{
public:
	msclr::auto_gcroot<Launcher::CMain^> launcher;
};

CExportados::CExportados()
{
	pImportado = new CSaCppInterno;
	pImportado->launcher = gcnew Launcher::CMain;
}
CExportados::~CExportados()
{
	delete pImportado;
	pImportado = nullptr;
}

void CExportados::Iniciar()
{
	pImportado->launcher->Iniciar();
}


