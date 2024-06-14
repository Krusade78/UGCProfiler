#include "../../framework.h"
#include "CGenerateEvents.h"
#include "ProcessUSBs_Axes.h"

void CAxes::SensibilityAndMapping(CProfile* pProfile, UINT32 joyId, PHID_INPUT_DATA old, PHID_INPUT_DATA input)
{
	UCHAR idx;
	UCHAR mode;
	pProfile->LockStatus();
	{
		mode = (pProfile->GetStatus()->SubMode << 4) | pProfile->GetStatus()->Mode;
	}
	pProfile->UnlockStatus();

	UCHAR mapped[3] = { 0, 0, 0 };
	VHID_INPUT_DATA output[3]{};

	//Sensibility
	for (idx = 0; idx < 24; idx++)
	{
		UCHAR sy1;
		UCHAR sy2;
		UINT16 sTopSl = 32767;
		INT16 sTop = 16383;

		if (sTopSl == 0) { sTopSl = 32767; }
		if (sTop == 0) { sTop = sTopSl / 2; }
		bool slider = false;
		pProfile->BeginProfileRead();
		{
			PROGRAMMING::AXISMODEL* axisMap = pProfile->GetProfile()->AxesMap.GetConf(joyId, mode, idx);
			if (axisMap == nullptr)
			{
				pProfile->EndProfileRead();
				continue;
			}
			slider = axisMap->IsSlider;
		}
		pProfile->EndProfileRead();

		INT32 x = input->Axis[idx]; //for casting
		if (!slider && (x == sTop))
		{
			continue;
		}
		bool left = (x < sTop);
		x = slider ? x : ((left) ? sTop - x : x - sTop );
		UCHAR pos = slider ? (UCHAR)((x * 10) / sTopSl) : (UCHAR)((x * 10) / sTop);
		if (pos == 10)
		{
			pos = 9;
		}
		pProfile->BeginProfileRead();
		{
			//checked null previously
			sy1 = (pos == 0) ? 0 : pProfile->GetProfile()->AxesMap.GetConf(joyId, mode, idx)->Sensibility[pos - 1];
			sy2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, mode, idx)->Sensibility[pos];
		}
		pProfile->EndProfileRead();
		if (slider)
		{
			x = (x == sTopSl) ? ((sy2 * sTopSl) / 100) : ((((sy2 - sy1) * ((10 * x) - (pos * sTopSl))) + (sy1 * sTopSl))) / 100;
		}
		else
		{
			x = (x == sTop) ? ((sy2 * sTop) / 100) : ((((sy2 - sy1) * ((10 * x) - (pos * sTop))) + (sy1 * sTop))) / 100;
			x = (left) ? sTop - x : x + sTop;
		}
		input->Axis[idx] = static_cast<UINT16>(x);
	}

	//Mapping
	for (idx = 0; idx < 24; idx++)
	{
		UCHAR vJoy;
		UCHAR axisType;
		UCHAR outputAxis;
		UCHAR mouseSens; //t
		UINT16 eTop, sTop, center;

		pProfile->BeginProfileRead();
		{
			PROGRAMMING::AXISMODEL* axisMap= pProfile->GetProfile()->AxesMap.GetConf(joyId, mode, idx);
			if (axisMap == nullptr)
			{
				pProfile->EndProfileRead();
				continue;
			}
			vJoy = axisMap->VJoyOutput;
			axisType = axisMap->Type;
			outputAxis = axisMap->OutputAxis;
			mouseSens = axisMap->MouseSensibility;
		}
		pProfile->EndProfileRead();
		pProfile->InitCalibrationRead();
		{
			CALIBRATION::ST_LIMITS* cal = pProfile->GetCalibration()->GetLimit(joyId, idx);
			if (cal != nullptr)
			{
				eTop = cal->Range;
				center = cal->Center;
				sTop = eTop;
			}
			else
			{
				pProfile->EndCalibrationRead();
				continue;
			}
		}
		pProfile->EndCalibrationRead();

		if (axisType == 0)
		{
			continue;
		}
		else if ((axisType & 1) == 1) // normal axis
		{
			output[vJoy].Axes[outputAxis] = input->Axis[idx];
			if ((axisType & 0x2) == 2) //inverted normal
			{
				output[vJoy].Axes[outputAxis] = sTop - output[vJoy].Axes[outputAxis];
			}
			mapped[vJoy] |= 1 << outputAxis;
		}
		else if (axisType & 0x8) //map to mouse
		{
			if (input->Axis[idx] != old->Axis[idx])
			{
				if (input->Axis[idx] == center)
				{
					Axis2Mouse(outputAxis, 0);
				}
				else
				{
					INT32 axisTransformed = input->Axis[idx] - center;
					if ((axisType & 0x2) == 0x2) //inverted
					{
						Axis2Mouse(outputAxis, static_cast<CHAR>(-axisTransformed * mouseSens));
					}
					else
					{
						Axis2Mouse(outputAxis, static_cast<CHAR>(axisTransformed * mouseSens));
					}

				}
			}
		}
	}

	for (idx = 0; idx < 3; idx++)
	{
		if (mapped[idx] != 0)
		{
			CGenerateEvents::DirectX(idx, mapped[idx], &output[idx]);
		}
	}
}

