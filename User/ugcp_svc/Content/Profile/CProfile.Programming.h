#pragma once
class PROGRAMMING
{
public:
#pragma warning(disable:26495)
	typedef struct
	{
		std::uint8_t MouseSensibility;
		std::uint8_t VJoyOutput;
		std::uint8_t Type;  //Mapped in bits 0:none, 1:Normal, 10:Inverted, 100:Mini, 1000:Mouse, 10000:Incremental, 100000: Bands
		std::uint8_t OutputAxis; //0:X, Y, Z, Rx, Ry, Rz, Sl1, Sl2
		std::uint8_t SoftDeadZone;
		double SensibilityX[20]; // curve points in X, last point MUST be 1.0
		double SensibilityY[20]; // curve points in Y
		double SensibilityS[20]; // slopes for interpolation
		std::uint8_t IsSlider;
		//double Inertia;
		double DampingK;
		std::vector<std::uint8_t> Bands;
		std::vector<std::uint16_t> Actions;
		std::uint8_t ToughnessInc;
		std::uint8_t ToughnessDec;
	} AXISMODEL;

	typedef struct
	{
		std::uint8_t Type;  //0: normal, 1:Toggle
		std::vector<std::uint16_t> Actions;
	} BUTTONMODEL;
#pragma warning(default:26495)
private:
	typedef struct
	{
		std::unordered_map<std::uint8_t, BUTTONMODEL> Buttons;
	} BUTTONMODEMODEL;
	typedef struct
	{
		std::unordered_map<std::uint8_t, BUTTONMODEMODEL> Modes;
	} BUTTONMAPMODEL;


	typedef struct
	{
		std::unordered_map<std::uint8_t, AXISMODEL> Axes;
	} AXISMODEMODEL;
	typedef struct
	{
		std::unordered_map<std::uint8_t, AXISMODEMODEL> Modes;
	} AXISMAPMODEL;

	std::unordered_map<std::uint32_t, BUTTONMAPMODEL> stButtonsMap;
	std::unordered_map<std::uint32_t, BUTTONMAPMODEL> stHatsMap;
	std::unordered_map<std::uint32_t, AXISMAPMODEL> stAxesMap;

	class CButtonMap
	{
	private:
		std::unordered_map<std::uint32_t, BUTTONMAPMODEL>* pStButtonsMap;
	public:
		inline CButtonMap(std::unordered_map<std::uint32_t, BUTTONMAPMODEL>* pStButtonsMap) { this->pStButtonsMap = pStButtonsMap; }

		inline std::vector<std::uint16_t>* NewButton(std::uint32_t joyId, std::uint8_t mode, std::uint8_t button, std::uint8_t type)
		{
			pStButtonsMap->insert({ joyId, BUTTONMAPMODEL() });
			pStButtonsMap->at(joyId).Modes.insert({ mode , BUTTONMODEMODEL() });
			pStButtonsMap->at(joyId).Modes.at(mode).Buttons.insert({ button, BUTTONMODEL() });
			pStButtonsMap->at(joyId).Modes.at(mode).Buttons.at(button).Type = type;
			return &pStButtonsMap->at(joyId).Modes.at(mode).Buttons.at(button).Actions;
		}

		inline BUTTONMODEL* GetConf(std::uint32_t joyId, std::uint8_t* mode, std::uint8_t button)
		{
			std::unordered_map<std::uint32_t, BUTTONMAPMODEL>::iterator pBmm = pStButtonsMap->find(joyId);
			if (pBmm != pStButtonsMap->end())
			{
				std::unordered_map<std::uint8_t, BUTTONMODEMODEL>::iterator pMm = pBmm->second.Modes.find(*mode);
				if (pMm == pBmm->second.Modes.end())
				{
					pMm = pBmm->second.Modes.find(0);
					if (pMm != pBmm->second.Modes.end())
					{
						*mode = 0;
					}
				}
				if (pMm != pBmm->second.Modes.end())
				{
					std::unordered_map<std::uint8_t, BUTTONMODEL>::iterator pB = pMm->second.Buttons.find(button);
					if (pB != pMm->second.Buttons.end())
					{
						return &pB->second;
					}
				}
			}
			return nullptr;
		}
	};

	class CAxesMap
	{
	private:
		std::unordered_map<std::uint32_t, AXISMAPMODEL>* pStAxesMap;
	public:
		inline CAxesMap(std::unordered_map<std::uint32_t, AXISMAPMODEL>* pStAxesMap) { this->pStAxesMap = pStAxesMap; }

		inline AXISMODEL* NewAxis(std::uint32_t joyId, std::uint8_t mode, std::uint8_t axis)
		{
			pStAxesMap->insert({ joyId, PROGRAMMING::AXISMAPMODEL() });
			pStAxesMap->at(joyId).Modes.insert({ mode , PROGRAMMING::AXISMODEMODEL() });
			pStAxesMap->at(joyId).Modes.at(mode).Axes.insert({ axis, AXISMODEL()});
			return &pStAxesMap->at(joyId).Modes.at(mode).Axes.at(axis);
		}

		inline AXISMODEL* GetConf(std::uint32_t joyId, std::uint8_t* mode, std::uint8_t axis)
		{
			std::unordered_map<std::uint32_t, AXISMAPMODEL>::iterator pAmm = pStAxesMap->find(joyId);
			if (pAmm != pStAxesMap->end())
			{
				std::unordered_map<std::uint8_t, AXISMODEMODEL>::iterator pMm = pAmm->second.Modes.find(*mode);
				if (pMm == pAmm->second.Modes.end())
				{
					pMm = pAmm->second.Modes.find(0);
					if (pMm != pAmm->second.Modes.end())
					{
						*mode = 0;
					}
				}
				if (pMm != pAmm->second.Modes.end())
				{
					std::unordered_map<std::uint8_t, AXISMODEL>::iterator pA = pMm->second.Axes.find(axis);
					if (pA != pMm->second.Axes.end())
					{
						return &pA->second;
					}
				}
			}
			return nullptr;
		}
	};
public:
	CButtonMap ButtonsMap = CButtonMap(&stButtonsMap);
	CButtonMap HatsMap = CButtonMap(&stHatsMap);
	CAxesMap AxesMap = CAxesMap(&stAxesMap);
	std::deque<CEventPacket*>* Actions = nullptr;
	std::uint8_t MouseTick = 5;

	inline void Clear() 
	{
		stButtonsMap.clear();
		stHatsMap.clear();
		stAxesMap.clear();
	}

	inline UCHAR GetRawDevice(std::uint32_t joyId) //to bypass profile and test/calibrate
	{
		std::unordered_map<std::uint32_t, bool> ids;
		for (auto const& id : stButtonsMap)
		{
			ids.insert({ id.first, false });
		}
		for (auto const& id : stHatsMap)
		{
			ids.insert({ id.first, false });
		}
		for (auto const& id : stAxesMap)
		{
			ids.insert({ id.first, false });
		}
		std::uint8_t retId = 1;
		for (auto const& joy : ids)
		{
			if (joy.first == joyId) return retId;
			retId++;
			if (retId == 4) break;
		}
		return 0;
	}

	inline bool DeviceIncluded(std::uint32_t joyId)
	{
		return ((stAxesMap.find(joyId) != stAxesMap.end()) || ((stButtonsMap.find(joyId) != stButtonsMap.end())) || ((stHatsMap.find(joyId) != stHatsMap.end())));
	}
};