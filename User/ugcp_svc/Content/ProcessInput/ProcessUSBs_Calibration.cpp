#include "../framework.h"
#include "ProcessUSBs_Calibration.h"

CCalibration::~CCalibration()
{
	limits.clear();
	jitters.clear();
}

void CCalibration::Calibrate(CProfile* pProfile, UINT32 joyId, PHID_INPUT_DATA pHidData)
{
	pProfile->InitCalibrationRead();
	{
		bool read = true;
		std::unordered_map<UINT32, bool>::iterator pnew = pProfile->GetCalibration()->New.find(joyId);
		if (pnew != pProfile->GetCalibration()->New.end())
		{
			if (pnew->second)
			{
				try
				{
					limits.erase(joyId);
					limits.insert({ joyId, std::vector<CALIBRATION::ST_LIMITS>() });
					for (auto const& axis : pProfile->GetCalibration()->Limits.at(joyId))
					{
						limits.at(joyId).push_back(CALIBRATION::ST_LIMITS());
						memcpy(&limits.at(joyId).back(), &axis, sizeof(CALIBRATION::ST_LIMITS));
					}
				}
				catch (...) {}
				try
				{
					jitters.erase(joyId);
					jitters.insert({ joyId, std::vector<CALIBRATION::ST_JITTER>()});
					for (auto const& axis : pProfile->GetCalibration()->Jitters.at(joyId))
					{
						jitters.at(joyId).push_back(CALIBRATION::ST_JITTER());
						memcpy(&jitters.at(joyId).back(), &axis, sizeof(CALIBRATION::ST_JITTER));
					}
				}
				catch (...) {}
				pnew->second = false;
			}
		}
	}
	pProfile->EndCalibrationRead();

	UCHAR idx = 0;

	// Antivibration

	std::unordered_map<UINT32, std::vector<CALIBRATION::ST_JITTER>>::iterator pjitt = jitters.find(joyId);
	if (pjitt != jitters.end())
	{
		for (auto& jitt : pjitt->second)
		{
			if (jitt.Antiv)
			{
				UINT16 pollAxis = static_cast<UINT16>(pHidData->Axis[idx++]);

				if ((pollAxis == jitt.PosChosen) ||(pollAxis < (jitt.PosChosen - jitt.Margin)) || (pollAxis > (jitt.PosChosen + jitt.Margin)))
				{
					jitt.PosRepeated = 0;
					jitt.PosChosen = pollAxis;
				}
				else
				{
					if (jitt.PosRepeated < jitt.Strength)
					{
						jitt.PosRepeated++;
						pollAxis = jitt.PosChosen;
					}
					else
					{
						jitt.PosRepeated = 0;
						jitt.PosChosen = pollAxis;
					}
				}
			}
		}
	}

	// Calibration

	std::unordered_map<UINT32, std::vector<CALIBRATION::ST_LIMITS>>::iterator plimit = limits.find(joyId);
	if (plimit != limits.end())
	{
		idx = 0;
		for (auto const& limit : plimit->second)
		{
			{
				UINT16 pollAxis = static_cast<UINT16>(pHidData->Axis[idx]);
				UINT16 width1, width2;
				width1 = (limit.Center - limit.Null) - limit.Left;
				width2 = limit.Right - (limit.Center + limit.Null);

				if (((pollAxis >= (limit.Center - limit.Null)) && (pollAxis <= (limit.Center + limit.Null))))
				{
					//Null zone
					pHidData->Axis[idx++] = limit.Center;
					continue;
				}
				else
				{
					if (pollAxis < limit.Left)
						pollAxis = limit.Left;
					if (pollAxis > limit.Right)
						pollAxis = limit.Right;

					if (pollAxis < limit.Center)
					{
						if (width1 != limit.Center)
						{
							if (pollAxis >= width1) { pollAxis = width1; }
							pollAxis -= limit.Left;
							pollAxis = ((pollAxis * limit.Center) / width1);
						}
					}
					else
					{
						if (width2 != (limit.Range - limit.Center))
						{
							if (pollAxis >= limit.Right) { pollAxis = limit.Right; }
							pollAxis -= (limit.Center + limit.Null);
							pollAxis = limit.Center + ((pollAxis * (limit.Range - limit.Center)) / width2);
						}
					}
				}

				pHidData->Axis[idx] = pollAxis;
			}

			idx++;
		}
	}
}
