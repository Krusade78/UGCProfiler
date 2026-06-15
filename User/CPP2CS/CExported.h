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
	~CExported();

	//Prevent wrong use
	CExported(const CExported&) = delete;
	CExported& operator=(const CExported&) = delete;
	CExported(CExported&&) = delete;
	CExported& operator=(CExported&&) = delete;
	//----

	void LoadDefault();
};
