#include "RawInput.h"
#include <strsafe.h>
#include <winternl.h>
#include <hidusage.h>
#include <hidpi.h>
typedef unsigned __int64 QWORD;
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
void URawInput::ProcessRawInput() const
{
		UINT size = 0;

		if (GetRawInputBuffer(NULL, &size, sizeof(RAWINPUTHEADER)) != 0)
		{
			Sleep(100);
			return;
		}
		if (size == 0)
		{
			Sleep(50);
			return;
		}
		size *= 128; // up to 128 messages

		PRAWINPUT pRawInput = reinterpret_cast<PRAWINPUT>(new BYTE[size]);
		while(true)
		{
			UINT sizeT = size;
			UINT nInput = GetRawInputBuffer(pRawInput, &sizeT, sizeof(RAWINPUTHEADER));
			if (nInput <= 0)
			{
				break;
			}

			RAWINPUT* raw = pRawInput;
			for (; nInput > 0; nInput--)
			{
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

							UINT ppdSize = 0;
							GetRawInputDeviceInfoW(raw->header.hDevice, RIDI_PREPARSEDDATA, NULL, &ppdSize);
							BYTE* ppdBuffer = new BYTE[ppdSize];
							cbSize = ppdSize;
							if (GetRawInputDeviceInfoW(raw->header.hDevice, RIDI_PREPARSEDDATA, ppdBuffer, &cbSize) == ppdSize)
							{
								ULONG dataSize = 0;
								if (HIDP_STATUS_BUFFER_TOO_SMALL == HidP_GetData(HidP_Input, NULL, &dataSize, reinterpret_cast<PHIDP_PREPARSED_DATA>(ppdBuffer), reinterpret_cast<PCHAR>(raw->data.hid.bRawData), raw->data.hid.dwSizeHid))
								{
									PHIDP_DATA data = new HIDP_DATA[dataSize];
									if (HIDP_STATUS_SUCCESS == HidP_GetData(HidP_Input, data, &dataSize, reinterpret_cast<PHIDP_PREPARSED_DATA>(ppdBuffer), reinterpret_cast<PCHAR>(raw->data.hid.bRawData), raw->data.hid.dwSizeHid))
									{
										URawInput* ptr = reinterpret_cast<URawInput*>(GetWindowLongPtrW(hWnd, GWLP_USERDATA));
										ptr->Call(cmps, size - 1, reinterpret_cast<BYTE*>(data), sizeof(HIDP_DATA) * dataSize);
									}
									delete[] data;
								}
							}
							delete[] ppdBuffer;
						}
						catch (...) {}
						delete[] cmps;
					}
				}

				raw = NEXTRAWINPUTBLOCK(raw);
			}
		}
		delete[] reinterpret_cast<BYTE*>(pRawInput);
		pRawInput = NULL;
}

void URawInput::Init(void* ptrm)
{
	this->ptrm = reinterpret_cast<msclr::gcroot<CPP2CS::RawInput^>*>(ptrm);

	hEvClose = CreateEventW(NULL, TRUE, FALSE, NULL);
	if (hEvClose != NULL)
	{
		hWnd = CreateWindowExW(0, L"STATIC", NULL, 0, 0, 0, 0, 0, HWND_MESSAGE, NULL, GetModuleHandle(NULL), NULL);
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

			while (true)
			{
				if (WaitForSingleObject(hEvClose, 0) == WAIT_OBJECT_0)
				{
					break;
				}
				DWORD ret = MsgWaitForMultipleObjects(1, &hEvClose, 0, INFINITE, QS_RAWINPUT);
				if (ret != (WAIT_OBJECT_0 + 1))
				{
					break;
				}
				else
				{
					ProcessRawInput();
				}
			}

			DestroyWindow(hWnd);
			hWnd = NULL;
		}

		ResetEvent(hEvClose);
	}
}

void URawInput::Close() const
{
	if (hEvClose != NULL)
	{
		SetEvent(hEvClose);
		while (WaitForSingleObject(hEvClose, 500) == WAIT_OBJECT_0)
		{
			Sleep(100);
		}
		CloseHandle(hEvClose);
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

