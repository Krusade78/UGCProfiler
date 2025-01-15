#pragma once
class STATUS
{
public:
	typedef struct TDST_AXIS
	{
		UINT16 IncrementalPos = 0;
		UCHAR Band = 0;
	} ST_AXIS;
private:
	// Button definitions
	typedef struct TDST_BUTTONMODEMODEL
	{
		std::unordered_map<UCHAR, UCHAR> Status; //values are CurrentPosition
	} BUTTONMODEMODEL;
	typedef struct TDST_BUTTONSTATUSMODEL
	{
		std::unordered_map<UCHAR, BUTTONMODEMODEL> Modes;
	} BUTTONSTATUSMODEL;

	typedef struct TDST_HIDBUTTONMODEL
	{
		UCHAR Status[16]{};
	} HIDBUTTONMODEL;

	//Axis definitions
	typedef struct
	{
		std::unordered_map<UCHAR, ST_AXIS> Status;
	} AXISMODEMODEL;
	typedef struct
	{
		std::unordered_map<UCHAR, AXISMODEMODEL> Modes;
	} AXISSTATUSMODEL;


	std::unordered_map<UINT32, BUTTONSTATUSMODEL> stButtons;
	std::unordered_map<UINT32, HIDBUTTONMODEL> stHIDButtons;

	std::unordered_map<UINT32, BUTTONSTATUSMODEL> stHats;
	std::unordered_map<UINT32, HIDBUTTONMODEL> stHIDHats;

	std::unordered_map<UINT32, AXISSTATUSMODEL> stAxes;

	std::unordered_map<UINT32, std::unordered_map<UCHAR, UCHAR>> stAxisPreciseMode;

	class CButtons
	{
	private:
		std::unordered_map <UINT32, STATUS::BUTTONSTATUSMODEL>* pStButtons;
		std::unordered_map<UINT32, HIDBUTTONMODEL>* pStHIDButtons;
	public:
		inline CButtons(std::unordered_map <UINT32, BUTTONSTATUSMODEL>* pStButtons, std::unordered_map<UINT32, HIDBUTTONMODEL>* pStHIDButtons)
		{
			this->pStButtons = pStButtons;
			this->pStHIDButtons = pStHIDButtons;
		}

		inline void NewStatus(UINT32 inputJoyId, UCHAR mode, UCHAR button)
		{
			pStButtons->insert({ inputJoyId, BUTTONSTATUSMODEL() });
			pStButtons->at(inputJoyId).Modes.insert({ mode , BUTTONMODEMODEL() });
			pStButtons->at(inputJoyId).Modes.at(mode).Status.insert({ button, 0 });
			pStHIDButtons->insert({ inputJoyId, HIDBUTTONMODEL() });
		}

		inline bool GetPos(UCHAR* pos, UINT32 inputJoyId, UCHAR mode, UCHAR button)
		{
			std::unordered_map<UINT32, BUTTONSTATUSMODEL>::const_iterator pBsm = pStButtons->find(inputJoyId);
			if (pBsm != pStButtons->end())
			{
				BUTTONMODEMODEL pAmm = pBsm->second.Modes.at(mode);
				std::unordered_map<UCHAR, UCHAR>::const_iterator pB = pAmm.Status.find(button);
				if (pB != pAmm.Status.end())
				{
					*pos =  pB->second;
					return true;
				}
			}

			return false;
		}

		inline void SetPos(UCHAR pos, bool fixed, UINT32 inputJoyId, UCHAR mode, UCHAR button)
		{
			std::unordered_map<UINT32, BUTTONSTATUSMODEL>::iterator pBsm = pStButtons->find(inputJoyId);
			if (pBsm != pStButtons->end())
			{
				BUTTONMODEMODEL& pAmm = pBsm->second.Modes.at(mode);
				std::unordered_map<UCHAR, UCHAR>::iterator pB = pAmm.Status.find(button);
				if (pB != pAmm.Status.end())
				{
					pB->second = fixed ? pos : pB->second + pos;
				}
			}
		}

		inline bool GetPressed(UCHAR* pressed, UINT32 inputJoyId, UCHAR button)
		{
			std::unordered_map<UINT32, HIDBUTTONMODEL>::const_iterator ijoy = pStHIDButtons->find(inputJoyId);
			if (ijoy != pStHIDButtons->end())
			{
				*pressed = (ijoy->second.Status[button / 8] >> (button % 8)) & 1;
				return true;
			}
			return false;
		}

