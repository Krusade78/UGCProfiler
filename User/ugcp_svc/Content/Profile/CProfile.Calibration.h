#pragma once
class CALIBRATION
{
public:
	typedef struct TDST_LIMITS
	{
		std::uint8_t Null;
		std::uint16_t Left;
		std::uint16_t Center;
		std::uint16_t Right;
		std::uint16_t Range;
	} ST_LIMITS;
	typedef struct TDST_JITTER
	{
		std::uint8_t Antiv;
		std::uint8_t PosRepeated;
		std::uint8_t Margin;
		std::uint8_t Strength;
		std::uint16_t PosChosen;
	} ST_JITTER;

	std::unordered_map<std::uint32_t, std::vector<ST_LIMITS>> Limits;
	std::unordered_map<std::uint32_t, std::vector<ST_JITTER>> Jitters;
	std::unordered_map<std::uint32_t, bool> New;

	ST_LIMITS* GetLimit(std::uint32_t joyId, std::uint8_t axis)
	{
		std::unordered_map<std::uint32_t, std::vector<ST_LIMITS>>::iterator pl = Limits.find(joyId);
		if (pl != Limits.end())
		{
			if (axis < pl->second.size())
			{
				return &pl->second[axis];
			}
		}
		return nullptr;
	}

	ST_JITTER* GetJitter(std::uint32_t joyId, std::uint8_t axis)
	{
		std::unordered_map<std::uint32_t, std::vector<ST_JITTER>>::iterator pj = Jitters.find(joyId);
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