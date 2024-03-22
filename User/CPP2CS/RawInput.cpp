#include "RawInput.h"
#include <strsafe.h>
#pragma managed

CPP2CS::RawInput::RawInput(CPP2CS::WndProcCallback^ callback)
{
	this->callback = callback;
	unmanaged = new URawInput();
}

void CPP2CS::RawInput::Init()
{
	reinterpret_cast<URawInput*>(unmanaged)->Init(new msclr::auto_gcroot<CPP2CS::RawInput^>(this));
}

void CPP2CS::RawInput::Close()
{
	if (unmanaged != nullptr)
	{
		reinterpret_cast<URawInput*>(unmanaged)->Close();
		delete unmanaged;
		unmanaged = nullptr;
	}
}

void CPP2CS::RawInput::Call(System::String^ hidInterface, array<System::Byte>^ data)
{
	this->callback(hidInterface, data);
}

#pragma unmanaged
LRESULT CALLBACK URawInput::WndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch (message)
	{
	case WM_NCCREATE:
		return 1;
	case WM_CLOSE:
		DestroyWindow(hWnd);
		return 0;
	case WM_DESTROY:
		PostQuitMessage(0);
		return 0;
	case 0xFF:
	{
		UINT size = 0;

		GetRawInputData(reinterpret_cast<HRAWINPUT>(lParam), RID_INPUT, NULL, &size, sizeof(RAWINPUTHEADER));
		if (size != 0)
		{
			BYTE* buff = new BYTE[size];

			UINT outSize = GetRawInputData(reinterpret_cast<HRAWINPUT>(lParam), RID_INPUT, buff, &size, sizeof(RAWINPUTHEADER));
			if (outSize == size)
			{
				RAWINPUT* raw = reinterpret_cast<RAWINPUT*>(buff);

				wchar_t pName[256]{};
				UINT cbSize = 256 * sizeof(wchar_t);
				GetRawInputDeviceInfoW(raw->header.hDevice, RIDI_DEVICENAME, pName, &cbSize);
				if (cbSize <= (256 * sizeof(wchar_t)))
				{
					{
						int size = lstrlenW(pName) + 1;
						wchar_t* cmps = new wchar_t[size];
						StringCchCopyW(cmps, size, pName);

						try
						{
							UINT32 deviceId = static_cast<UINT32>(wcstol(&pName[12], NULL, 16)) << 16;
							deviceId |= static_cast<UINT32>(wcstol(&pName[21], NULL, 16));

							URawInput* ptr = reinterpret_cast<URawInput*>(GetWindowLongPtrW(hWnd, GWLP_USERDATA));
							ptr->Call(cmps, size - 1, raw->data.hid.bRawData, raw->data.hid.dwSizeHid);
						}
						catch (...) {}
						delete[] cmps;
					}
				}
			}
			delete[] buff;
		}

		return 0;
	}
	default:
		return 0;
	}
}

void URawInput::Init(void* ptrm)
{
	this->ptrm = reinterpret_cast<msclr::gcroot<CPP2CS::RawInput^>*>(ptrm);

	WNDCLASSW wc = { 0 };
	wc.lpfnWndProc = WndProc;
	wc.hInstance = GetModuleHandle(NULL);
	wc.lpszClassName = L"UGCP RawInput";

	RegisterClassW(&wc);
	hWnd = CreateWindowW(L"UGCP RawInput", NULL, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, NULL, NULL);
	if (hWnd != NULL)
	{
		SetWindowLongPtrW(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(this));

		RAWINPUTDEVICE* rdev = new RAWINPUTDEVICE[2];
		rdev[0].usUsagePage = 0x01;
		rdev[0].usUsage = 0x04;
		rdev[0].hwndTarget = hWnd;
		rdev[0].dwFlags = 0;
		rdev[1].usUsagePage = 0x01;
		rdev[1].usUsage = 0x05;
		rdev[1].hwndTarget = hWnd;
		rdev[1].dwFlags = 0;

		if (!RegisterRawInputDevices(rdev, 1, sizeof(RAWINPUTDEVICE)))
		{
			DestroyWindow(hWnd);
			return;
		}

		MSG msg;
		while (GetMessageW(&msg, NULL, 0, 0) != 0)
		{
			DispatchMessage(&msg);
		}

		hWnd = NULL;
	}
}

void URawInput::Close() const
{
	if (hWnd != NULL)
	{
		SendMessageW(hWnd, WM_CLOSE, 0, 0);
	}
}
#pragma managed

void URawInput::Call(wchar_t* hidInterface, int interfaceSize, BYTE* data, DWORD dataSize)
{
	array<BYTE>^ mdata = gcnew array<BYTE>(dataSize);
	pin_ptr<BYTE> pmdata = &mdata[0];
	memcpy_s(pmdata, dataSize, data, dataSize);
	array<wchar_t>^ minterface = gcnew array<wchar_t>(interfaceSize);
	pin_ptr<wchar_t> pminterface = &minterface[0];
	memcpy_s(pminterface, interfaceSize * sizeof(wchar_t), hidInterface, interfaceSize * sizeof(wchar_t));
	(*ptrm)->Call(gcnew System::String(minterface), mdata);
}

