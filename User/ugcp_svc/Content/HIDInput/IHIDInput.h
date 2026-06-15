#pragma once
#include <wchar.h>
#include <Shlwapi.h>

#pragma comment(lib, "Shlwapi.lib")

class IHIDInput
{
public:
    virtual ~IHIDInput() = default;
    virtual bool Prepare() = 0;
    virtual bool Open() = 0;
    virtual void Close(bool exit) = 0;
	virtual unsigned short Read(void* buff) = 0;

	inline static std::uint32_t GetHardwareId(wchar_t* path)
    {
        std::uint32_t ret = 0xFFFFFFFF;

        const wchar_t* vidStr = StrStrIW(path, L"VID_");
        if (vidStr)
        {
            vidStr += 4;
            const wchar_t* pidStr = vidStr + 9;

            ret = wcstoul(vidStr, nullptr, 16) << 16;
            ret |= wcstoul(pidStr, nullptr, 16);
        }
        return ret;
    }
}; 

