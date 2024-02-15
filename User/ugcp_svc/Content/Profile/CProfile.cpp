#include "../framework.h"
#include <setupapi.h>
#include <initguid.h>
#include "CProfile.h"
#include "../EventQueue/CEventQueue.h"
#include "../ProcessOutput/CProcessOutput.h"
#include "../X52/USBX52Write.h"
#include "../X52/MFDMenu.h"
#include "../ProcessOutput/CVirtualHID.h"

static const UCHAR vJoyReport[142] = {
	0x05, 0x01,        // Usage Page (Generic Desktop Ctrls)
	0x15, 0x00,        // Logical Minimum (0)
	0x09, 0x04,        // Usage (Joystick)
	0xA1, 0x01,        // Collection (Application)
	0x05, 0x01,        //   Usage Page (Generic Desktop Ctrls)
	0x85, 0x01,        //   Report ID (1)
	0x09, 0x01,        //   Usage (Pointer)
	0x15, 0x00,        //   Logical Minimum (0)
	0x75, 0x20,        //   Report Size (32)
	0x95, 0x01,        //   Report Count (1)
	0xA1, 0x00,        //   Collection (Physical)
	0x09, 0x30,        //     Usage (X)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x31,        //     Usage (Y)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x32,        //     Usage (Z)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x33,        //     Usage (Rx)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x34,        //     Usage (Ry)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x35,        //     Usage (Rz)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x36,        //     Usage (Slider)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x37,        //     Usage (Dial)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0xC0,              //   End Collection
	0x15, 0x00,        //   Logical Minimum (0)
	0x27, 0x3C, 0x8C, 0x00, 0x00,  //   Logical Maximum (35899)
	0x35, 0x00,        //   Physical Minimum (0)
	0x47, 0x3C, 0x8C, 0x00, 0x00,  //   Physical Maximum (35899)
	0x65, 0x14,        //   Unit (System: English Rotation, Length: Centimeter)
	0x75, 0x20,        //   Report Size (32)
	0x95, 0x01,        //   Report Count (1)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x05, 0x09,        //   Usage Page (Button)
	0x15, 0x00,        //   Logical Minimum (0)
	0x25, 0x01,        //   Logical Maximum (1)
	0x55, 0x00,        //   Unit Exponent (0)
	0x65, 0x00,        //   Unit (None)
	0x19, 0x01,        //   Usage Minimum (0x01)
	0x29, 0x20,        //   Usage Maximum (0x20)
	0x75, 0x01,        //   Report Size (1)
	0x95, 0x20,        //   Report Count (32)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x75, 0x60,        //   Report Size (96)
	0x95, 0x01,        //   Report Count (1)
	0x81, 0x01,        //   Input (Const,Array,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0xC0,              // End Collection
};

CProfile::CProfile(void* pVHID)
{
	this->pVHID = pVHID;
	hMutexProgram = CreateMutex(NULL, FALSE, NULL);
	hMutexCalibration = CreateMutex(NULL, FALSE, NULL);
	hMutexStatus = CreateMutex(NULL, FALSE, NULL);
	status.Mode = 0;
	status.SubMode = 0;
	profile.MouseTick = 10;
	rawMode = FALSE;
	calibrationMode = FALSE;
}

CProfile::~CProfile()
{
	WaitForSingleObject(hMutexCalibration, INFINITE);
	calibration.Limits.clear();
	calibration.Jitters.clear();
	CloseHandle(hMutexCalibration);
	ClearProfile();
	CloseHandle(hMutexProgram);
	CloseHandle(hMutexStatus);
}

void CProfile::ClearProfile()
{
	WaitForSingleObject(hMutexProgram, INFINITE);
	{
		if (CEventQueue::Get() != nullptr)
		{
			CEventQueue::Get()->Clear();
		}
		if (CProcessOutput::Get() != nullptr)
		{
			CProcessOutput::Get()->ClearEvents();
		}

		if (profile.Actions != nullptr)
		{
			while (!profile.Actions->empty())
			{
				delete profile.Actions->front();
				profile.Actions->pop_front();
			}
			delete profile.Actions;
			profile.Actions = nullptr;
		}

		profile.Clear();

		LockStatus();
		{
			status.Clear();
		}
		UnlockStatus();
		newProfileIn = true;
		newProfileOut = true;
	}
	ReleaseMutex(hMutexProgram);
}

