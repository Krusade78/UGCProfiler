#include "../framework.h"
#include <SetupAPI.h>
#include <hidsdi.h>
#include "CWinUSBX52.h"
#include "MFDMenu.h"


bool CWinUSBX52::Prepare()
{
    Close(false);

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

    auto buf = std::make_unique<std::uint8_t[]>(size);
    memset(buf.get(), 0, size);
    PSP_DEVICE_INTERFACE_DETAIL_DATA didData = reinterpret_cast<PSP_DEVICE_INTERFACE_DETAIL_DATA>(buf.get());
    didData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
    if (!SetupDiGetDeviceInterfaceDetail(diDevs, &diData, didData, size, &size, NULL))
    {
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    {
        std::lock_guard<std::mutex> lock(mutex);
        if (hardwareId == HARDWARE_ID_X52)
        {
            pathInterface = didData->DevicePath; //string is copied
        }
    }

    SetupDiDestroyDeviceInfoList(diDevs);

    return true;
}

bool CWinUSBX52::Open()
{
    std::unique_lock<std::mutex> lock(mutex);
    {
        if ((hardwareId != 0) && (!pathInterface.empty()) && (hwusb.load() == nullptr))
        {
            unique_handle nhdev(CreateFileW(pathInterface.c_str(), GENERIC_WRITE | GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, NULL));
            if (!nhdev.valid())
            {
                return false;
            }
            else
            {
                WINUSB_INTERFACE_HANDLE wih = nullptr;
                if (!WinUsb_Initialize(nhdev.get(), &wih))
                {
                    lock.unlock();
                    Close(false);
                    return false;
                }

                hdev.set(nhdev.move()); //hdev should be nullptr here
                hwusb.store(wih);

                memset(&pipe, 0, sizeof(WINUSB_PIPE_INFORMATION));
                if (!WinUsb_QueryPipe(wih, 0, 0, &pipe))
                {
                    lock.unlock();
                    Close(false);
                    return false;
                }

                DWORD newReportLenght = 0;
                if (HidD_GetPreparsedData(nhdev.get(), reinterpret_cast<PHIDP_PREPARSED_DATA*>(&preparsed)))
                {
                    HIDP_CAPS caps;
                    if (HidP_GetCaps(reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), &caps) == HIDP_STATUS_SUCCESS)
                    {
                        if (GetDeviceMap(preparsed, &caps))
                        {
                            newReportLenght = caps.InputReportByteLength;
                        }
                    }
                }
                if (newReportLenght != 0)
                {
                    reportLenght.store(newReportLenght);
                    reportBuffer = std::make_unique<CHAR[]>(newReportLenght);
                    CMFDMenu::Get().SetWelcome();
                }
                else
                {
                    lock.unlock();
                    Close(false);
                    return false;
                }
            }
        }
    }

    return true;
}

void CWinUSBX52::Close(bool exit)
{
    {
        std::unique_lock<std::mutex> lock(mutex);
        HANDLE h = hwusb.exchange(nullptr);
        if (h != nullptr)
        {
            CancelIoEx(h, NULL);
            WinUsb_Free(h);
        }
    }

    CHIDDevices::Close(exit);
}

unsigned short CWinUSBX52::Read(void* buff)
{
    if (!paused.load())
    {
        auto hUSB = hwusb.load();
        if (hUSB == nullptr)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(3000));
            bool prepared;
            bool exit;
            {
                std::unique_lock<std::mutex> lock(mutex);
                prepared = !pathInterface.empty();
                exit = (hardwareId == 0);
            }
            if (exit)
            {
                return 0;
            }
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
            static_cast<char*>(buff)[0] = 0; //report id
            if (WinUsb_ReadPipe(hUSB, pipe.PipeId, &static_cast<std::uint8_t*>(buff)[1], reportLenght.load() - 1, &readSize, nullptr))
            {
                readSize++;
                ULONG size = 0;
                if (HidP_GetData(HidP_Input, nullptr, &size, reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), reportBuffer.get(), reportLenght.load()) == HIDP_STATUS_BUFFER_TOO_SMALL)
                {
                    if (HidP_GetData(HidP_Input, reinterpret_cast<PHIDP_DATA>(buff), &size, reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), reportBuffer.get(), reportLenght.load()) == HIDP_STATUS_SUCCESS)
                    {
                        return static_cast<unsigned short>(size);
                    }
                }
                return 0;
            }
        }
    }

    std::this_thread::sleep_for(std::chrono::milliseconds(1500));
    return 0;
}

void CWinUSBX52::SetPause(bool onoff)
{
    if (onoff)
    {
        if (paused.exchange(true) == false)
        {
            Close(false);
        }
    }
    else
    {
        Close(false);
        paused.store(false);
    }
}