void CAxes::Axis2Mouse(UCHAR axis, CHAR mov)
{
	EV_COMMAND event;

	if (axis == 0)
	{
		if (mov == 0)
		{
			event.Type = CommandType::Release | CommandType::MouseLeft;
			event.Basic.Data = 0;
		}
		else
		{
			if (mov >= 0)
			{
				event.Type = CommandType::MouseRight;
				event.Basic.Data = mov;
			}
			else
			{
				event.Type = CommandType::MouseLeft;
				event.Basic.Data = -mov;
			}
		}
	}
	else {
		if (mov == 0)
		{
			event.Type = CommandType::Release | CommandType::MouseUp;
			event.Basic.Data = 0;
		}
		else
		{
			if (mov >= 0)
			{
				event.Type = CommandType::MouseDown;
				event.Basic.Data = mov;
			}
			else
			{
				event.Type = CommandType::MouseUp;
				event.Basic.Data = -mov;
			}
		}
	}

	CGenerateEvents::Mouse(&event);
}

void CAxes::MoveAxis(CProfile* pProfile, UINT32 joyId, UCHAR idx, UINT16 _new)
{
	UINT16 actionId;
	UCHAR change;
	EV_COMMAND axisData;
	UCHAR mode;

	pProfile->BeginProfileRead();
	{
		pProfile->LockStatus();
		{
			mode = (pProfile->GetStatus()->SubMode << 4) | pProfile->GetStatus()->Mode;
			change = TranslateRotary(pProfile, joyId, idx, _new, mode);
			STATUS::ST_AXIS* pStatus = nullptr;
			if (pProfile->GetStatus()->Axes.GetStatus(&pStatus, joyId, mode, idx))
			{
				axisData.Extended.Incremental = pStatus->IncrementalPos;
				axisData.Extended.Band = pStatus->Band;
			}
		}
		pProfile->UnlockStatus();
		if (change != 255)
		{
			actionId = pProfile->GetProfile()->AxesMap.GetConf(joyId, mode, idx)->Actions[change];
			axisData.Extended.Mode = mode & 0xf;
			axisData.Extended.Submode = mode >> 4;
		}
	}
	pProfile->EndProfileRead();

	if (change != 255)
	{
		if (actionId == 0)
		{
			CGenerateEvents::CheckHolds();
		}
		else
		{
			CGenerateEvents::Command(joyId, actionId, idx, CGenerateEvents::Origin::Axis, &axisData);
		}
	}
}

/// <summary>
/// Inside LockStatus and LockProfile
/// </summary>
UCHAR CAxes::TranslateRotary(CProfile* pProfile, UINT32 joyId, UCHAR axis, UINT16 _new, UCHAR mode)
{
	UCHAR idn = 255;
	bool incremental;
	bool bands;

	PROGRAMMING::AXISMODEL* axisMap = pProfile->GetProfile()->AxesMap.GetConf(joyId, mode, axis);
	if (axisMap == nullptr)
	{
		return 255;
	}

	incremental =(axisMap->Type & 16) == 16;
	bands = (axisMap->Type & 32) == 32;


	CALIBRATION::ST_LIMITS* pl = pProfile->GetCalibration()->GetLimit(joyId, axis);
	if (pl == nullptr)
	{
		return 255;
	}
	UINT16 range = pl->Range;
	UINT16 newPos = _new;

	STATUS::ST_AXIS* status;
	if (!pProfile->GetStatus()->Axes.GetStatus(&status, joyId, mode, axis))
	{
		return 255;
	}

	if (incremental)
	{
		UINT16 old = status->IncrementalPos;
		if (_new > old)
		{
			UCHAR positions = axisMap->ToughnessInc;
			if (old < (range - positions))
			{
				if (newPos > (old + positions))
				{
					status->IncrementalPos = newPos;
					idn = 0;
				}
			}
		}
		else
		{
			UCHAR positions = axisMap->ToughnessDec;
			if (old > positions)
			{
				if (newPos < (old - positions))
				{
					status->IncrementalPos = newPos;
					idn = 1;
				}
			}
		}
	}
	else if (bands)
	{

		UCHAR	oldBand = status->Band;
		UCHAR	currentBand = 255;
		UINT16	previousPos = 0;
		UCHAR	idc = 0;

		for(auto const& mapBand : axisMap->Bands)
		{
			bool exit = false;
			UCHAR band = mapBand;

			if ((band == 0) || (band >= 100))
			{
				band = 100;
				exit = true;
			}

			if ((newPos >= previousPos) && (newPos < ((band * range) / 100)))
			{
				currentBand = idc;
				break;
			}
			if (exit)
			{
				break;
			}
			previousPos = static_cast<UINT16>((band * range) / 100);
			idc++;
		}
		if ((currentBand != 255) && (currentBand != oldBand))
		{
			status->Band = currentBand;

			idn = currentBand;
		}
	}

	return idn;
}