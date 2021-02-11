#include "../framework.h"
#include "CVirtualHID.h"

CVirtualHID::CVirtualHID()
{
   RtlZeroMemory(&Estado, sizeof(Estado));
   hMutextRaton = CreateSemaphore(NULL, 1, 1, NULL);
}

CVirtualHID::~CVirtualHID()
{
    CloseHandle(hVHid);
    CloseHandle(hMutextRaton);
}

bool CVirtualHID::Iniciar()
{
    HANDLE hdev = CreateFile(L"\\\\.\\XHOTAS_VHID_Interface", GENERIC_WRITE, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
    if (INVALID_HANDLE_VALUE == hdev)
    {
        return false;
    }

    hVHid = hdev;
    return true;
}

void CVirtualHID::EnviarRequestRaton(BYTE* inputData)
{
    DWORD tam = 0;
    UCHAR* buff = new UCHAR[sizeof(Estado.Raton) + 1];
    buff[0] = 2;
    RtlCopyMemory(&buff[1], inputData, sizeof(Estado.Raton));
    WriteFile(hVHid, buff, sizeof(Estado.Raton) + 1, &tam, NULL);
    //DWORD err = GetLastError();
    delete[] buff;

}

void CVirtualHID::EnviarRequestTeclado(BYTE* inputData)
{
    DWORD tam = 0;
    UCHAR* buff = new UCHAR[30];
    buff[0] = 1;
    RtlCopyMemory(&buff[1], inputData, 29);
    WriteFile(hVHid, buff, 30, &tam, NULL);
    //DWORD err = GetLastError();
    delete[] buff;

}

void CVirtualHID::EnviarRequestJoystick(UCHAR joyId, PVHID_INPUT_DATA inputData)
{
    DWORD tam = 0;
    UCHAR* buff = new UCHAR[sizeof(VHID_INPUT_DATA) + 1];
    buff[0] = joyId + 3;
    RtlCopyMemory(&buff[1], inputData, sizeof(VHID_INPUT_DATA));
    WriteFile(hVHid, buff, sizeof(VHID_INPUT_DATA) + 1, &tam, NULL);
    //DWORD err = GetLastError();
    delete[] buff;
}
