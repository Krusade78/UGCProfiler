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

	UCHAR mapped[16] = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; //16 = Max vJoy devs
	VHID_INPUT_DATA output[16]{};

	//Sensibility
	for (idx = 0; idx < 24; idx++)
	{
		FLOAT x1, x2, y1, y2, m1, m2;
		double range, center;

		bool slider = false;
		//double inertia = 0;
		double dampingK = 0;
		double softDeadZone = 0;
		pProfile->BeginProfileRead();
		{
			PROGRAMMING::AXISMODEL* axisMap = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx);
			if (axisMap == nullptr)
			{
				pProfile->EndProfileRead();
				continue;
			}
			slider = axisMap->IsSlider;
			//inertia = axisMap->Inertia;
			dampingK = axisMap->DampingK;
			softDeadZone = static_cast<double>(axisMap->SoftDeadZone) / static_cast<double>(100);
		}
		pProfile->EndProfileRead();

		pProfile->InitCalibrationRead();
		{
			CALIBRATION::ST_LIMITS* cal = pProfile->GetCalibration()->GetLimit(joyId, idx);
			if (cal != nullptr)
			{
				range = cal->Right;
				center = cal->Center;
			}
			else
			{
				pProfile->EndCalibrationRead();
				continue;
			}
		}
		pProfile->EndCalibrationRead();

		//normalize 0.0 - 1.0 for interpolation
		double normalPos;
		bool left = input->Axis[idx] < center;
		bool truncatedMax = false;
		if (slider)
		{
			normalPos = input->Axis[idx] / range;
		}
		else
		{
			if (input->Axis[idx] == center)
			{
				input->Axis[idx] = 16383;
				continue;
			}
			normalPos = left ? ((center - input->Axis[idx]) / center) : (input->Axis[idx] - center) / (range - center);
		}

		//soft dead zone
		if ((softDeadZone > 0) && (normalPos < softDeadZone))
		{
			double u = normalPos / softDeadZone;
			normalPos = softDeadZone * (6 * u * u * u * u * u - 15 * u * u * u * u + 10 * u * u * u);
		}


		pProfile->BeginProfileRead();
		{
			//checked null previously
			if (normalPos >= pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[19])
			{
				truncatedMax = true;
				normalPos = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityY[19];
			}
			else
			{
				if (normalPos < pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[0])
				{
					x1 = 0;
					x2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[0];
					y1 = 0;
					y2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityY[0];
					m1 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityY[0] / pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[0];
					m2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityS[0];
				}
				else
				{
					for (CHAR pos = 0; pos < 19; pos++)
					{
						if (normalPos < pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[pos + 1])
						{
							x1 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[pos];
							x2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityX[pos + 1];
							y1 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityY[pos];
							y2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityY[pos + 1];
							m1 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityS[pos];
							m2 = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->SensibilityS[pos + 1];
							break;
						}
					}
				}
			}
		}
		pProfile->EndProfileRead();

		if (!truncatedMax) // interpolation
		{
			double h = x2 - x1;
			double s0 = (normalPos - x1) / h;

			double h00 = (1 + 2 * s0) * (1 - s0) * (1 - s0);
			double h10 = s0 * (1 - s0) * (1 - s0);
			double h01 = s0 * s0 * (3 - 2 * s0);
			double h11 = s0 * s0 * (s0 - 1);

			normalPos = h00 * y1 + h10 * h * m1 + h01 * y2 + h11 * h * m2;
		}

		//damping & inertia
		{
			double lastPos = normalPos;
			double lastVelocity = 0;
			//double lastInertia = 0;
			pProfile->LockStatus();
			{
				STATUS::ST_AXIS* pStatus = nullptr;
				if (pProfile->GetStatus()->Axes.GetStatus(&pStatus, joyId, mode, idx))
				{
					lastPos = pStatus->LastPos;
					lastVelocity = pStatus->LastVelocity;
					//lastInertia = pStatus->LastInertiaPos;
				}
			}
			pProfile->UnlockStatus();

			double vel = normalPos - lastPos;
			double acc = vel - lastVelocity;
			double centerFactor = 1.0 - normalPos;
			double dampedPos = normalPos - dampingK * acc * centerFactor; //damping k = 0.25 (flight)

			// clamp
			if (dampedPos > 1.0) { dampedPos = 1.0; }

			if (dampingK == 0) //disabled
			{
				dampedPos = normalPos;
			}

			//inertia
			//normalPos = (dampedPos * (1.0 - inertia)) + (lastInertia * inertia);
			normalPos = dampedPos;

			pProfile->LockStatus();
			{
				STATUS::ST_AXIS* pStatus = nullptr;
				if (pProfile->GetStatus()->Axes.GetStatus(&pStatus, joyId, mode, idx))
				{
					pStatus->LastPos = dampedPos;
					pStatus->LastVelocity = vel;
					//pStatus->LastInertiaPos = normalPos;
				}
			}
			pProfile->UnlockStatus();
		}


		if (slider) //scale to vJoy
		{
			input->Axis[idx] = normalPos * 32767;
		}
		else
		{
			input->Axis[idx] = left ? 16383 * (1.0 - normalPos) : 16383 + (normalPos * 16384);
		}
	}

	//Mapping
	for (idx = 0; idx < 24; idx++)
	{
		UCHAR vJoy;
		UCHAR axisType;
		UCHAR outputAxis;
		UCHAR mouseSens; //t

		pProfile->BeginProfileRead();
		{
			PROGRAMMING::AXISMODEL* axisMap= pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx);
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

		if (axisType == 0)
		{
			continue;
		}
		else if ((axisType & 1) == 1) // normal axis
		{
			output[vJoy].Axes[outputAxis] = input->Axis[idx];
			if ((axisType & 0x2) == 2) //inverted normal
			{
				output[vJoy].Axes[outputAxis] = 32767 - output[vJoy].Axes[outputAxis];
			}
			mapped[vJoy] |= 1 << outputAxis;
		}
		else if (axisType & 0x8) //map to mouse
		{
			if (input->Axis[idx] != old->Axis[idx])
			{
				if (input->Axis[idx] == 16383)
				{
					Axis2Mouse(outputAxis, 0);
				}
				else
				{
					INT32 axisTransformed = input->Axis[idx] - 16383;
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

	for (idx = 0; idx < 16; idx++)
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
			event.Basic.Data1 = 0;
		}
		else
		{
			if (mov >= 0)
			{
				event.Type = CommandType::MouseRight;
				event.Basic.Data1 = mov;
			}
			else
			{
				event.Type = CommandType::MouseLeft;
				event.Basic.Data1 = -mov;
			}
		}
	}
	else {
		if (mov == 0)
		{
			event.Type = CommandType::Release | CommandType::MouseUp;
			event.Basic.Data1 = 0;
		}
		else
		{
			if (mov >= 0)
			{
				event.Type = CommandType::MouseDown;
				event.Basic.Data1 = mov;
			}
			else
			{
				event.Type = CommandType::MouseUp;
				event.Basic.Data1 = -mov;
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
			actionId = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, idx)->Actions[change];
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

	PROGRAMMING::AXISMODEL* axisMap = pProfile->GetProfile()->AxesMap.GetConf(joyId, &mode, axis);
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
	const UINT16 range = pl->Range;
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
		if ((currentBand == 255) && (newPos >= previousPos))
		{
			currentBand = idc;
		}
		if ((currentBand != 255) && (currentBand != oldBand))
		{
			status->Band = currentBand;

			idn = currentBand;
		}
	}

	return idn;
}