bool CProfile::HF_IoWriteMap(BYTE* SystemBuffer, DWORD size)
{
	if (!resetComanmds)
	{
		return false;
	}
	resetComanmds = false;
	if (size < (17 + 1 + 2)) //txt, mouse, button sz, axes sz
	{
		ClearProfile();
		return false;
	}
	else
	{
		BYTE txt[17];
		RtlCopyMemory(txt, SystemBuffer, 17);
		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(txt, 17);
		RtlZeroMemory(txt, 17);
		txt[0] = 2;
		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(txt, 2);
		txt[0] = 3;
		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(txt, 2);
	}

	std::unordered_map<UINT32, char> ids;

	WaitForSingleObject(hMutexProgram, INFINITE);
	{
		BYTE* ptr = SystemBuffer + 17;
		RtlCopyMemory(&profile.MouseTick, ptr++ , 1);

		BYTE njoys = *ptr++;
		for (BYTE j = 0; j < *(SystemBuffer + 17 + 1); j++)
		{
			UINT32 joyId = *((UINT32*)ptr);
			ids.insert({ joyId, 0 });
			ptr += sizeof(UINT32);			
			for (BYTE nModes = *ptr++; nModes > 0; nModes--)
			{
				BYTE mode = *ptr++;
				for (BYTE nButtons = *ptr++; nButtons > 0; nButtons--)
				{
					BYTE buttonId = *ptr++;
					std::vector<UINT16>* pActions = profile.ButtonsMap.NewButton(joyId, mode, buttonId, *ptr++);
					status.Buttons.NewStatus(joyId, mode, buttonId);

					for (BYTE nActions = *ptr++; nActions > 0; nActions--)
					{
						pActions->push_back(*((UINT16*)ptr));
						ptr += sizeof(UINT16);
					}
				}
			}
		}
		njoys = *ptr++;
		for (BYTE j = 0; j < njoys; j++)
		{
			UINT32 joyId = *((UINT32*)ptr);
			ids.insert({joyId, 0});
			ptr += sizeof(UINT32);
			for (BYTE nModes = *ptr++; nModes > 0; nModes--)
			{
				BYTE mode = *ptr++;
				for (BYTE nAxes = *ptr++; nAxes > 0; nAxes--)
				{
					BYTE axisKey = *ptr++;
					PROGRAMMING::AXISMODEL* axis = profile.AxesMap.NewAxis(joyId, mode, axisKey);
					status.Axes.NewStatus(joyId, mode, axisKey);

					axis->MouseSensibility = *ptr++;
					axis->VJoyOutput = *ptr++;
					axis->Type = *ptr++;
					axis->OutputAxis = *ptr++;
					RtlCopyMemory(&axis->Sensibility, ptr, 10);
					ptr += 10;
					axis->IsSlider = *ptr++;
					for (BYTE nBands = *ptr++; nBands > 0; nBands--)
					{
						axis->Bands.push_back(*ptr++);
					}
					for (BYTE nActions = *ptr++; nActions > 0; nActions--)
					{
						axis->Actions.push_back(*((UINT16*)ptr));
						ptr += sizeof(UINT16);
					}
					axis->ToughnessInc = *ptr++;
					axis->ToughnessDec = *ptr++;
				}
			}
		}
	}
	ReleaseMutex(hMutexProgram);

	UINT32* newDevices = new UINT32[ids.size()];
	UCHAR i = 0;
	while (ids.size() > 0)
	{
#pragma warning(disable:6386)
		newDevices[i++] = ids.begin()->first;
#pragma warning(default:6386)
		ids.erase(ids.begin()->first);
	}
	if (RefreshHidInputDevice != nullptr)
	{
		RefreshHidInputDevice(hidInput, newDevices, i);
	}
	delete[] newDevices;

	return HIDvJoyRange();
}

