#pragma once
#pragma unmanaged

#ifdef DLLIMPORT
#define DLLAPI __declspec(dllimport)
#else
#define DLLAPI __declspec(dllexport)
#endif

class InternalCS2Cpp;

class DLLAPI CExported
{
private:
	InternalCS2Cpp* pImported = nullptr;
public:
	CExported();
	virtual ~CExported();

	void LoadDefault();
};
