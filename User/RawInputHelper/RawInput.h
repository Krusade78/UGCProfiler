#pragma once
#pragma unmanaged
#include <SDKDDKVer.h>
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#pragma managed
#include <msclr/auto_gcroot.h>

namespace CPP2CS
{
	public delegate void WndProcCallback(System::String^ hidInterface, array<System::Byte>^ hidData);

	public ref class RawInput
	{
	public:
		RawInput(WndProcCallback^ callback);
		inline ~RawInput() { Close(); }
		void Call(System::String^ hidInterface, array<System::Byte>^ data);
		void Init();
		void Close();
	private:
		void* unmanaged = nullptr;
		WndProcCallback^ callback = nullptr;
	};
}

#pragma unmanaged
class URawInput
{
public:
	void Init(void* ptrm);
	void Close() const;
	void Call(wchar_t* hidInterface, int interfaceSize, BYTE* data, DWORD dataSize);
private:
	void ProcessRawInput() const;
	HANDLE hEvClose = NULL;
	HWND hWnd = NULL;
	msclr::gcroot<CPP2CS::RawInput^>* ptrm = nullptr;
};

