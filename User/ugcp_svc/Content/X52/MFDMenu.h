#pragma once

class CMFDMenu
{
public:
	CMFDMenu();
	~CMFDMenu();

	static CMFDMenu* Get() { return pLocal; }

	void SetWelcome();
	void MenuPressButton(UCHAR button);
	void MenuReleaseButton(UCHAR button);
	void SetHourActivated(bool onoff) { menuMFD.IsHourActivated = onoff; }
	void SetDateActivated(bool onoff) { menuMFD.IsDateActivated = onoff; }
	bool IsActivated() const { return menuMFD.Activated; }
	bool X52Joy() const { return !menuMFD.IsNXTActivated; }

private:
	enum class Button : unsigned char
	{
		Enter = 0,
		Down,
		Up
	};
	struct
	{
		PTP_TIMER	TimerMenu;
		bool		TimerWaiting;
		bool		Activated;
		UCHAR		ButtonStatus;

		PTP_TIMER	TimerHour;
		bool		IsHourActivated;
		bool		IsDateActivated;

		UCHAR		CursorStatus;
		UCHAR		PageStatus;

		bool		IsNXTActivated;
		struct
		{
			SHORT       Minutes; //horas + minutos (en minutos totales)
			BOOLEAN		_24h;
		} Hour[3];

		UCHAR		GlobalLight;
		UCHAR		MFDLight;
	} menuMFD;

	static CMFDMenu* pLocal;

	bool activeHour = false;
	bool activeDate = false;

	void ReadConfiguration();
	void SaveConfiguration() const;
	static void CALLBACK EvtTickMenu(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);
	static void CALLBACK EvtTickHour(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer);

	void ChangeStatus(UCHAR button);
	void CloseMenu();
	void ShowPage1() const;
	void ShowPageOnOff();
	void ShowPageLight(UCHAR status) const;
	void ShowPageHour(bool sel, CHAR hour, UCHAR minute, bool h24) const;
};


