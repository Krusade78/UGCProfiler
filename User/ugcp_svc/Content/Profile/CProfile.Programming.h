#pragma once
class PROGRAMMING
{
public:
#pragma warning(disable:26495)
	typedef struct
	{
		UCHAR MouseSensibility;
		UCHAR VJoyOutput;
		UCHAR Type;  //Mapped in bits 0:none, 1:Normal, 10:Inverted, 100:Mini, 1000:Mouse, 10000:Incremental, 100000: Bands
		UCHAR OutputAxis; //0:X, Y, Z, Rx, Ry, Rz, Sl1, Sl2
		UCHAR Sensibility[10]; // Curva de sernsibilidad
		UCHAR IsSlider;
		std::vector<UCHAR> Bands;
		std::vector<UINT16> Actions;
		UCHAR ToughnessInc;
		UCHAR ToughnessDec;
	} AXISMODEL;
#pragma warning(default:26495)

	typedef struct
	{
		UCHAR Type;  //0: normal, 1:Toggle
		std::vector<UINT16> Actions;
	} BUTTONMODEL;
private:
	typedef struct
	{
		std::unordered_map<UCHAR, BUTTONMODEL> Buttons;
	} BUTTONMODEMODEL;
	typedef struct
	{
		std::unordered_map<UCHAR, BUTTONMODEMODEL> Modes;
	} BUTTONMAPMODEL;


	typedef struct
	{
		std::unordered_map<UCHAR, AXISMODEL> Axes;
	} AXISMODEMODEL;
	typedef struct
	{
		std::unordered_map<UCHAR, AXISMODEMODEL> Modes;
	} AXISMAPMODEL;

	std::unordered_map<UINT32, BUTTONMAPMODEL> stButtonsMap;
	std::unordered_map<UINT32, AXISMAPMODEL> stAxesMap;

	class CButtonMap
	{
	private:
		std::unordered_map<UINT32, BUTTONMAPMODEL>* pStButtonsMap;
	public:
		inline CButtonMap(std::unordered_map<UINT32, BUTTONMAPMODEL>* pStButtonsMap) { this->pStButtonsMap = pStButtonsMap; }

		inline std::vector<UINT16>* NewButton(UINT32 joyId, UCHAR mode, UCHAR button, UCHAR type)
		{
			pStButtonsMap->insert({ joyId, BUTTONMAPMODEL() });
			pStButtonsMap->at(joyId).Modes.insert({ mode , BUTTONMODEMODEL() });
			pStButtonsMap->at(joyId).Modes.at(mode).Buttons.insert({ button, BUTTONMODEL() });
			pStButtonsMap->at(joyId).Modes.at(mode).Buttons.at(button).Type = type;
			return &pStButtonsMap->at(joyId).Modes.at(mode).Buttons.at(button).Actions;
		}

		inline BUTTONMODEL* GetConf(UINT32 joyId, UCHAR mode, UCHAR button)
		{
			std::unordered_map<UINT32, BUTTONMAPMODEL>::iterator pBmm = pStButtonsMap->find(joyId);
			if (pBmm != pStButtonsMap->end())
			{
				BUTTONMODEMODEL* pMm = &pBmm->second.Modes.at(mode);
				std::unordered_map<UCHAR, BUTTONMODEL>::iterator pB = pMm->Buttons.find(button);
				if (pB != pMm->Buttons.end())
				{
					return &pB->second;
				}
			}
			return nullptr;
		}
	};

	class CAxesMap
	{
	private:
		std::unordered_map<UINT32, AXISMAPMODEL>* pStAxesMap;
	public:
		inline CAxesMap(std::unordered_map<UINT32, AXISMAPMODEL>* pStAxesMap) { this->pStAxesMap = pStAxesMap; }

		inline AXISMODEL* NewAxis(UINT32 joyId, UCHAR mode, UCHAR axis)
		{
			pStAxesMap->insert({ joyId, PROGRAMMING::AXISMAPMODEL() });
			pStAxesMap->at(joyId).Modes.insert({ mode , PROGRAMMING::AXISMODEMODEL() });
			pStAxesMap->at(joyId).Modes.at(mode).Axes.insert({ axis, AXISMODEL()});
			return &pStAxesMap->at(joyId).Modes.at(mode).Axes.at(axis);
		}

		inline AXISMODEL* GetConf(UINT32 joyId, UCHAR mode, UCHAR axis)
		{
			std::unordered_map<UINT32, AXISMAPMODEL>::iterator pAmm = pStAxesMap->find(joyId);
			if (pAmm != pStAxesMap->end())
			{
				AXISMODEMODEL* pMm = &pAmm->second.Modes.at(mode);
				std::unordered_map<UCHAR, AXISMODEL>::iterator pA = pMm->Axes.find(axis);
				if (pA != pMm->Axes.end())
				{
					return &pA->second;
				}
			}
			return nullptr;
		}
	};
public:
	CButtonMap ButtonsMap = CButtonMap(&stButtonsMap);
	CAxesMap AxesMap = CAxesMap(&stAxesMap);
	std::deque<CEventPacket*>* Actions = nullptr;
	UCHAR MouseTick = 5;

	inline void Clear() 
	{
		stButtonsMap.clear();
		stAxesMap.clear();
	}

	inline UCHAR GetRawDevice(UINT32 joyId) //to bypass profile and test/calibrate
	{
		std::unordered_map<UINT32, bool> ids;
		for (auto const& id : stButtonsMap)
		{
			ids.insert({ id.first, false });
		}
		for (auto const& id : stAxesMap)
		{
			ids.insert({ id.first, false });
		}
		UCHAR retId = 1;
		for (auto const& joy : ids)
		{
			if (joy.first == joyId) return retId;
			retId++;
			if (retId == 4) break;
		}
		return 0;
	}

	inline bool DeviceIncluded(UINT32 joyId)
	{
		return ((stAxesMap.find(joyId) != stAxesMap.end()) || ((stButtonsMap.find(joyId) != stButtonsMap.end())));
	}
};