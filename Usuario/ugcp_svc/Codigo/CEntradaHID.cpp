#include "framework.h"
#include <Dbt.h>
#include <Hidsdi.h>
#include "CEntradaHID.h"
#include "IEntradaHID.h"
//#include "CalibradoDx/CDirectInput.h"

CEntradaHID::CEntradaHID(CPerfil* perfil, CColaEventos* colaEv)
{
    procesarHID = new CPreprocesar(perfil, colaEv);
    pedales = new CPedalesEntrada();
    x52 = new CX52Entrada();
    nxt = new CNXTEntrada();
}

CEntradaHID::~CEntradaHID()
{
    if (pnpHdn != NULL) UnregisterDeviceNotification(pnpHdn);
    if (pnpHWnd != NULL) DestroyWindow(pnpHWnd);
    salir = true;

    delete pedales;
    delete x52;
    delete nxt;
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
            }
        }
    }
    if (h == NULL)
    {
        return false;
    }

    if (pedales->Preparar() && x52->Preparar() && nxt->Preparar())
    {
        if (PnpNotification(hInst))
        {
            if(pedales->Abrir() && x52->Abrir() && nxt->Abrir())
            {
                return true;
            }
        }
    }

    return false;
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
            //HidD_GetHidGuid(&dbbd.dbcc_classguid);

            if (NULL != RegisterDeviceNotification(pnpHWnd, &dbbd, DEVICE_NOTIFY_WINDOW_HANDLE | DEVICE_NOTIFY_ALL_INTERFACE_CLASSES))
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
                        if (IEntradaHID::CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_PEDALES))
                        {
                            local->pedales->Preparar();
                            local->pedales->Abrir();
                        }
                        else if (IEntradaHID::CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_X52))
                        {
                            local->x52->Preparar();
                            local->x52->Abrir();
                        }
                        else if (IEntradaHID::CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_NXT))
                        {
                            local->nxt->Preparar();
                            local->nxt->Abrir();
                        }

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
                        if (IEntradaHID::CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_PEDALES))
                        {
                            local->pedales->Cerrar();
                        }
                        else if (IEntradaHID::CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_X52))
                        {
                            local->x52->Cerrar();
                        }
                        else if (IEntradaHID::CompararHardwareId(dbdi->dbcc_name, HARDWARE_ID_NXT))
                        {
                            local->nxt->Cerrar();
                        }
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
        ok = GetMessage(&msg, 0, 0, 0);
    }
    while (ok != 0);
}

DWORD WINAPI CEntradaHID::HiloLectura(LPVOID param)
{
    CEntradaHID* local = (CEntradaHID*)param;
    IEntradaHID* dev = NULL;
    UCHAR hilo = 0;
    if (InterlockedCompareExchange16(&local->hiloCerrado[0], FALSE, TRUE) == TRUE)
    {
        dev = local->pedales;
    }
    else if (InterlockedCompareExchange16(&local->hiloCerrado[1], FALSE, TRUE) == TRUE)
    {
        hilo = 1;
        dev = local->x52;
    }
    else
    {
        InterlockedExchange16(&local->hiloCerrado[2], FALSE);
        hilo = 2;
        dev = local->nxt;
    }

    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

    UCHAR buff[64];
    while (!local->salir)
    {   
        unsigned short tam = dev->Leer(buff);
        if (tam > 0)
        {
            local->procesarHID->AñadirACola(buff, tam);
        }
    }

    InterlockedExchange16(&local->hiloCerrado[hilo], TRUE);
    return 0;
}