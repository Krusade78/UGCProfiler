#include "framework.h"
#include <strsafe.h>
#include <Dbt.h>
#include <Hidsdi.h>
#include "CEntradaHID.h"

CEntradaHID::CEntradaHID(CPerfil* perfil, CColaEventos* colaEv)
{
    procesarHID = new CPreprocesar(perfil, colaEv);
}

CEntradaHID::~CEntradaHID()
{
    if (pnpHdn != NULL) UnregisterDeviceNotification(pnpHdn);
    if (pnpHWnd != NULL) DestroyWindow(pnpHWnd);
    salir = true;
    CerrarDevices();
    delete[] rutaPedales; rutaPedales = NULL;
    delete[] rutaX52; rutaX52 = NULL;
    delete[] rutaNXT; rutaNXT = NULL;
    while (InterlockedCompareExchange16(&hiloCerrado[0], 0, 0) == FALSE) Sleep(1000);
    while (InterlockedCompareExchange16(&hiloCerrado[1], 0, 0) == FALSE) Sleep(1000);
    while (InterlockedCompareExchange16(&hiloCerrado[2], 0, 0) == FALSE) Sleep(1000);
    delete procesarHID;
}

bool CEntradaHID::Iniciar(HINSTANCE hInst)
{
    if (!procesarHID->Iniciar())
    {
        return false;
    }

    if (GetRutasConectados())
    {
        if (PnpNotification(hInst))
        {
            if(AbrirDevices())
            {
                HANDLE h = CreateThread(NULL, 0, HiloLectura, this, 0, NULL);
                if (h != NULL)
                {
                    while (InterlockedCompareExchange16(&hiloCerrado[0], FALSE, FALSE))
                    {
                        Sleep(500);
                    }
                    h = CreateThread(NULL, 0, HiloLectura, this, 0, NULL);
                    if (h != NULL)
                    {
                        while (InterlockedCompareExchange16(&hiloCerrado[1], FALSE, FALSE))
                        {
                            Sleep(500);
                        }
                        h = CreateThread(NULL, 0, HiloLectura, this, 0, NULL);
                        if (h != NULL)
                        {
                            while (InterlockedCompareExchange16(&hiloCerrado[2], FALSE, FALSE))
                            {
                                Sleep(500);
                            }
                            return true;
                        }
                    }
                }
            }
        }
    }

    return false;
}

bool CEntradaHID::GetRutasConectados()
{
    HDEVINFO                            diDevs = INVALID_HANDLE_VALUE;
    GUID                                hidGuid;
    SP_DEVICE_INTERFACE_DATA            diData;

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
            SetupDiDestroyDeviceInfoList(diDevs);
            return false;
        }
        
        if (CompararHardwareId(didData->DevicePath, HARDWARE_ID_PEDALES))
        {
            rutaPedales = new wchar_t[tam];
            StringCchCopy(rutaPedales, tam, didData->DevicePath);
        }
        if (CompararHardwareId(didData->DevicePath, HARDWARE_ID_X52))
        {
            rutaX52 = new wchar_t[tam];
            StringCchCopy(rutaX52, tam, didData->DevicePath);
        }
        if (CompararHardwareId(didData->DevicePath, HARDWARE_ID_NXT))
        {
            rutaNXT = new wchar_t[tam];
            StringCchCopy(rutaNXT, tam, didData->DevicePath);
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

bool CEntradaHID::CompararHardwareId(wchar_t* path, const wchar_t* hardwareId)
{
    int tam = lstrlen(path) + 1;
    wchar_t* cmps = new wchar_t[tam];
    StringCchCopy(cmps, tam, path);
    RtlZeroMemory(&cmps[lstrlen(hardwareId)], sizeof(wchar_t));
    bool ok = (lstrcmpi(cmps, hardwareId) == 0);
    delete[] cmps;
    return ok;
}

bool CEntradaHID::PnpNotification(HINSTANCE hInst)
{
    WNDCLASSEXW wcex;
    RtlZeroMemory(&wcex, sizeof(WNDCLASSEX));
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc = (WNDPROC)PnpMsjProc;
    wcex.hInstance = hInst;
    wcex.lpszClassName = L"xhotas pnp notification";

    if (RegisterClassExW(&wcex))
    {
        pnpHWnd = CreateWindowEx(0, wcex.lpszClassName, L"", 0, 0, 0, 0, 0, HWND_MESSAGE, 0, hInst, 0);
        if (pnpHWnd != NULL)
        {
            DEV_BROADCAST_DEVICEINTERFACE dbbd;
            RtlZeroMemory(&dbbd, sizeof(dbbd));
            dbbd.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);;
            dbbd.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
            HidD_GetHidGuid(&dbbd.dbcc_classguid);

            if (NULL != RegisterDeviceNotification(pnpHWnd, &dbbd, DEVICE_NOTIFY_WINDOW_HANDLE))
            {
                SetWindowLongPtr(pnpHWnd, GWLP_USERDATA, (LONG_PTR)this);
                return true;
            }
        }
    }

    return false;
}

