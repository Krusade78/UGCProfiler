#include "../framework.h"
#include <setupapi.h>
#include <initguid.h>
#include "CProfile.h"
#include "../EventQueue/CEventQueue.h"
#include "../ProcessOutput/CProcessOutput.h"
#include "../X52/USBX52Write.h"
#include "../X52/MFDMenu.h"
#include "../ProcessOutput/CVirtualHID.h"


CProfile::CProfile()
{
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

bool CProfile::HF_IoWriteMap(std::span<const std::uint8_t> SystemBuffer)
{
	if (!resetComanmds)
	{
		return false;
	}
	resetComanmds = false;
	if (SystemBuffer.size() < (17 + 1 + 2)) //txt, mouse, button sz, axes sz
	{
		ClearProfile();
		return false;
	}
	else
	{
		CX52Write::Get().Set_Text(SystemBuffer.subspan(0, 17));
		static constexpr std::uint8_t cmd2[] = { 2, 0 };
		CX52Write::Get().Set_Text(cmd2);
		static constexpr std::uint8_t cmd3[] = { 3, 0 };
		CX52Write::Get().Set_Text(cmd3);
	}

	std::unordered_map<UINT32, char> ids;

	WaitForSingleObject(hMutexProgram, INFINITE);
	{
		const std::uint8_t* ptr = SystemBuffer.data() + 17;
		RtlCopyMemory(&profile.MouseTick, ptr++ , 1);

		BYTE count = *ptr++;
		for (BYTE j = 0; j < *(SystemBuffer.data() + 17 + 1); j++)
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

		count = *ptr++;
		for (BYTE j = 0; j < count; j++)
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
					std::vector<UINT16>* pActions = profile.HatsMap.NewButton(joyId, mode, buttonId, *ptr++);
					status.Hats.NewStatus(joyId, mode, buttonId);

					for (BYTE nActions = *ptr++; nActions > 0; nActions--)
					{
						pActions->push_back(*((UINT16*)ptr));
						ptr += sizeof(UINT16);
					}
				}
			}
		}

		count = *ptr++;
		for (BYTE j = 0; j < count; j++)
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
					axis->SoftDeadZone = *ptr++;
					memcpy(&axis->SensibilityX, ptr, 20 * sizeof(double));
					ptr += 20 * sizeof(double);
					memcpy(&axis->SensibilityY, ptr, 20 * sizeof(double));
					ptr += 20 * sizeof(double);
					memcpy(&axis->SensibilityS, ptr, 20 * sizeof(double));
					ptr += 20 * sizeof(double);
					axis->IsSlider = *ptr++;
					//RtlCopyMemory(&axis->Inertia, ptr,  sizeof(double));
					//ptr += sizeof(double);
					memcpy(&axis->DampingK, ptr, sizeof(double));
					ptr += sizeof(double);
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

	return true;
}

bool CProfile::HF_IoWriteCommands(std::span<const std::uint8_t> SystemBuffer)
{
	const std::uint8_t* bufIn;
	size_t sizePredicted = 0;

	ClearProfile();

	if (SystemBuffer.size() != 0)
	{
		//Check OK
		bufIn = SystemBuffer.data();
		while (sizePredicted < SystemBuffer.size())
		{
			sizePredicted += (static_cast<size_t>(*bufIn) * 4) + 1;
			bufIn += (static_cast<size_t>(*bufIn) * 4) + 1;
		}
		if (sizePredicted != SystemBuffer.size())
		{
			return false;
		}
	}

	resetComanmds = true;

	CMFDMenu::Get().SetHourActivated(true);
	CMFDMenu::Get().SetDateActivated(true);

	if (SystemBuffer.size() == 0)
	{
		return true;
	}

	WaitForSingleObject(hMutexProgram, INFINITE);
	{
		profile.Actions = new std::deque<CEventPacket*>();
		bufIn = SystemBuffer.data();
		sizePredicted = 0;
		while (sizePredicted < SystemBuffer.size())
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
				mem->Basic.Data1 = *(bufIn + 1);
				mem->Basic.Data2 = *(bufIn + 2);
				mem->Basic.Extra = *(bufIn + 3) & 0xf;
				mem->Basic.OutputJoy = *(bufIn + 3) >> 4;
				if ((((PEV_COMMAND)bufIn)->Type == CommandType::X52MfdHour) || (((PEV_COMMAND)bufIn)->Type == CommandType::X52MfdHour24))
				{
					CMFDMenu::Get().SetHourActivated(false);
				}
				else if (((PEV_COMMAND)bufIn)->Type == CommandType::MfdDate)
				{
					CMFDMenu::Get().SetDateActivated(false);
				}
				bufIn += 4;
				sizePredicted += 4;
				commandQueue->AddCommand(mem);
			}
			profile.Actions->push_back(commandQueue);
		}
	}
	ReleaseMutex(hMutexProgram);

	return true;
}

void CProfile::WriteAntivibration(std::span<const std::uint8_t> data)
{
	WaitForSingleObject(hMutexCalibration, INFINITE);

	const std::uint8_t* ptr = data.data();
	ptr += sizeof(UINT32);
	for (UINT32 njoys = 0; njoys < *reinterpret_cast<const UINT32*>(data.data()); njoys++)
	{
		std::unordered_map<UINT32, std::vector<CALIBRATION::ST_JITTER>>::iterator pOld = calibration.Jitters.find(*reinterpret_cast<const UINT32*>(ptr));
		if (pOld != calibration.Jitters.end())
		{
			calibration.Jitters.erase(*reinterpret_cast<const UINT32*>(ptr));
		}
		calibration.New.insert({ *reinterpret_cast<const UINT32*>(ptr), true }).first->second = true;
		std::vector<CALIBRATION::ST_JITTER>* vector = &calibration.Jitters.insert({ *reinterpret_cast<const UINT32*>(ptr), std::vector<CALIBRATION::ST_JITTER>()}).first->second;
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

void CProfile::WriteCalibration(std::span<const std::uint8_t> data)
{
	WaitForSingleObject(hMutexCalibration, INFINITE);

	const std::uint8_t* ptr = data.data();
	ptr += sizeof(UINT32);
	for (UINT32 njoys = 0; njoys < *reinterpret_cast<const UINT32*>(data.data()); njoys++)
	{
		std::unordered_map<UINT32, std::vector<CALIBRATION::ST_LIMITS>>::iterator pOld = calibration.Limits.find(*reinterpret_cast<const UINT32*>(ptr));
		if (pOld != calibration.Limits.end())
		{
			calibration.Limits.erase(*reinterpret_cast<const UINT32*>(ptr));
		}
		calibration.New.insert({ *reinterpret_cast<const UINT32*>(ptr), true }).first->second = true;
		std::vector<CALIBRATION::ST_LIMITS>* vector = &calibration.Limits.insert({ *reinterpret_cast<const UINT32*>(ptr), std::vector<CALIBRATION::ST_LIMITS>()}).first->second;
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