#include "../framework.h"
#include "CHIDDevices.h"
#include <SetupAPI.h>
#include <hidsdi.h>
#include "../NXT/HIDNXTWrite.h"

//constexpr auto HARDWARE_ID_NXT = L"\\\\?\\HID#VID_231d&PID_0200";
constexpr UINT32 HARDWARE_ID_NXT = 0x231d0200;

CHIDDevices::CHIDDevices(UINT32 hardwareId)
{
    mutex = CreateSemaphore(NULL, 1, 1, NULL);
    this->hardwareId = hardwareId;
}

CHIDDevices::~CHIDDevices()
{
    Close(true);
    CloseHandle(mutex);
}

bool CHIDDevices::Prepare()
{
    Close(false);

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
        DWORD size = 0;
        SetupDiGetDeviceInterfaceDetail(diDevs, &diData, NULL, 0, &size, NULL);

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

        UINT32 hId = IHIDInput::GetHardwareId(didData->DevicePath);
        WaitForSingleObject(mutex, INFINITE);
        if (hId == hardwareId)
        {
            {
                pathInterface = new wchar_t[size];
                StringCchCopy(pathInterface, size, didData->DevicePath);
            }
            ReleaseSemaphore(mutex, 1, NULL);
            delete[] buf;
            break;
        }

        ReleaseSemaphore(mutex, 1, NULL);
        delete[] buf;
    }
    if (GetLastError() != 0)
    {
        SetupDiDestroyDeviceInfoList(diDevs);
        return false;
    }

    SetupDiDestroyDeviceInfoList(diDevs);

    return true;
}

bool CHIDDevices::Open()
{
    WaitForSingleObject(mutex, INFINITE);
    {
        if ((hardwareId != 0) && (pathInterface != nullptr) && (InterlockedCompareExchangePointer(&hdev, nullptr, nullptr) == nullptr))
        {
            PVOID nhdev = CreateFile(pathInterface, GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_EXISTING, 0, NULL);
            if (INVALID_HANDLE_VALUE == nhdev)
            {
                ReleaseSemaphore(mutex, 1, NULL);
                return false;
            }
            else
            {
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
                    InterlockedExchangePointer(&hdev, nhdev);
                    if (hardwareId == HARDWARE_ID_NXT)
                    {
                        CNXTWrite::Get()->SetPath(pathInterface);
                    }
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
                    CloseHandle(nhdev);
                }
            }
        }
    }
    ReleaseSemaphore(mutex, 1, NULL);

    return true;
}

void CHIDDevices::Close(bool exit)
{
    WaitForSingleObject(mutex, INFINITE);
    {
        if (InterlockedCompareExchangePointer(&hdev, nullptr, nullptr) != nullptr)
        {
            if (hardwareId == HARDWARE_ID_NXT)
            {
                CNXTWrite::Get()->SetPath(nullptr);
            }
            HANDLE h = InterlockedExchangePointer(&hdev, nullptr);
            CancelIoEx(h, NULL);
            CloseHandle(h);
        }
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

        delete[] pathInterface; pathInterface = nullptr;
    }

    while (map.size() > 0)
    {
        delete map.back();
        map.pop_back();
    }

    if (exit)
    {
        hardwareId = 0;
    }
    ReleaseSemaphore(mutex, 1, NULL);

}