LRESULT CALLBACK CEntradaHID::PnpMsjProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    CEntradaHID* local = (CEntradaHID*)GetWindowLongPtr(hwnd, GWLP_USERDATA);
    switch (uMsg)
    {
        case WM_DEVICECHANGE:
        {
            switch (wParam)
            {
                case DBT_DEVICEARRIVAL:
                    if (DBT_DEVTYP_DEVICEINTERFACE == ((PDEV_BROADCAST_HDR)lParam)->dbch_devicetype)
                    {
                        PDEV_BROADCAST_DEVICEINTERFACE dbdi = (PDEV_BROADCAST_DEVICEINTERFACE)lParam;
                        int tam = lstrlen(dbdi->dbcc_name) + 1;
                        if (local->rutaPedales == NULL)
                        {
                            if (local->CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_PEDALES))
                            {
                                local->rutaPedales = new wchar_t[tam];
                                StringCchCopy(local->rutaPedales, tam, dbdi->dbcc_name);
                            }
                        }
                        if (local->rutaX52 == NULL)
                        {
                            if (local->CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_X52))
                            {
                                local->rutaX52 = new wchar_t[tam];
                                StringCchCopy(local->rutaX52, tam, dbdi->dbcc_name);
                            }
                        }
                        if (local->rutaNXT == NULL)
                        {
                            if (local->CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_NXT))
                            {
                                local->rutaNXT = new wchar_t[tam];
                                StringCchCopy(local->rutaNXT, tam, dbdi->dbcc_name);
                            }
                        }
                        local->AbrirDevices();
                        return TRUE;
                    }
                    break;
                case DBT_DEVICEQUERYREMOVE:
                case DBT_DEVICEREMOVEPENDING:
                case DBT_DEVICEREMOVECOMPLETE:
                {
                    if (DBT_DEVTYP_DEVICEINTERFACE == ((PDEV_BROADCAST_HDR)lParam)->dbch_devicetype)
                    {
                        PDEV_BROADCAST_DEVICEINTERFACE dbdi = (PDEV_BROADCAST_DEVICEINTERFACE)lParam;
                        int tam = lstrlen(dbdi->dbcc_name) + 1;
                        if (local->rutaPedales != NULL)
                        {
                            if (local->CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_PEDALES))
                            {
                                delete[] local->rutaPedales;
                                local->rutaPedales = NULL;
                            }
                        }
                        if (local->rutaX52 != NULL)
                        {
                            if (local->CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_X52))
                            {
                                delete[] local->rutaX52;
                                local->rutaX52 = NULL;
                            }
                        }
                        if (local->rutaNXT != NULL)
                        {
                            if (local->CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_NXT))
                            {
                                delete[] local->rutaNXT;;
                                local->rutaNXT = NULL;

                            }
                        }
                        local->CerrarDevices();
                    }
                    return TRUE;
                }
                default:
                    break;
            }
            return 0;
        }
        case WM_DESTROY:
            PostQuitMessage(0);
            break;
        default:
            return DefWindowProc(hwnd, uMsg, wParam, lParam);
    }
    return 0;
}

