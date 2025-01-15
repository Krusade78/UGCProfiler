#include "../framework.h"
#include "CProcessInput.h"
#include "GenerateEvents/CGenerateEvents.h"
#include "GenerateEvents/ProcessUSBs_Buttons-Hats.h"
#include "GenerateEvents/ProcessUSBs_Axes.h"
//#include "../X52/MFDMenu.h"

CProcessInput::CProcessInput(CProfile* pProfile)
{
	this->pProfile = pProfile;
}

void CProcessInput::GetOldHidData(UINT32 joyId, PHID_INPUT_DATA data)
{
	std::unordered_map<UINT32, HID_INPUT_DATA>::iterator ls = lastStatus.find(joyId);
	if (ls == lastStatus.end())
	{
		lastStatus.insert({ joyId, HID_INPUT_DATA() });
		ls = lastStatus.find(joyId);
	}

	RtlCopyMemory(data, &ls->second, sizeof(HID_INPUT_DATA));
}

void CProcessInput::SetOldHidData(UINT32 joyId, PHID_INPUT_DATA data)
{
	RtlCopyMemory(&lastStatus.at(joyId), data, sizeof(HID_INPUT_DATA));
}

void CProcessInput::Process(UINT32 joyId, PHID_INPUT_DATA p_hidData)
{
	HID_INPUT_DATA viejohidData;

	GetOldHidData(joyId, &viejohidData);
	SetOldHidData(joyId, p_hidData);

	bool cambio = false;
	for (UINT64 i = 0; i < sizeof(HID_INPUT_DATA) / sizeof(UINT64); i++)
	{
		if (((UINT64*)&viejohidData)[i] != ((UINT64*)p_hidData)[i])
		{
			cambio = true;
			break;
		}
	}
	if (!cambio)
	{
		return;
	}

	if (!pProfile->GetRawMode() && !pProfile->GetCalibrationMode())
	{
		//	Buttons

		for (UCHAR idx = 0; idx < 2; idx++)
		{
			UINT64 changed = p_hidData->Buttons[idx] ^ viejohidData.Buttons[idx];
			if (changed != 0)
			{
				for (UCHAR exp = 0; exp < 64; exp++)
				{
					if ((changed >> exp) & 1)
					{ // if button has changed
						if ((p_hidData->Buttons[idx] >> exp) & 1)
							CButtonsHats::PressButton(pProfile, joyId, (idx * 64) + exp);
						else
							CButtonsHats::ReleaseButton(pProfile, joyId, (idx * 64) + exp);
					}
				}
			}
		}

		// Hats

		for (UCHAR idx = 0; idx < 4; idx++)
		{
			if (p_hidData->Hats[idx] != viejohidData.Hats[idx])
			{
				if (viejohidData.Hats[idx] != 255)
					CButtonsHats::ReleaseHat(pProfile, joyId, (idx * 8) + viejohidData.Hats[idx]);
				if (p_hidData->Hats[idx] != 255)
					CButtonsHats::PressHat(pProfile, joyId, (idx * 8) + p_hidData->Hats[idx]);
			}
		}

		// Axes
		for (UCHAR idx = 0; idx < 24; idx++)
		{
			if (p_hidData->Axis[idx] != viejohidData.Axis[idx])
				CAxes::MoveAxis(pProfile, joyId, idx, p_hidData->Axis[idx]);
		}

		// Sensibility and mapping
		CAxes::SensibilityAndMapping(pProfile, joyId, &viejohidData, p_hidData);
	}
	else
	{
		pProfile->BeginProfileRead();
		UCHAR outputId = pProfile->GetProfile()->GetRawDevice(joyId);
		pProfile->EndProfileRead();
		if (outputId == 0) { return; }

		VHID_INPUT_DATA vJoyData;
		RtlCopyMemory(&vJoyData.Axes, &p_hidData->Axis, sizeof(vJoyData.Axes));
		RtlCopyMemory(&vJoyData.Buttons, reinterpret_cast<UCHAR*>(&p_hidData->Buttons), sizeof(vJoyData.Buttons));
		RtlCopyMemory(&vJoyData.Hats, &p_hidData->Hats, sizeof(vJoyData.Hats));
		CGenerateEvents::DirectX(outputId, 0xff, &vJoyData);
	}
}