bool CProfile::HF_IoWriteCommands(BYTE* SystemBuffer, DWORD InputBufferLength)
{
	BYTE* bufIn;
	size_t sizePredicted = 0;

	ClearProfile();

	if (InputBufferLength != 0)
	{
		//Check OK
		bufIn = SystemBuffer;
		while (sizePredicted < InputBufferLength)
		{
			sizePredicted += (static_cast<size_t>(*bufIn) * 3) + 1;
			bufIn += (static_cast<size_t>(*bufIn) * 3) + 1;
		}
		if (sizePredicted != InputBufferLength)
		{
			return false;
		}
	}

	resetComanmds = true;

	if (CMFDMenu::Get() != nullptr) CMFDMenu::Get()->SetHourActivated(true);
	if (CMFDMenu::Get() != nullptr) CMFDMenu::Get()->SetDateActivated(true);

	if (InputBufferLength == 0)
	{
		return true;
	}

	WaitForSingleObject(hMutexProgram, INFINITE);
	{
		profile.Actions = new std::deque<CEventPacket*>();
		bufIn = SystemBuffer;
		sizePredicted = 0;
		while (sizePredicted < InputBufferLength)
		{
			CEventPacket* commandQueue = new CEventPacket();
			UCHAR actionSize = *bufIn;
			UCHAR i = 0;

			bufIn++;
			sizePredicted++;

			for (i = 0; i < actionSize; i++)
			{
				PEV_COMMAND mem = new EV_COMMAND;
				RtlZeroMemory(mem, sizeof(EV_COMMAND));
				mem->Type = *bufIn;
				mem->Basic.Data = *(bufIn + 1);
				mem->Basic.OutputJoy = *(bufIn + 2);
				if ((((PEV_COMMAND)bufIn)->Type == CommandType::X52MfdHour) || (((PEV_COMMAND)bufIn)->Type == CommandType::X52MfdHour24))
				{
					if (CMFDMenu::Get() != nullptr) CMFDMenu::Get()->SetHourActivated(false);
				}
				else if (((PEV_COMMAND)bufIn)->Type == CommandType::MfdDate)
				{
					if (CMFDMenu::Get() != nullptr) CMFDMenu::Get()->SetDateActivated(false);
				}
				bufIn += 3;
				sizePredicted += 3;
				commandQueue->AddCommand(mem);
			}
			profile.Actions->push_back(commandQueue);
		}
	}
	ReleaseMutex(hMutexProgram);

	return true;
}

void CProfile::WriteAntivibration(BYTE* datos)
{
	WaitForSingleObject(hMutexCalibration, INFINITE);
	calibration.Jitters.clear();

	BYTE* ptr = datos;
	ptr += sizeof(UINT32);
	for (UINT32 njoys = 0; njoys < *reinterpret_cast<UINT32*>(datos); njoys++)
	{
		calibration.New.insert({ *reinterpret_cast<UINT32*>(ptr), true });
		calibration.Jitters.insert({ *reinterpret_cast<UINT32*>(ptr), std::vector<CALIBRATION::ST_JITTER>()});
		std::vector<CALIBRATION::ST_JITTER>* vector = &calibration.Jitters.at(*reinterpret_cast<UINT32*>(ptr));
		ptr += sizeof(UINT32);
		BYTE naxes = *ptr++;
		for (BYTE njitters = 0; njitters < naxes; njitters++)
		{
			vector->push_back(CALIBRATION::ST_JITTER());
			RtlCopyMemory(&vector->at(njitters), ptr, sizeof(CALIBRATION::ST_JITTER));
			ptr += sizeof(CALIBRATION::ST_JITTER);
		}
	}
	ReleaseMutex(hMutexCalibration);
}

