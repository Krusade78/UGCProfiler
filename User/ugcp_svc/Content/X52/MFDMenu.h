#pragma once
#include <cstdint>


class CMFDMenu
{
public:
	CMFDMenu();
	~CMFDMenu();

	static CMFDMenu& Get() { return *pInstance; }

	void SetWelcome();
	//void MenuPressButton(std::uint8_t button);
	//void MenuReleaseButton(std::uint8_t button);
	void SetHourActivated(bool onoff) { menuMFD.IsHourActivated = onoff; }
	void SetDateActivated(bool onoff) { menuMFD.IsDateActivated = onoff; }
	bool IsActivated() const { return menuMFD.Activated; }
	bool X52Joy() const { return !menuMFD.IsNXTActivated; }

private:
	enum class Button : std::uint8_t
	{
		Enter = 0,
		Down,
		Up
	};
	struct
	{
		bool			TimerWaiting;
		bool			Activated;
		std::uint8_t	ButtonStatus;

		bool			IsHourActivated;
		bool			IsDateActivated;

		std::uint8_t	CursorStatus;
		std::uint8_t	PageStatus;

		bool			IsNXTActivated;
		struct
		{
			short		Minutes; //horas + minutos (en minutos totales)
			bool		_24h;
		} Hour[3];

		std::uint8_t	GlobalLight;
		std::uint8_t	MFDLight;
	} menuMFD;

	PTP_TIMER timerMenu{};
	PTP_TIMER timerHour{};

	inline static CMFDMenu* pInstance{ nullptr };

	void ReadConfiguration();
	void SaveConfiguration() const;
	static void CALLBACK EvtTickMenu(_Inout_ PTP_CALLBACK_INSTANCE pcInstance, _Inout_opt_ PVOID context, _Inout_ PTP_TIMER pTimer);
	static void CALLBACK EvtTickHour(_Inout_ PTP_CALLBACK_INSTANCE pcInstance, _Inout_opt_ PVOID context, _Inout_ PTP_TIMER pTimer);

	void ChangeStatus(std::uint8_t button);
	void CloseMenu();
	void ShowPage1() const;
	void ShowPageOnOff();
	void ShowPageLight(std::uint8_t status) const;
	void ShowPageHour(bool sel, char hour, std::uint8_t minute, bool h24) const;
};


