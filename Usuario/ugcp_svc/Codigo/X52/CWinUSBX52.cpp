#include "../framework.h"
#include <SetupAPI.h>
#include "CWinUSBX52.h"
#include "MenuMFD.h"
#include "EscribirUSBX52.h"

CX52Entrada::CX52Entrada()
{
    mutexOperar = CreateSemaphore(NULL, 1, 1, NULL);
}

CX52Entrada::~CX52Entrada()
{
    Cerrar();
    CloseHandle(mutexOperar);
}

bool CX52Entrada::Preparar()
{
    Cerrar();

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

    DWORD tam = 0;
    if ((FALSE == SetupDiGetDeviceInterfaceDetail(diDevs, &diData, NULL, 0, &tam, NULL)) && (ERROR_INSUFFICIENT_BUFFER != GetLastError()))
    {
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    UCHAR* buf = new UCHAR[tam];
    RtlZeroMemory(buf, tam);
    PSP_DEVICE_INTERFACE_DETAIL_DATA didData = (PSP_DEVICE_INTERFACE_DETAIL_DATA)buf;
    didData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
    if (!SetupDiGetDeviceInterfaceDetail(diDevs, &diData, didData, tam, &tam, NULL))
    {
        delete[] buf;
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    WaitForSingleObject(mutexOperar, INFINITE);
    {
        rutaPedales = new wchar_t[tam];
        StringCchCopy(rutaPedales, tam, didData->DevicePath);
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);

    delete[] buf;
    SetupDiDestroyDeviceInfoList(diDevs);

    return true;
}

bool CX52Entrada::Abrir()
{
    WaitForSingleObject(mutexOperar, INFINITE);
    {
        if ((rutaPedales != nullptr) && (InterlockedCompareExchangePointer(&hwusb, nullptr, nullptr) == nullptr))
        {
            PVOID hdev = CreateFile(rutaPedales, GENERIC_WRITE | GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, NULL);
            if (INVALID_HANDLE_VALUE == hdev)
            {
                ReleaseSemaphore(mutexOperar, 1, NULL);
                return false;
            }
            else
            {
                WINUSB_INTERFACE_HANDLE wih = nullptr;
                if (!WinUsb_Initialize(hdev, &wih))
                {
                    CloseHandle(hdev);
                    ReleaseSemaphore(mutexOperar, 1, NULL);
                    Cerrar();
                    return false;
                }

                usbh = hdev;
                InterlockedExchangePointer(&hwusb, wih);

                ZeroMemory(&pipe, sizeof(WINUSB_PIPE_INFORMATION));
                if (!WinUsb_QueryPipe(hwusb, 0, 0, &pipe))
                {
                    ReleaseSemaphore(mutexOperar, 1, NULL);
                    Cerrar();
                    return false;
                }

                CX52Salida::Get()->SetWinUSB(hwusb);
                CMenuMFD::Get()->SetInicio();
            }
        }
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);

    return true;
}

void CX52Entrada::Cerrar()
{
    WaitForSingleObject(mutexOperar, INFINITE);
    {
        if (InterlockedCompareExchangePointer(&hwusb, nullptr, nullptr) != nullptr)
        {
            CX52Salida::Get()->SetWinUSB(nullptr);

            HANDLE h = InterlockedExchangePointer(&hwusb, nullptr);
            CancelIoEx(h, NULL);
            WinUsb_Free(h);
            CancelIoEx(usbh, NULL);
            CloseHandle(usbh);
            usbh = INVALID_HANDLE_VALUE;
        }

        delete[] rutaPedales; rutaPedales = nullptr;
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);
}

unsigned short CX52Entrada::Leer(void* buff)
{
    if (InterlockedCompareExchangePointer(&hwusb, nullptr, nullptr) == nullptr)
    {
        Sleep(3000);
        return 0;
    }
    else
    {
        ULONG tam = 0;
        ((CHAR*)buff)[0] = 0;
        if (!WinUsb_ReadPipe(hwusb, pipe.PipeId, &static_cast<UCHAR*>(buff)[1], 14, &tam, NULL))
        {
            Sleep(1500);
            return 0;
        }
    }

    return 15;
}