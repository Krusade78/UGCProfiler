#include "../framework.h"
#include "LeerHIDNXT.h"
#include <SetupAPI.h>
#include <hidsdi.h>
#include "EscribirHIDNXT.h"

CNXTEntrada::CNXTEntrada()
{
    mutexOperar = CreateSemaphore(NULL, 1, 1, NULL);
}

CNXTEntrada::~CNXTEntrada()
{
    Cerrar();
    CloseHandle(mutexOperar);
}

bool CNXTEntrada::Preparar()
{
    Cerrar();

    HDEVINFO                            diDevs = INVALID_HANDLE_VALUE;
    GUID                                hidGuid;
    SP_DEVICE_INTERFACE_DATA            diData{};

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

        if (IEntradaHID::CompararHardwareId(didData->DevicePath, HARDWARE_ID_NXT))
        {
            WaitForSingleObject(mutexOperar, INFINITE);
            {
                rutaNXT = new wchar_t[tam];
                StringCchCopy(rutaNXT, tam, didData->DevicePath);
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

bool CNXTEntrada::Abrir()
{
    WaitForSingleObject(mutexOperar, INFINITE);
    {
        if ((rutaNXT != nullptr) && (InterlockedCompareExchangePointer(&hdevNXT, nullptr, nullptr) == nullptr))
        {
            PVOID hdev = CreateFile(rutaNXT, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
            if (INVALID_HANDLE_VALUE == hdev)
            {
                ReleaseSemaphore(mutexOperar, 1, NULL);
                return false;
            }
            else
            {
                InterlockedExchangePointer(&hdevNXT, hdev);
                CNXTSalida::Get()->SetRuta(rutaNXT);
            }
        }
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);

    return true;
}

void CNXTEntrada::Cerrar()
{
    WaitForSingleObject(mutexOperar, INFINITE);
    {
        if (InterlockedCompareExchangePointer(&hdevNXT, nullptr, nullptr) != nullptr)
        {
            CNXTSalida::Get()->SetRuta(nullptr);
            HANDLE h = InterlockedExchangePointer(&hdevNXT, nullptr);
            CancelIoEx(h, NULL);
            CloseHandle(h);
        }

        delete[] rutaNXT; rutaNXT = nullptr;
    }
    ReleaseSemaphore(mutexOperar, 1, NULL);
}

unsigned short CNXTEntrada::Leer(void* buff)
{
    DWORD tam = 0;
    if (InterlockedCompareExchangePointer(&hdevNXT, nullptr, nullptr) == nullptr)
    {
        Sleep(3000);
    }
    else
    {
        if (!ReadFile(hdevNXT, buff, READ_TAM, &tam, NULL))
        {
            Sleep(1500);
            return 0;
        }
    }

	return static_cast<unsigned short>(tam);
}
