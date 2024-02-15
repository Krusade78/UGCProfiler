#pragma once
#include <strsafe.h>
#include <stdlib.h>

constexpr auto BUFF_TAM = 64;

class IHIDInput
{
public:
    virtual bool Prepare() = 0;
    virtual bool Open() = 0;
    virtual void Close() = 0;
	virtual unsigned short Read(void* buff) = 0;

	inline static UINT32 GetHardwareId(wchar_t* path)
    {
        int size = lstrlen(path) + 1;
        wchar_t* cmps = new wchar_t[size];
        StringCchCopy(cmps, size, path);

        UINT32 ret = 0;
        try
        {
            ret = static_cast<UINT32>(wcstol(&path[12], NULL, 16)) << 16;
            ret |= static_cast<UINT32>(wcstol(&path[21], NULL, 16));
        }
        catch(...) {}
        delete[] cmps;
        return ret;
    }
}; 

