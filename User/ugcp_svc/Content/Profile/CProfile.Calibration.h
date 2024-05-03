#pragma once
class CALIBRATION
{
public:
	typedef struct TDST_LIMITS
	{
		UCHAR Null;
		UINT16 Left;
		UINT16 Center;
		UINT16 Right;
		UINT16 Range;
	} ST_LIMITS;
	typedef struct TDST_JITTER
	{
		UCHAR Antiv;
		UCHAR PosRepeated;
		UCHAR Margin;
		UCHAR Strength;
		UINT16 PosChosen;
	} ST_JITTER;

	std::unordered_map<UINT32, std::vector<ST_LIMITS>> Limits;
	std::unordered_map<UINT32, std::vector<ST_JITTER>> Jitters;
	std::unordered_map<UINT32, bool> New;

	ST_LIMITS* GetLimit(UINT32 joyId, UCHAR axis)
	{
		std::unordered_map<UINT32, std::vector<ST_LIMITS>>::iterator pl = Limits.find(joyId);
		if (pl != Limits.end())
		{
			if (axis < pl->second.size())
			{
				return &pl->second[axis];
			}
		}
		return nullptr;
	}

	ST_JITTER* GetJitter(UINT32 joyId, UCHAR axis)
	{
		std::unordered_map<UINT32, std::vector<ST_JITTER>>::iterator pj = Jitters.find(joyId);
		if (pj != Jitters.end())
		{
			if (axis < pj->second.size())
			{
				return &pj->second[axis];
			}
		}
		return nullptr;
	}
};