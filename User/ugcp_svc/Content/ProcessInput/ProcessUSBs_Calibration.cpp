#include "../framework.h"
#include "ProcessUSBs_Calibration.h"

CCalibration::~CCalibration()
{
	limitsCache.clear();
	jittersCache.clear();
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
					limitsCache.erase(joyId);
					limitsCache.insert({ joyId, std::vector<CALIBRATION::ST_LIMITS>() });
					for (auto const& axis : pProfile->GetCalibration()->Limits.at(joyId))
					{
						limitsCache.at(joyId).push_back(CALIBRATION::ST_LIMITS());
						memcpy(&limitsCache.at(joyId).back(), &axis, sizeof(CALIBRATION::ST_LIMITS));
					}
				}
				catch (...) {}
				try
				{
					jittersCache.erase(joyId);
					jittersCache.insert({ joyId, std::vector<CALIBRATION::ST_JITTER>()});
					for (auto const& axis : pProfile->GetCalibration()->Jitters.at(joyId))
					{
						jittersCache.at(joyId).push_back(CALIBRATION::ST_JITTER());
						memcpy(&jittersCache.at(joyId).back(), &axis, sizeof(CALIBRATION::ST_JITTER));
					}
				}
				catch (...) {}
				pnew->second = false;
			}
		}
	}
	pProfile->EndCalibrationRead();

	UCHAR idx = 0;

	// Antijitter

	std::unordered_map<UINT32, std::vector<CALIBRATION::ST_JITTER>>::iterator pjitt = jittersCache.find(joyId);
	if (pjitt != jittersCache.end())
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

	std::unordered_map<UINT32, std::vector<CALIBRATION::ST_LIMITS>>::iterator plimit = limitsCache.find(joyId);
	if (plimit != limitsCache.end())
	{
		idx = 0;
		for (auto const& limit : plimit->second)
		{
			{
				UINT16 pollAxis = static_cast<UINT16>(pHidData->Axis[idx]);
				UINT16 width1, width2;
				width1 = limit.Center - limit.Null - limit.Left;
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
						pollAxis = 0;
					else if (pollAxis >= limit.Right)
						pollAxis = limit.Right;
					else
					{
						if (pollAxis < limit.Center)
						{
							if (width1 != 0)
							{
								pollAxis -= limit.Left;
								pollAxis = ((pollAxis * limit.Center) + (width1 / 2)) / width1; //Equivalent to round function
							}
							else
							{
								pollAxis = limit.Center;
							}
						}
						else
						{
							if (width2 != 0)
							{
								pollAxis -= (limit.Center + limit.Null + 1); //move range to 0
								pollAxis = ((pollAxis * (limit.Right - limit.Center)) + (width2 / 2)) / width2; //Equivalent to round function
								pollAxis += limit.Center + 1;
							}
							else
							{
								pollAxis = limit.Center;
							}
						}
					}
				}

				pHidData->Axis[idx] = pollAxis;
			}

			idx++;
		}
	}
}
