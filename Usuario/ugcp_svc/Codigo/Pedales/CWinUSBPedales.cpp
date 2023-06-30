#include "../framework.h"
#include <SetupAPI.h>
#include <hidsdi.h>
#include "CWinUSBPedales.h"

CPedalesEntrada::CPedalesEntrada()
{
    mutexOperar = CreateSemaphore(NULL, 1, 1, NULL);
}

CPedalesEntrada::~CPedalesEntrada()
{
    Cerrar();
    CloseHandle(mutexOperar);
}

bool CPedalesEntrada::Preparar()
{
    Cerrar();

    HDEVINFO diDevs = INVALID_HANDLE_VALUE;
    GUID hidGuid;
    SP_DEVICE_INTERFACE_DATA diData{};

    HidD_GetHidGuid(&hidGuid);
    diDevs = SetupDiGetClassDevs(&hidGuid, NULL, NULL, (DIGCF_PRESENT | DIGCF_DEVICEINTERFACE));
    if (INVALID_HANDLE_VALUE == diDevs)
    {
        return false;
    }

    diData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
    DWORD idx = 0;
    while (SetupDiEnumDeviceInterfaces(diDevs, NULL, &hidGuid, idx++, &diData))
    {
        DWORD tam = 0;
        SetupDiGetDeviceInterfaceDetail(diDevs, &diData, NULL, 0, &tam, NULL);

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

        if (IEntradaHID::CompararHardwareId(didData->DevicePath, HARDWARE_ID_PEDALES))
        {
            WaitForSingleObject(mutexOperar, INFINITE);
            {
                rutaPedales = new wchar_t[tam];
                StringCchCopy(rutaPedales, tam, didData->DevicePath);
            }
            ReleaseSemaphore(mutexOperar, 1, NULL);
        }

        delete[] buf;
    }
    if (GetLastError() != ERROR_NO_MORE_ITEMS)
    {
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    SetupDiDestroyDeviceInfoList(diDevs);

    return true;
}

bool CPedalesEntrada::Abrir()
{
    WaitForSingleObject(mutexOperar, INFINITE);
    {
        if ((rutaPedales != nullptr) && (InterlockedCompareExchangePointer(&hdevPedales, nullptr, nullptr) == nullptr))
        {
            PVOID hdev = CreateFile(rutaPedales, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
            if (INVALID_HANDLE_VALUE == hdev)
            {
                ReleaseSemaphore(mutexOperar, 1, NULL);
                return false;
            }
            else
            {
                InterlockedExchangePointer(&hdevPedales, hdev);
            }
        }
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);

    return true;
}

void CPedalesEntrada::Cerrar()
{
    WaitForSingleObject(mutexOperar, INFINITE);
    {
        if (InterlockedCompareExchangePointer(&hdevPedales, nullptr, nullptr) != nullptr)
        {
            HANDLE h = InterlockedExchangePointer(&hdevPedales, nullptr);
            CancelIoEx(h, NULL);
            CloseHandle(h);
        }

        delete[] rutaPedales; rutaPedales = nullptr;
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);
}

unsigned short CPedalesEntrada::Leer(void* buff)
{
    DWORD tam = 0;
    if (InterlockedCompareExchangePointer(&hdevPedales, nullptr, nullptr) == nullptr)
    {
        Sleep(3000);
        return 0;
    }
    else
    {
        if (!ReadFile(hdevPedales, buff, READ_TAM, &tam, NULL))
        {
            Sleep(1500);
            return 0;
        }
    }

    return static_cast<unsigned short>(tam);
}