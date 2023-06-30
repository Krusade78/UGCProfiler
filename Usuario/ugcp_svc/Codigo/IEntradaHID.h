#pragma once
#include <strsafe.h>

constexpr auto BUFF_TAM = 64;

class IEntradaHID
{
public:
    virtual bool Preparar() = 0;
    virtual bool Abrir() = 0;
    virtual void Cerrar() = 0;
	virtual unsigned short Leer(void* buff) = 0;

	inline static bool CompararHardwareId(wchar_t* path, const wchar_t* hardwareId)
    {
        int tam = lstrlen(path) + 1;
        wchar_t* cmps = new wchar_t[tam];
        StringCchCopy(cmps, tam, path);
        RtlZeroMemory(&cmps[lstrlen(hardwareId)], sizeof(wchar_t));
        bool ok = (lstrcmpi(cmps, hardwareId) == 0);
        delete[] cmps;
        return ok;
    }
}; 