void CProfile::WriteCalibration(BYTE* datos)
{
	WaitForSingleObject(hMutexCalibration, INFINITE);
	calibration.Limits.clear();

	BYTE* ptr = datos;
	ptr += sizeof(UINT32);
	for (UINT32 njoys = 0; njoys < *reinterpret_cast<UINT32*>(datos); njoys++)
	{
		calibration.New.insert({ *reinterpret_cast<UINT32*>(ptr), true });
		calibration.Limits.insert({ *reinterpret_cast<UINT32*>(ptr), std::vector<CALIBRATION::ST_LIMITS>()});
		std::vector<CALIBRATION::ST_LIMITS>* vector = &calibration.Limits.at(*reinterpret_cast<UINT32*>(ptr));
		ptr += sizeof(UINT32);
		BYTE naxes = *ptr++;
		for (BYTE nlimits = 0; nlimits < naxes; nlimits++)
		{
			vector->push_back(CALIBRATION::ST_LIMITS());
			RtlCopyMemory(&vector->at(nlimits), ptr, sizeof(CALIBRATION::ST_LIMITS));
			vector->back().Range = vector->back().Right;
			ptr += sizeof(CALIBRATION::ST_LIMITS);
		}
	}
	ReleaseMutex(hMutexCalibration);
}

bool CProfile::HIDvJoyRange()
{
	//USHORT ranges[3][15];
	//bool used[3][15];

	//ZeroMemory(ranges, sizeof(ranges));
	//ZeroMemory(used, sizeof(used));

	//for (char i = 0; i < 4; i++)
	//{
	//	for (char j = 0; j < 8; j++)
	//	{
	//		for (char m = 0; m < 3; m++)
	//		{
	//			for (char p = 0; p < 2; p++)
	//			{
	//				if ((profile.MapaEjes[i][p][m][j].TipoEje & 7) && !used[profile.MapaEjes[i][p][m][j].JoySalida][profile.MapaEjes[i][p][m][j].Eje])
	//				{
	//					used[profile.MapaEjes[i][p][m][j].JoySalida][profile.MapaEjes[i][p][m][j].Eje] = true;
	//					ranges[profile.MapaEjes[i][p][m][j].JoySalida][profile.MapaEjes[i][p][m][j].Eje] = calibration.Limits[i][j].Range;
	//				}
	//			}
	//		}
	//	}
	//}

	//BYTE* buf = new BYTE[142];
	//DWORD size = 142;
	//for (char i = 1; i < 4; i++)
	//{
	//	wchar_t dev[] = L"SYSTEM\\CurrentControlSet\\Services\\vjoy\\Parameters\\Device01";
	//	_itow_s(i, &dev[57], 2, 10);
	//	memcpy(buf, vJoyReport, 142);
	//	buf[11] = i;
	//	*((USHORT*)&buf[25]) = ranges[i - 1][0] == 0 ? 32767 : ranges[i - 1][0];
	//	*((USHORT*)&buf[32]) = ranges[i - 1][1] == 0 ? 32767 : ranges[i - 1][1];
	//	*((USHORT*)&buf[39]) = ranges[i - 1][2] == 0 ? 32767 : ranges[i - 1][2];
	//	*((USHORT*)&buf[46]) = ranges[i - 1][3] == 0 ? 32767 : ranges[i - 1][3];
	//	*((USHORT*)&buf[53]) = ranges[i - 1][4] == 0 ? 32767 : ranges[i - 1][4];
	//	*((USHORT*)&buf[60]) = ranges[i - 1][5] == 0 ? 32767 : ranges[i - 1][5];
	//	*((USHORT*)&buf[67]) = ranges[i - 1][6] == 0 ? 32767 : ranges[i - 1][6];
	//	*((USHORT*)&buf[74]) = ranges[i - 1][7] == 0 ? 32767 : ranges[i - 1][7];

	//	if (ERROR_SUCCESS != RegSetKeyValueW(HKEY_LOCAL_MACHINE, dev, L"HidReportDesctiptorSize", REG_DWORD, &size, 4))
	//	{
	//		delete[] buf;
	//		return false;
	//	}
	//	if (ERROR_SUCCESS != RegSetKeyValue(HKEY_LOCAL_MACHINE, dev, L"HidReportDesctiptor", REG_BINARY, buf, 142))
	//	{
	//		delete[] buf;
	//		return false;
	//	}
	//}
	//delete[] buf;

	//UpdatevJoy();
	return true;
}

