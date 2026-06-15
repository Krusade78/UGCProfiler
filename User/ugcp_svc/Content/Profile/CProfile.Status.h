#pragma once
class STATUS
{
public:
	typedef struct TDST_AXIS
	{
		std::uint16_t IncrementalPos;
		std::uint8_t Band;
		double LastPos;
		double LastVelocity;
		//double LastInertiaPos = 0;
	} ST_AXIS;
private:
	// Button definitions
	typedef struct TDST_BUTTONMODEMODEL
	{
		std::unordered_map<std::uint8_t, std::uint8_t> Status; //values are CurrentPosition
	} BUTTONMODEMODEL;
	typedef struct TDST_BUTTONSTATUSMODEL
	{
		std::unordered_map<std::uint8_t, BUTTONMODEMODEL> Modes;
	} BUTTONSTATUSMODEL;

	typedef struct TDST_HIDBUTTONMODEL
	{
		std::uint8_t Status[16]{};
	} HIDBUTTONMODEL;

	//Axis definitions
	typedef struct
	{
		std::unordered_map<std::uint8_t, ST_AXIS> Status;
	} AXISMODEMODEL;
	typedef struct
	{
		std::unordered_map<std::uint8_t, AXISMODEMODEL> Modes;
	} AXISSTATUSMODEL;


	std::unordered_map<std::uint32_t, BUTTONSTATUSMODEL> stButtons;
	std::unordered_map<std::uint32_t, HIDBUTTONMODEL> stHIDButtons;

	std::unordered_map<std::uint32_t, BUTTONSTATUSMODEL> stHats;
	std::unordered_map<std::uint32_t, HIDBUTTONMODEL> stHIDHats;

	std::unordered_map<std::uint32_t, AXISSTATUSMODEL> stAxes;

	std::unordered_map<std::uint32_t, std::unordered_map<std::uint8_t, std::uint8_t>> stAxisPreciseMode;

	class CButtons
	{
	private:
		std::unordered_map <std::uint32_t, STATUS::BUTTONSTATUSMODEL>* pStButtons;
		std::unordered_map<std::uint32_t, HIDBUTTONMODEL>* pStHIDButtons;
	public:
		inline CButtons(std::unordered_map <std::uint32_t, BUTTONSTATUSMODEL>* pStButtons, std::unordered_map<std::uint32_t, HIDBUTTONMODEL>* pStHIDButtons)
		{
			this->pStButtons = pStButtons;
			this->pStHIDButtons = pStHIDButtons;
		}

		inline void NewStatus(std::uint32_t inputJoyId, std::uint8_t mode, std::uint8_t button)
		{
			pStButtons->insert({ inputJoyId, BUTTONSTATUSMODEL() });
			pStButtons->at(inputJoyId).Modes.insert({ mode , BUTTONMODEMODEL() });
			pStButtons->at(inputJoyId).Modes.at(mode).Status.insert({ button, 0 });
			pStHIDButtons->insert({ inputJoyId, HIDBUTTONMODEL() });
		}

		inline bool GetPos(std::uint8_t* pos, std::uint32_t inputJoyId, std::uint8_t mode, std::uint8_t button)
		{
			std::unordered_map<std::uint32_t, BUTTONSTATUSMODEL>::const_iterator pBsm = pStButtons->find(inputJoyId);
			if (pBsm != pStButtons->end())
			{
				std::unordered_map<std::uint8_t, BUTTONMODEMODEL>::const_iterator pAmm = pBsm->second.Modes.find(mode);
				if (pAmm == pBsm->second.Modes.end())
				{
					pAmm = pBsm->second.Modes.find(0);
				}
				if (pAmm != pBsm->second.Modes.end())
				{
					std::unordered_map<std::uint8_t, std::uint8_t>::const_iterator pB = pAmm->second.Status.find(button);
					if (pB != pAmm->second.Status.end())
					{
						*pos = pB->second;
						return true;
					}
				}

			}

			return false;
		}