		inline void SetPressed(UCHAR pressed, UINT32 inputJoyId, UCHAR button)
		{
			std::unordered_map<UINT32, HIDBUTTONMODEL>::iterator ijoy = pStHIDButtons->find(inputJoyId);
			if (ijoy != pStHIDButtons->end())
			{
				if (pressed)
				{
					ijoy->second.Status[button / 8] |= 1 << (button % 8);
				}
				else
				{
					ijoy->second.Status[button / 8] &= ~(1 << (button % 8));
				}
			}
		}
	};

	class CAxes
	{
	private:
		std::unordered_map<UINT32, AXISSTATUSMODEL>* pStAxes;
	public:
		inline CAxes(std::unordered_map<UINT32, AXISSTATUSMODEL>* pStAxes) { this->pStAxes = pStAxes; }

		inline void NewStatus(UINT32 inputJoyId, UCHAR mode, UCHAR axis)
		{
			pStAxes->insert({ inputJoyId, AXISSTATUSMODEL() });
			pStAxes->at(inputJoyId).Modes.insert({ mode , AXISMODEMODEL() });
			pStAxes->at(inputJoyId).Modes.at(mode).Status.insert({ axis, ST_AXIS()});
		}

		inline bool GetStatus(ST_AXIS** status, UINT32 inputJoyId, UCHAR mode, UCHAR axis)
		{
			std::unordered_map<UINT32, AXISSTATUSMODEL>::iterator ijoy = pStAxes->find(inputJoyId);
			if (ijoy != pStAxes->end())
			{
				AXISMODEMODEL* imode = &ijoy->second.Modes.at(mode);
				std::unordered_map<UCHAR, ST_AXIS>::iterator iaxis = imode->Status.find(axis);
				if (iaxis != imode->Status.end())
				{
					*status = &iaxis->second;
					return true;
				}
			}

			return false;
		}
	};

	class CAxesPreciseMode
	{
	private:
		std::unordered_map<UINT32, std::unordered_map<UCHAR, UCHAR>>* stAxisPreciseMode;
	public:
		inline CAxesPreciseMode(std::unordered_map<UINT32, std::unordered_map<UCHAR, UCHAR>>* stAxes) { this->stAxisPreciseMode = stAxisPreciseMode; }

		inline bool GetStatus(UCHAR* status, UINT32 inputJoyId, UCHAR axis)
		{
			std::unordered_map<UINT32, std::unordered_map<UCHAR, UCHAR>>::const_iterator ijoy = stAxisPreciseMode->find(inputJoyId);
			if (ijoy != stAxisPreciseMode->end())
			{
				std::unordered_map<UCHAR, UCHAR>::const_iterator iaxis = ijoy->second.find(axis);
				if (iaxis != ijoy->second.end())
				{
					*status = iaxis->second;
					return true;
				}
			}

			return false;
		}

		inline void SetStatus(UCHAR preciseMode, UINT32 inputJoyId, UCHAR axis)
		{
			std::unordered_map<UINT32, std::unordered_map<UCHAR, UCHAR>>::iterator ijoy = stAxisPreciseMode->find(inputJoyId);
			if (ijoy != stAxisPreciseMode->end())
			{
				std::unordered_map<UCHAR, UCHAR>::iterator iaxis = ijoy->second.find(axis);
				if (iaxis != ijoy->second.end())
				{
					iaxis->second = preciseMode;
				}
			}
		}
	};
public:
	CButtons Buttons = CButtons(&stButtons, &stHIDButtons);
	CButtons Hats = CButtons(&stHats, &stHIDHats);
	CAxes Axes = CAxes(&stAxes);
	CAxesPreciseMode AxisPreciseMode = CAxesPreciseMode(&stAxisPreciseMode);
	char SubMode = 0;
	char Mode = 0;

	inline void Clear()
	{
		Mode = 0;
		SubMode = 0;
		stButtons.clear();
		stHIDButtons.clear();
		stHats.clear();
		stHIDHats.clear();
		stAxes.clear();
		stAxisPreciseMode.clear();
	}
};