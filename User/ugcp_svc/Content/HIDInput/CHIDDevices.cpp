#include "../framework.h"
#include "CHIDDevices.h"
#include <SetupAPI.h>
#include "../NXT/HIDNXTWrite.h"

//constexpr auto HARDWARE_ID_NXT = L"\\\\?\\HID#VID_231d&PID_0200";
constexpr std::uint32_t HARDWARE_ID_NXT = 0x231d0200;

CHIDDevices::CHIDDevices(std::uint32_t hardwareId)
{
    this->hardwareId = hardwareId;
}

CHIDDevices::~CHIDDevices()
{
    Close(true);
}

bool CHIDDevices::Prepare()
{
    Close(false);

    HDEVINFO                            diDevs = INVALID_HANDLE_VALUE;
    GUID                                hidGuid;
    SP_DEVICE_INTERFACE_DATA            diData{};

    HidD_GetHidGuid(&hidGuid);
    diDevs = SetupDiGetClassDevsW(&hidGuid, NULL, NULL, (DIGCF_PRESENT | DIGCF_DEVICEINTERFACE));
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

        auto buf = std::make_unique<std::uint8_t[]>(size);
        memset(buf.get(), 0, size);
        PSP_DEVICE_INTERFACE_DETAIL_DATA didData = reinterpret_cast<PSP_DEVICE_INTERFACE_DETAIL_DATA>(buf.get());
        didData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA);
        if (!SetupDiGetDeviceInterfaceDetail(diDevs, &diData, didData, size, &size, NULL))
        {
            SetupDiDestroyDeviceInfoList(diDevs);
            return false;
        }

        UINT32 hId = IHIDInput::GetHardwareId(didData->DevicePath);
        {
            std::lock_guard<std::mutex> lock(mutex);
            if (hId == hardwareId)
            {
                pathInterface = didData->DevicePath;
                break;
            }
        }
    }

    SetupDiDestroyDeviceInfoList(diDevs);

    return GetLastError() == 0;
}

bool CHIDDevices::Open()
{
    std::lock_guard<std::mutex> lock(mutex);
    {
        if ((hardwareId != 0) && !pathInterface.empty() && !hdev.valid())
        {
            unique_handle nhdev(CreateFileW(pathInterface.c_str(), GENERIC_READ, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, 0, NULL));
            if (!nhdev.valid())
            {
                return false;
            }
            else
            {
                DWORD newReportLenght = 0;
                PHIDP_PREPARSED_DATA newPrepased = nullptr;
                if (HidD_GetPreparsedData(nhdev.get(), &newPrepased))
                {
                    HIDP_CAPS caps;
                    if (HidP_GetCaps(newPrepased, &caps) == HIDP_STATUS_SUCCESS)
                    {
                        if (GetDeviceMap(newPrepased, &caps))
                        {
                            newReportLenght = caps.InputReportByteLength;
                        }
                    }
                }
                if (newReportLenght != 0)
                {
                    reportLenght.store(newReportLenght);
                    reportBuffer = std::make_unique<CHAR[]>(newReportLenght);
                    preparsed = newPrepased;
                    hdev.set(nhdev.move());
                    if (hardwareId == HARDWARE_ID_NXT)
                    {
                        CNXTWrite::Get().SetPath(pathInterface);
                    }
                }
                else
                {
                    if (newPrepased != nullptr)
                    {
                        HidD_FreePreparsedData(reinterpret_cast<PHIDP_PREPARSED_DATA>(newPrepased));
                    }
                }
            }
        }
    }

    return true;
}

void CHIDDevices::Close(bool exit)
{
    std::lock_guard<std::mutex> lock(mutex);
    {
        unique_handle old(hdev.move());
        if (old.valid())
        {
            if (hardwareId == HARDWARE_ID_NXT)
            {
                CNXTWrite::Get().SetPath(L"");
            }
            CancelIoEx(old.get(), nullptr);
        }
    }

    reportBuffer.reset();
    reportLenght.store(0);
    if (preparsed != nullptr)
    {
        HidD_FreePreparsedData(reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed));
        preparsed = nullptr;
    }

    pathInterface.clear();

    map.clear();
    map.shrink_to_fit();

    if (exit)
    {
        hardwareId = 0;
    }
}