bool CHIDDevices::GetDeviceMap(void* ppd, void* pc)
{
    PHIDP_CAPS pCaps = static_cast<PHIDP_CAPS>(pc);
    PHIDP_PREPARSED_DATA pData = static_cast<PHIDP_PREPARSED_DATA>(ppd);
    HIDP_BUTTON_CAPS* bCaps = new HIDP_BUTTON_CAPS[pCaps->NumberInputButtonCaps];
    HIDP_VALUE_CAPS* vCaps = new HIDP_VALUE_CAPS[pCaps->NumberInputValueCaps];

    #pragma region GetCaps
    if (pCaps->NumberInputButtonCaps != 0)
    {
        USHORT ustam = pCaps->NumberInputButtonCaps;
        if (HidP_GetButtonCaps(HIDP_REPORT_TYPE::HidP_Input, bCaps, &ustam, pData) != 0x110000)
        {
            delete[] bCaps;
            delete[] vCaps;
            return false;
        }
    }
    if (pCaps->NumberInputValueCaps != 0)
    {
        USHORT ustam = pCaps->NumberInputValueCaps;
        if (HidP_GetValueCaps(HIDP_REPORT_TYPE::HidP_Input, vCaps, &ustam, pData) != 0x110000)
        {
            delete[] bCaps;
            delete[] vCaps;
            return false;
        }
    }
    #pragma endregion

    UCHAR btId = 0;
    UCHAR axId = 0;
    UCHAR hatId = 0;
    for (USHORT idx = 0; idx < pCaps->NumberInputDataIndices; idx++)
    {
        for(USHORT i = 0; i < pCaps->NumberInputButtonCaps; i++)
        {
            HIDP_BUTTON_CAPS bt = bCaps[i];
            if (bt.Range.DataIndexMin == idx)
            {
                ST_MAP* bmap = new ST_MAP;
                //bmap->ReportId = bt.ReportID;
                bmap->Bits = bt.Range.DataIndexMax - bt.Range.DataIndexMin + 1;
                bmap->IsButton = TRUE;
                bmap->IsHat = FALSE;
                bmap->Index = bt.Range.DataIndexMin;
                //bmap->Skip = FALSE;
                map.push_back(bmap);
                btId++;
                if (btId == 128) { return false; }
                break;
            }
        }

        for(USHORT i = 0; i < pCaps->NumberInputValueCaps; i++)
        {
            HIDP_VALUE_CAPS val = vCaps[i];
            if (val.NotRange.DataIndex == idx)
            {
                if ((val.NotRange.Usage == 0) || (val.NotRange.Usage == 1))
                {
                    //ST_MAP* amap = new ST_MAP;
                    //amap->ReportId = val.ReportID;
                    //amap->Bits = static_cast<UCHAR>(val.BitSize);
                    //amap->IsButton = FALSE;
                    //amap->IsHat = FALSE;
                    //amap->Index = val.NotRange.DataIndex;
                    //amap->Skip = TRUE;
                    //map.push_back(amap);
                }
                else
                {
                    if ((val.NotRange.Usage != 57) && (val.LogicalMin != 0)) { return false; }
                    if ((val.NotRange.Usage == 57) && (val.LogicalMax + 1 - val.LogicalMin) != 8 && (val.LogicalMax + 1 - val.LogicalMin) != 4) { return false; }
                    ST_MAP* amap = new ST_MAP;
                    //amap->ReportId = val.ReportID;
                    amap->Bits = static_cast<UCHAR>(val.BitSize);
                    amap->IsButton = FALSE;
                    amap->IsHat = (val.NotRange.Usage == 57) ? static_cast<UCHAR>(val.LogicalMin) | ((static_cast<UCHAR>(val.LogicalMax) & 0xf) << 4) : 0;
                    amap->Index = val.NotRange.DataIndex;
                    //amap->Skip = FALSE;
                    map.push_back(amap);
                    if (!amap->IsHat) { axId++; } else { hatId++; }
                    if ((axId == 24) || (hatId == 4)) { return false; }
                }
                break;
            }
        }
    }
    return true;
}

unsigned short CHIDDevices::Read(void* buff)
{
    if (InterlockedCompareExchangePointer(&hdev, nullptr, nullptr) == nullptr)
    {
        Sleep(3000);
        WaitForSingleObject(mutex, INFINITE);
        bool prepared = (pathInterface != nullptr);
        bool exit = (hardwareId == 0);
        ReleaseSemaphore(mutex, 1, NULL);
        if (exit)
        {
            return 0;
        }
        if (!prepared)
        {
            Prepare();
        }
        Open();
    }
    else
    {
        if (InterlockedCompareExchange(&reportLenght, 0, 0) != 0)
        {
            DWORD readSize = 0;
            if (ReadFile(hdev, reportBuffer, reportLenght, &readSize, NULL))
            {
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
