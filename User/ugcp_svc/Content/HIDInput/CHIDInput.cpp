#include "../framework.h"
#include <Dbt.h>
#include <Hidsdi.h>
#include "CHIDInput.h"
#include "IHIDInput.h"
#include "../X52/CWinUSBX52.h"
//#include "CalibradoDx/CDirectInput.h"

CHIDInput::CHIDInput(CProfile* profile, CEventQueue* evQueue)
{
    processHID = new CPreprocess(profile, evQueue, this, &LockDevices, &UnlockDevices, &GetDevice);
    pProfile = profile;
    profile->SetRefreshDevicesCallback(this, &CallbackRefreshDevices);
    profile->SetPauseWinUSBCallback(&CallbackPauseWinUSB);
    mutexDevices = CreateSemaphore(NULL, 1, 1, NULL);
}

CHIDInput::~CHIDInput()
{
    if (pnpHdn != NULL) UnregisterDeviceNotification(pnpHdn);
    if (pnpHWnd != NULL) DestroyWindow(pnpHWnd);

    for (auto& ex : exit)
    {
        ex.second = true;
    }

    LockDevices(this);
    while (hidDevices.size() > 0)
    {
        delete hidDevices.begin()->second;
        hidDevices.erase(hidDevices.begin()->first);
    }
    UnlockDevices(this);

    while (threadClosed.size() > 0)
    {
        if (InterlockedCompareExchange16(&threadClosed.begin()->second, FALSE, FALSE) == FALSE)
        {
            Sleep(1000);
        }
        else
        {
            threadClosed.erase(threadClosed.begin()->first);
        }
    }

    exit.clear();

    delete processHID;
    CloseHandle(mutexDevices);
}

bool CHIDInput::Init(HINSTANCE hInst)
{
    if (!processHID->Init())
    {
        return false;
    }

    return PnpNotification(hInst);
}

void CHIDInput::RefreshDevices(UINT32* ids, UCHAR size)
{
    LockDevices(this);

    for (UCHAR i = 0; i < size; i++)
    {
        if (hidDevices.find(ids[i]) == hidDevices.end())
        {
            if (ids[i] == HARDWARE_ID_X52)
            {
                hidDevices.insert({ ids[i], static_cast<CHIDDevices*>(new CWinUSBX52()) });
            }
            else
            {
                hidDevices.insert({ ids[i], new CHIDDevices(ids[i]) });
            }
            threadClosed.insert({ ids[i], 1 });
            exit.insert({ ids[i], false });
            ST_THREAD_PARAMS params{ this, ids[i]};
            HANDLE h = CreateThread(NULL, 0, ThreadRead, &params, 0, NULL);
            if (h != NULL)
            {
                while (InterlockedCompareExchange16(&threadClosed.at(ids[i]), 0, 0) == 1)
                {
                    Sleep(500);
                }
                if (hidDevices.at(ids[i])->Prepare())
                {
                    hidDevices.at(ids[i])->Open();
                }
            }
            else
            {
                threadClosed.erase(ids[i]);
            }
        }
    }

    std::deque<UINT32> notfound;
    for (auto const& cii : hidDevices)
    {
        bool found = false;
        for (UCHAR i = 0; i < size; i++)
        {
            if (cii.first == ids[i]) { found = true; break; }
        }
        if (!found)
        {
            notfound.push_back(cii.first);
        }
    }
    while (!notfound.empty())
    {
        exit.at(notfound.back()) = true;
        while (true)
        {
            if (InterlockedCompareExchange16(&threadClosed.at(notfound.back()), FALSE, FALSE) == FALSE)
            {
                Sleep(1000);
            }
            else
            {
                threadClosed.erase(notfound.back());
                break;
            }
        }

        delete hidDevices.at(notfound.back());
        hidDevices.erase(notfound.back());
        exit.erase(notfound.back());
        notfound.pop_back();
    }

    UnlockDevices(this);
}

void CHIDInput::PauseWinUSB(bool onoff)
{
    LockDevices(this);
    CWinUSBX52* pX52 = dynamic_cast<CWinUSBX52*>(GetDevice(this, HARDWARE_ID_X52));
    if (pX52 != nullptr)
    {
        if (onoff)
        {
            pX52->SetPause(onoff);
        }
    }
    UnlockDevices(this);
}

bool CHIDInput::PnpNotification(HINSTANCE hInst)
{
    WNDCLASSEXW wcex;
    RtlZeroMemory(&wcex, sizeof(WNDCLASSEX));
    wcex.cbSize = sizeof(WNDCLASSEX);
    wcex.lpfnWndProc = (WNDPROC)PnpMsjProc;
    wcex.hInstance = hInst;
    wcex.lpszClassName = L"UGCP pnp notification";

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

LRESULT CALLBACK CHIDInput::PnpMsjProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    CHIDInput* local = (CHIDInput*)GetWindowLongPtr(hwnd, GWLP_USERDATA);
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
                        UINT32 joyId = IHIDInput::GetHardwareId(dbdi->dbcc_name);
                        if (local->pProfile->GetProfile()->DeviceIncluded(joyId))
                        {
                            local->RefreshDevices(&joyId, 1);
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
                        UINT32 joyId = IHIDInput::GetHardwareId(dbdi->dbcc_name);
                        if (local->exit.find(joyId) != local->exit.end())
                        {
                            local->exit.at(joyId) = true;
                            while (true)
                            {
                                if (InterlockedCompareExchange16(&local->threadClosed.at(joyId), FALSE, FALSE) == FALSE)
                                {
                                    Sleep(1000);
                                }
                                else
                                {
                                    local->threadClosed.erase(joyId);
                                    break;
                                }
                            }
                            local->LockDevices(local);
                            delete local->hidDevices.at(joyId);
                            local->hidDevices.erase(joyId);
                            local->exit.erase(joyId);
                            local->UnlockDevices(local);
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

void CHIDInput::LoopWnd()
{
    MSG msg;
    BOOL ok = 1;
    do
    {
        ok = GetMessage(&msg, 0, 0, 0);
    }
    while (ok != 0);
}

DWORD WINAPI CHIDInput::ThreadRead(LPVOID param)
{
    ST_THREAD_PARAMS* thParams = (ST_THREAD_PARAMS*)param;
    CHIDInput* local = thParams->Parent;
    UINT32 joyId = thParams->joyId;

    IHIDInput* dev = local->hidDevices.at(joyId); //devices already locked at createthread
    InterlockedExchange16(&local->threadClosed.at(joyId), FALSE);

    SetThreadPriority(GetCurrentThread(), THREAD_PRIORITY_TIME_CRITICAL);

    UCHAR buff[256]{};
    RtlCopyMemory(buff, &joyId, sizeof(UINT32));
    while (!local->exit.at(joyId))
    {  
        unsigned short tam = dev->Read(&buff[sizeof(UINT32)]);
        if (tam > 0)
        {
            local->processHID->AddToQueue(buff, tam + sizeof(UINT32));
        }
    }

    InterlockedExchange16(&local->threadClosed.at(joyId), TRUE);
    return 0;
}