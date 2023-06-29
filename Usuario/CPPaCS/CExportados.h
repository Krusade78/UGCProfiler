#pragma once
#pragma unmanaged

#ifdef DLLIMPORT
#define DLLAPI __declspec(dllimport)
#else
#define DLLAPI __declspec(dllexport)
#endif

class CSaCppInterno;

class DLLAPI CExportados
{
private:
	CSaCppInterno* pImportado = nullptr;
public:
	CExportados();
	virtual ~CExportados();
	void Iniciar();
};