void CProfile::UpdatevJoy()
{
	//CONST GUID guidHid = { 0x745a17a0,0x74d3,0x11d0,{0xb6,0xfe,0x00,0xa0,0xc9,0x0f,0x57,0xda} };
	//DWORD idx = 0;
	//HDEVINFO di = SetupDiGetClassDevs(&guidHid, NULL, NULL, DIGCF_PRESENT);
	//if (di != INVALID_HANDLE_VALUE)
	//{
	//	SP_DEVINFO_DATA dev;
	//	ZeroMemory(&dev, sizeof(SP_DEVINFO_DATA));
	//	dev.cbSize = sizeof(SP_DEVINFO_DATA);
	//	DWORD idx = 0;
	//	while (SetupDiEnumDeviceInfo(di, idx, &dev))
	//	{
	//		idx++;
	//		DWORD size = 0;
	//		SetupDiGetDeviceRegistryPropertyA(di, &dev, SPDRP_HARDWAREID, NULL, NULL, 0, &size);
	//		if (size != 0)
	//		{
	//			BYTE* desc = new BYTE[size];
	//			if (SetupDiGetDeviceRegistryPropertyA(di, &dev, SPDRP_HARDWAREID, NULL, desc, size, NULL))
	//			{
	//				if ((_stricmp((char*)desc, "HID\\VID_1234&PID_BEAD&REV_0219&Col01") == 0) ||
	//					(_stricmp((char*)desc, "HID\\VID_1234&PID_BEAD&REV_0219&Col02") == 0) ||
	//					(_stricmp((char*)desc, "HID\\VID_1234&PID_BEAD&REV_0219&Col03") == 0))
	//				{
	//					SetupDiCallClassInstaller(DIF_REMOVE, di, &dev);
	//				}
	//			}
	//			delete[]desc; desc = NULL;
	//		}
	//	}
	//	idx = 0;
	//	while (SetupDiEnumDeviceInfo(di, idx, &dev))
	//	{
	//		idx++;
	//		DWORD size = 0;
	//		SetupDiGetDeviceRegistryPropertyW(di, &dev, SPDRP_HARDWAREID, NULL, NULL, 0, &size);
	//		if (size != 0)
	//		{
	//			BYTE* desc = new BYTE[size];
	//			if (SetupDiGetDeviceRegistryPropertyW(di, &dev, SPDRP_HARDWAREID, NULL, desc, size, NULL))
	//			{
	//				if (_wcsicmp((wchar_t*)desc, L"root\\VID_1234&PID_BEAD&REV_0219") == 0)
	//				{
	//					SP_PROPCHANGE_PARAMS params
	//					{
	//						params.ClassInstallHeader.cbSize = sizeof(SP_CLASSINSTALL_HEADER),
	//						params.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE,
	//						params.Scope = DICS_FLAG_GLOBAL,
	//						params.StateChange = DICS_DISABLE,
	//						params.HwProfile = 0
	//					};          
	//					if (SetupDiSetClassInstallParamsW(di, &dev, &params.ClassInstallHeader, sizeof(params)))
	//					{
	//						SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, di, &dev);
	//						params.StateChange = DICS_ENABLE;
	//						Sleep(1000);
	//						if (SetupDiSetClassInstallParamsW(di, &dev, &params.ClassInstallHeader, sizeof(params)))
	//						{
	//							SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, di, &dev);
	//						}
	//					}
	//					DWORD err = GetLastError();
	//					delete[]desc; desc = NULL;
	//					break;
	//				}
	//			}
	//			delete[]desc; desc = NULL;
	//		}
	//	}
	//	SetupDiDestroyDeviceInfoList(di);
	//}
}