		inline void SetPos(std::uint8_t pos, bool fixed, std::uint32_t inputJoyId, std::uint8_t mode, std::uint8_t button)
		{
			std::unordered_map<std::uint32_t, BUTTONSTATUSMODEL>::iterator pBsm = pStButtons->find(inputJoyId);
			if (pBsm != pStButtons->end())
			{
				std::unordered_map<std::uint8_t, BUTTONMODEMODEL>::iterator pAmm = pBsm->second.Modes.find(mode);
				if (pAmm == pBsm->second.Modes.end())
				{
					pAmm = pBsm->second.Modes.find(0);
				}
				if (pAmm != pBsm->second.Modes.end())
				{
					std::unordered_map<std::uint8_t, std::uint8_t>::iterator pB = pAmm->second.Status.find(button);
					if (pB != pAmm->second.Status.end())
					{
						pB->second = fixed ? pos : pB->second + pos;
					}
				}
			}
		}

		inline bool GetPressed(std::uint8_t* pressed, std::uint32_t inputJoyId, std::uint8_t button)
		{
			std::unordered_map<UINT32, HIDBUTTONMODEL>::const_iterator ijoy = pStHIDButtons->find(inputJoyId);
			if (ijoy != pStHIDButtons->end())
			{
				*pressed = (ijoy->second.Status[button / 8] >> (button % 8)) & 1;
				return true;
			}
			return false;
		}

		inline void SetPressed(std::uint8_t pressed, std::uint32_t inputJoyId, std::uint8_t button)
		{
			std::unordered_map<std::uint32_t, HIDBUTTONMODEL>::iterator ijoy = pStHIDButtons->find(inputJoyId);
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
		std::unordered_map<std::uint32_t, AXISSTATUSMODEL>* pStAxes;
	public:
		inline CAxes(std::unordered_map<std::uint32_t, AXISSTATUSMODEL>* pStAxes) { this->pStAxes = pStAxes; }

		inline void NewStatus(std::uint32_t inputJoyId, std::uint8_t mode, std::uint8_t axis)
		{
			pStAxes->insert({ inputJoyId, AXISSTATUSMODEL() });
			pStAxes->at(inputJoyId).Modes.insert({ mode , AXISMODEMODEL() });
			pStAxes->at(inputJoyId).Modes.at(mode).Status.insert({ axis, ST_AXIS()});
		}

		inline bool GetStatus(ST_AXIS** status, std::uint32_t inputJoyId, std::uint8_t mode, std::uint8_t axis)
		{
			std::unordered_map<std::uint32_t, AXISSTATUSMODEL>::iterator ijoy = pStAxes->find(inputJoyId);
			if (ijoy != pStAxes->end())
			{
				std::unordered_map<std::uint8_t, AXISMODEMODEL>::iterator imode = ijoy->second.Modes.find(mode);
				if (imode == ijoy->second.Modes.end())
				{
					imode = ijoy->second.Modes.find(0);
				}
				if (imode != ijoy->second.Modes.end())
				{
					std::unordered_map<std::uint8_t, ST_AXIS>::iterator iaxis = imode->second.Status.find(axis);
					if (iaxis != imode->second.Status.end())
					{
						*status = &iaxis->second;
						return true;
					}
				}
			}

			return false;
		}
	};

	class CAxesPreciseMode
	{
	private:
		std::unordered_map<std::uint32_t, std::unordered_map<std::uint8_t, std::uint8_t>>* stAxisPreciseMode;
	public:
		inline CAxesPreciseMode(std::unordered_map<std::uint32_t, std::unordered_map<std::uint8_t, std::uint8_t>>* stAxes) { this->stAxisPreciseMode = stAxisPreciseMode; }

		inline bool GetStatus(std::uint8_t* status, std::uint32_t inputJoyId, std::uint8_t axis)
		{
			std::unordered_map<std::uint32_t, std::unordered_map<std::uint8_t, std::uint8_t>>::const_iterator ijoy = stAxisPreciseMode->find(inputJoyId);
			if (ijoy != stAxisPreciseMode->end())
			{
				std::unordered_map<std::uint8_t, std::uint8_t>::const_iterator iaxis = ijoy->second.find(axis);
				if (iaxis != ijoy->second.end())
				{
					*status = iaxis->second;
					return true;
				}
			}

			return false;
		}

		inline void SetStatus(std::uint8_t preciseMode, std::uint32_t inputJoyId, std::uint8_t axis)
		{
			std::unordered_map<std::uint32_t, std::unordered_map<std::uint8_t, std::uint8_t>>::iterator ijoy = stAxisPreciseMode->find(inputJoyId);
			if (ijoy != stAxisPreciseMode->end())
			{
				std::unordered_map<std::uint8_t, std::uint8_t>::iterator iaxis = ijoy->second.find(axis);
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