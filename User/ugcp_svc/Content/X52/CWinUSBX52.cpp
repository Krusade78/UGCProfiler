#include "../framework.h"
#include <SetupAPI.h>
#include <hidsdi.h>
#include "CWinUSBX52.h"
#include "MFDMenu.h"
#include "USBX52Write.h"

bool CWinUSBX52::Prepare()
{
    Close();

    HDEVINFO diDevs = INVALID_HANDLE_VALUE;
    SP_DEVICE_INTERFACE_DATA diData{};

    diDevs = SetupDiGetClassDevs(&guidInterface, NULL, NULL, (DIGCF_PRESENT | DIGCF_DEVICEINTERFACE));
    if (INVALID_HANDLE_VALUE == diDevs)
    {
        return false;
    }

    diData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
    if (!SetupDiEnumDeviceInterfaces(diDevs, NULL, &guidInterface, 0, &diData))
    {
        DWORD err = GetLastError();
        SetupDiDestroyDeviceInfoList(diDevs);
        return (err == ERROR_NO_MORE_ITEMS);
    }

    DWORD size = 0;
    if ((FALSE == SetupDiGetDeviceInterfaceDetail(diDevs, &diData, NULL, 0, &size, NULL)) && (ERROR_INSUFFICIENT_BUFFER != GetLastError()))
    {
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    UCHAR* buf = new UCHAR[size];
    RtlZeroMemory(buf, size);
    PSP_DEVICE_INTERFACE_DETAIL_DATA didData = (PSP_DEVICE_INTERFACE_DETAIL_DATA)buf;
    didData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
    if (!SetupDiGetDeviceInterfaceDetail(diDevs, &diData, didData, size, &size, NULL))
    {
        delete[] buf;
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    WaitForSingleObject(mutex, INFINITE);
    {
        pathInterface = new wchar_t[size];
        StringCchCopy(pathInterface, size, didData->DevicePath);
    }
    ReleaseSemaphore(mutex, 1, NULL);

    delete[] buf;
    SetupDiDestroyDeviceInfoList(diDevs);

    return true;
}

bool CWinUSBX52::Open()
{
    WaitForSingleObject(mutex, INFINITE);
    {
        if ((pathInterface != nullptr) && (InterlockedCompareExchangePointer(&hwusb, nullptr, nullptr) == nullptr))
        {
            PVOID nhdev = CreateFile(pathInterface, GENERIC_WRITE | GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, NULL);
            if (INVALID_HANDLE_VALUE == nhdev)
            {
                ReleaseSemaphore(mutex, 1, NULL);
                return false;
            }
            else
            {
                WINUSB_INTERFACE_HANDLE wih = nullptr;
                if (!WinUsb_Initialize(nhdev, &wih))
                {
                    CloseHandle(nhdev);
                    ReleaseSemaphore(mutex, 1, NULL);
                    Close();
                    return false;
                }

                InterlockedExchangePointer(&hdev, nhdev);
                InterlockedExchangePointer(&hwusb, wih);

                ZeroMemory(&pipe, sizeof(WINUSB_PIPE_INFORMATION));
                if (!WinUsb_QueryPipe(hwusb, 0, 0, &pipe))
                {
                    ReleaseSemaphore(mutex, 1, NULL);
                    Close();
                    return false;
                }

                reportLenght = 0;
                if (HidD_GetPreparsedData(nhdev, reinterpret_cast<PHIDP_PREPARSED_DATA*>(&preparsed)))
                {
                    HIDP_CAPS caps;
                    if (HidP_GetCaps(reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), &caps) == HIDP_STATUS_SUCCESS)
                    {
                        if (GetDeviceMap(preparsed, &caps))
                        {
                            reportLenght = caps.InputReportByteLength;
                        }
                    }
                }
                if (reportLenght != 0)
                {
                    reportBuffer = new CHAR[reportLenght];
                    CX52Write::Get()->SetWinUSB(hwusb);
                    CMFDMenu::Get()->SetWelcome();
                }
                else
                {
                    if (reportBuffer != nullptr)
                    {
                        delete[] reportBuffer;
                        reportBuffer = nullptr;
                    }
                    if (preparsed != nullptr)
                    {
                        HidD_FreePreparsedData(reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed));
                        preparsed = nullptr;
                    }
                    ReleaseSemaphore(mutex, 1, NULL);
                    Close();
                    return false;
                }
            }
        }
    }
    ReleaseSemaphore(mutex, 1, NULL);

    return true;
}

void CWinUSBX52::Close()
{
    WaitForSingleObject(mutex, INFINITE);
    {
        if (InterlockedCompareExchangePointer(&hwusb, nullptr, nullptr) != nullptr)
        {
            CX52Write::Get()->SetWinUSB(nullptr);

            HANDLE h = InterlockedExchangePointer(&hwusb, nullptr);
            CancelIoEx(h, NULL);
            WinUsb_Free(h);
        }
    }
    ReleaseSemaphore(mutex, 1, NULL);

    CHIDDevices::Close();
}

unsigned short CWinUSBX52::Read(void* buff)
{
    if (!InterlockedOr8(&paused, 0))
    {
        if (InterlockedCompareExchangePointer(&hwusb, nullptr, nullptr) == nullptr)
        {
            Sleep(3000);
            WaitForSingleObject(mutex, INFINITE);
            bool prepared = (pathInterface != nullptr);
            ReleaseSemaphore(mutex, 1, NULL);
            if (!prepared)
            {
                Prepare();
            }
            Open();
            return 0;
        }
        else
        {
            ULONG readSize = 0;
            ((CHAR*)buff)[0] = 0; //report id
            if (WinUsb_ReadPipe(hwusb, pipe.PipeId, &static_cast<UCHAR*>(buff)[1], reportLenght - 1, &readSize, NULL))
            {
                readSize++;
                ULONG size = 0;
                if (HidP_GetData(HidP_Input, NULL, &size, reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), reportBuffer, reportLenght) == HIDP_STATUS_BUFFER_TOO_SMALL)
                {
                    if (HidP_GetData(HidP_Input, reinterpret_cast<PHIDP_DATA>(buff), &size, reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), reportBuffer, reportLenght) == HIDP_STATUS_SUCCESS)
                    {
                        return static_cast<unsigned short>(size);
                    }
                }
                return 0;
            }
        }
    }

    Sleep(1500);
    return 0;
}

void CWinUSBX52::SetPause(bool onoff)
{
    if (onoff)
    {
        if (InterlockedOr8(&paused, 1) == 0)
        {
            Close();
        }
    }
    else
    {
        Close();
        InterlockedAnd8(&paused, 0);
    }
}