void CEntradaHID::LoopWnd()
{
    MSG msg;
    BOOL ok = 1;
    do
    {
        ok = GetMessage(&msg, pnpHWnd, 0, 0);
    }
    while (ok != 0);
}

bool CEntradaHID::AbrirDevices()
{
    bool ok = true;
    if ((rutaPedales != NULL) && (InterlockedCompareExchangePointer(&hdevPedales, NULL, NULL) == NULL))
    {
        HANDLE hdev = CreateFile(rutaPedales, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
        if (INVALID_HANDLE_VALUE == hdev)
        {
            ok = false;
        }
        else
        {
            InterlockedExchangePointer(&hdevPedales, hdev);
        }
    }
    if ((rutaX52 != NULL) && (InterlockedCompareExchangePointer(&hdevX52, NULL, NULL) == NULL))
    {
        HANDLE hdev = CreateFile(rutaX52, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
        if (INVALID_HANDLE_VALUE == hdev)
        {
            ok = false;
        }
        else
        {
            InterlockedExchangePointer(&hdevX52, hdev);
        }
    }
    if ((rutaNXT != NULL) && (InterlockedCompareExchangePointer(&hdevNXT, NULL, NULL) == NULL))
    {
        HANDLE hdev = CreateFile(rutaNXT, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
        if (INVALID_HANDLE_VALUE == hdev)
        {
            ok = false;
        }
        else
        {
            InterlockedExchangePointer(&hdevNXT, hdev);
        }
    }

    return ok;
}

void CEntradaHID::CerrarDevices()
{
    if ((rutaPedales != NULL) && (InterlockedCompareExchangePointer(&hdevPedales, NULL, NULL) != NULL))
    {
        HANDLE h = InterlockedExchangePointer(&hdevPedales, NULL);
        CancelIoEx(h, NULL);
        CloseHandle(h);
    }
    if ((rutaX52 != NULL) && (InterlockedCompareExchangePointer(&hdevX52, NULL, NULL) != NULL))
    {
        HANDLE h = InterlockedExchangePointer(&hdevX52, NULL);
        CancelIoEx(h, NULL);
        CloseHandle(h);
    }
    if ((rutaNXT != NULL) && (InterlockedCompareExchangePointer(&hdevNXT, NULL, NULL) != NULL))
    {
        HANDLE h = InterlockedExchangePointer(&hdevNXT, NULL);
        CancelIoEx(h, NULL);
        CloseHandle(h);
    }
}

DWORD WINAPI CEntradaHID::HiloLectura(LPVOID param)
{
    CEntradaHID* local = (CEntradaHID*)param;
    PVOID* dev = NULL;
    UCHAR hilo = 0;
    if (InterlockedCompareExchange16(&local->hiloCerrado[0], FALSE, TRUE) == TRUE)
    {
        dev = &local->hdevPedales;
    }
    else if (InterlockedCompareExchange16(&local->hiloCerrado[1], FALSE, TRUE) == TRUE)
    {
        hilo = 1;
        dev = &local->hdevX52;
    }
    else
    {
        InterlockedExchange16(&local->hiloCerrado[2], FALSE);
        hilo = 2;
        dev = &local->hdevNXT;
    }

    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

    UCHAR buff[16];
    while (!local->salir)
    {   
        if (*dev == NULL)
        {
            Sleep(3000);
        }
        else
        {
            DWORD tam = 0;
            if (!ReadFile(*dev, buff, 16, &tam, NULL))
            {
                Sleep(1500);
            }
            else
            {
                local->procesarHID->AñadirACola(&buff[1], tam - 1);
            }
        }
    }

    InterlockedExchange16(&local->hiloCerrado[hilo], TRUE);
    return 0;
}