bool CHIDDevices::GetDeviceMap(PHIDP_PREPARSED_DATA pData, PHIDP_CAPS pCaps)
{
    auto bCaps =  std::make_unique<HIDP_BUTTON_CAPS[]>(pCaps->NumberInputButtonCaps);
    auto vCaps = std::make_unique<HIDP_VALUE_CAPS[]>(pCaps->NumberInputValueCaps);

    #pragma region GetCaps
    if (pCaps->NumberInputButtonCaps != 0)
    {
        USHORT ustam = pCaps->NumberInputButtonCaps;
        if (HidP_GetButtonCaps(HIDP_REPORT_TYPE::HidP_Input, bCaps.get(), &ustam, pData) != 0x110000)
        {
            return false;
        }
    }
    if (pCaps->NumberInputValueCaps != 0)
    {
        USHORT ustam = pCaps->NumberInputValueCaps;
        if (HidP_GetValueCaps(HIDP_REPORT_TYPE::HidP_Input, vCaps.get(), &ustam, pData) != 0x110000)
        {
            return false;
        }
    }
    #pragma endregion

    std::uint8_t btId = 0;
    std::uint8_t axId = 0;
    std::uint8_t hatId = 0;
    for (USHORT idx = 0; idx < pCaps->NumberInputDataIndices; idx++)
    {
        for(USHORT i = 0; i < pCaps->NumberInputButtonCaps; i++)
        {
            HIDP_BUTTON_CAPS bt = bCaps.get()[i];
            if (bt.Range.DataIndexMin == idx)
            {
                ST_MAP bmap;
                //bmap->ReportId = bt.ReportID;
                bmap.Bits = bt.Range.DataIndexMax - bt.Range.DataIndexMin + 1;
                bmap.IsButton = TRUE;
                bmap.IsHat = FALSE;
                bmap.Index = bt.Range.DataIndexMin;
                //bmap->Skip = FALSE;
                map.push_back(bmap);
                btId++;
                if (btId == 128) { return false; }
                break;
            }
        }

        for(USHORT i = 0; i < pCaps->NumberInputValueCaps; i++)
        {
            HIDP_VALUE_CAPS val = vCaps.get()[i];
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
                    ST_MAP amap;
                    //amap->ReportId = val.ReportID;
                    amap.Bits = static_cast<std::uint8_t>(val.BitSize);
                    amap.IsButton = FALSE;
                    amap.IsHat = (val.NotRange.Usage == 57) ? static_cast<std::uint8_t>(val.LogicalMin) | ((static_cast<std::uint8_t>(val.LogicalMax) & 0xf) << 4) : 0;
                    amap.Index = val.NotRange.DataIndex;
                    //amap->Skip = FALSE;
                    map.push_back(amap);
                    if (!amap.IsHat) { axId++; } else { hatId++; }
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
    if (!hdev.valid())
    {
        std::this_thread::sleep_for(std::chrono::milliseconds(3000));
        bool prepared;
        bool exit;
        {
            std::lock_guard<std::mutex> lock(mutex);
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
    }
    else
    {
        if (reportLenght.load() != 0)
        {
            DWORD readSize = 0;
            if (ReadFile(hdev.get(), reportBuffer.get(), reportLenght.load(), &readSize, nullptr))
            {
                ULONG size = 0;
                if (HidP_GetData(HidP_Input, NULL, &size, reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), reportBuffer.get(), reportLenght.load()) == HIDP_STATUS_BUFFER_TOO_SMALL)
                {
                    if (HidP_GetData(HidP_Input, reinterpret_cast<PHIDP_DATA>(buff), &size, reinterpret_cast<PHIDP_PREPARSED_DATA>(preparsed), reportBuffer.get(), reportLenght) == HIDP_STATUS_SUCCESS)
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
