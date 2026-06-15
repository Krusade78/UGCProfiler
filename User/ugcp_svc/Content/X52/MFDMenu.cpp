#include "../framework.h"
#include "MFDMenu.h"
#include "USBX52Write.h"
#include <fstream>

CMFDMenu::CMFDMenu()
{
	pInstance = this;
	menuMFD = {};
	ReadConfiguration();
	timerMenu = CreateThreadpoolTimer(EvtTickMenu, this, nullptr);
	timerHour = CreateThreadpoolTimer(EvtTickHour, this, nullptr);

	LARGE_INTEGER t{};
	t.QuadPart = -40000000LL; //4 seconds
	FILETIME timeout{};
	timeout.dwHighDateTime = t.HighPart;
	timeout.dwLowDateTime = t.LowPart;
	SetThreadpoolTimer(timerHour, &timeout, 4000, 0);
}

CMFDMenu::~CMFDMenu()
{
	pInstance = nullptr;
	SetThreadpoolTimer(timerHour, nullptr, 0, 0);
	CloseThreadpoolTimer(timerHour);
	SetThreadpoolTimer(timerMenu, nullptr, 0, 0);
	CloseThreadpoolTimer(timerMenu);
	if (menuMFD.Activated)
	{
		CloseMenu();
	}
}

void CMFDMenu::SetWelcome()
{
	std::uint8_t row1[] = "\x01  Saitek X-52";
	CX52Write::Get().Set_Text(row1);
	std::uint8_t row2[] = "\x02  Driver v11.0";
	CX52Write::Get().Set_Text(row2);
	std::uint8_t row3[] = "\x03 ";
	CX52Write::Get().Set_Text(row3);

	CX52Write::Get().Light_Global(menuMFD.GlobalLight);
	CX52Write::Get().Light_MFD(menuMFD.MFDLight);
}

#pragma region "Configuration"
void CMFDMenu::ReadConfiguration()
{
	menuMFD.IsHourActivated = true;
	menuMFD.IsDateActivated = true;

	std::ifstream f("mfdconf.dat", std::ios_base::binary);
	if (f.is_open())
	{
		short value = 0;
		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.IsNXTActivated = static_cast<bool>(value);

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.GlobalLight = static_cast<std::uint8_t>(value);

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.MFDLight = static_cast<std::uint8_t>(value);

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.Hour[0].Minutes = value;

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.Hour[1].Minutes = value;

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.Hour[2].Minutes = value;

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.Hour[0]._24h = value == 1;

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.Hour[1]._24h = value == 1;

		f.read(reinterpret_cast<char*>(&value), 2);
		if (!f.good()) { return; }
		menuMFD.Hour[2]._24h = value == 1;
	}
}

void CMFDMenu::SaveConfiguration() const
{
	std::ofstream f("mfdconf.dat", std::ios_base::binary | std::ios_base::trunc);
	if (f.is_open())
	{
		short value = 0;
		value = static_cast<short>(menuMFD.IsNXTActivated);
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }

		value = static_cast<short>(menuMFD.GlobalLight);
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = static_cast<short>(menuMFD.MFDLight);
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = menuMFD.Hour[0].Minutes;
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = menuMFD.Hour[1].Minutes;
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = menuMFD.Hour[2].Minutes;
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = menuMFD.Hour[0]._24h ? 1 : 0;
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = menuMFD.Hour[1]._24h ? 1 : 0;
		f.write(reinterpret_cast<const char*>(&value), 2);
		if (!f.good()) { return; }
		
		value = menuMFD.Hour[2]._24h ? 1 : 0;
		f.write(reinterpret_cast<const char*>(&value), 2);
	}
}
#pragma endregion

void CALLBACK CMFDMenu::EvtTickHour(_Inout_ PTP_CALLBACK_INSTANCE pcInstance, _Inout_opt_ PVOID context, _Inout_ PTP_TIMER pTimer)
{
	if (context != NULL)
	{
		CMFDMenu* local = static_cast<CMFDMenu*>(context);
		SYSTEMTIME now;
		GetLocalTime(&now);

		if (local->menuMFD.IsDateActivated)
		{
			std::array<std::uint8_t, 2> bfd{};
			bfd[0] = 1; bfd[1] = static_cast<std::uint8_t>(now.wDay);
			CX52Write::Get().Set_Date(bfd);
			bfd[0] = 2; bfd[1] = static_cast<std::uint8_t>(now.wMonth);
			CX52Write::Get().Set_Date(bfd);
			bfd[0] = 3; bfd[1] = static_cast<std::uint8_t>(now.wYear % 100);
			CX52Write::Get().Set_Date(bfd);
		}

		if (local->menuMFD.IsHourActivated)
		{
			long hour = local->menuMFD.Hour[0].Minutes * 60;
			std::int64_t absHour = ((hour < 0) ? -1 : 1) * static_cast<__int64>(hour);
			FILETIME nowFT;
			SystemTimeToFileTime(&now, &nowFT);
			LARGE_INTEGER liNowFT{};
			liNowFT.HighPart = nowFT.dwHighDateTime;
			liNowFT.LowPart = nowFT.dwLowDateTime;

			(hour < 0) ? liNowFT.QuadPart -= (absHour * 10000000LL) : liNowFT.QuadPart += (absHour * 10000000LL);
			nowFT.dwHighDateTime = liNowFT.HighPart;
			nowFT.dwLowDateTime = liNowFT.LowPart;
			FileTimeToSystemTime(&nowFT, &now);
			std::array<std::uint8_t, 3> bf{};
			bf[0] = 1;
			bf[1] = static_cast<std::uint8_t>(now.wHour);
			bf[2] = static_cast<std::uint8_t>(now.wMinute);
			if (local->menuMFD.Hour[0]._24h)
			{
				CX52Write::Get().Set_Hour24(bf);
			}
			else
			{
				CX52Write::Get().Set_Hour(bf);
			}
		}
	}
}

void CALLBACK CMFDMenu::EvtTickMenu(_Inout_ PTP_CALLBACK_INSTANCE pcInstance, _Inout_opt_ PVOID context, _Inout_ PTP_TIMER pTimer)
{
	if (context != NULL)
	{
		CMFDMenu* local = static_cast<CMFDMenu*>(context);
		std::uint8_t param = 1;

		local->menuMFD.Activated = true;
		local->menuMFD.TimerWaiting = false;
		CX52Write::Get().Light_Info(param);
		param = 2;
		CX52Write::Get().Light_MFD(param);
		local->menuMFD.ButtonStatus = 0;
		local->menuMFD.CursorStatus = 0;
		local->menuMFD.PageStatus = 0;
		local->ShowPage1();
	}
}

#pragma region "Menu"
//void CMFDMenu::MenuPressButton(std::uint8_t button)
//{
//	if (button == static_cast<std::uint8_t>(Button::Enter))
//	{
//		if (!menuMFD.TimerWaiting && !menuMFD.Activated)
//		{
//			menuMFD.TimerWaiting = true;
//			LARGE_INTEGER t{};
//			t.QuadPart = -30000000LL; //3 segundos
//			FILETIME timeout{};
//			timeout.dwHighDateTime = t.HighPart;
//			timeout.dwLowDateTime = t.LowPart;
//			SetThreadpoolTimer(timerMenu, &timeout, 0, 0);
//		}
//	}
//	if (menuMFD.Activated)
//	{
//		menuMFD.ButtonStatus |= 1u << button;
//	}
//}
//
//void CMFDMenu::MenuReleaseButton(std::uint8_t button)
//{
//	if (button == static_cast<std::uint8_t>(Button::Enter))
//	{
//		if (menuMFD.TimerWaiting && !menuMFD.Activated)
//		{
//			menuMFD.TimerWaiting = false;
//			SetThreadpoolTimer(timerMenu, nullptr, 0, 0);
//		}
//	}
//	if (menuMFD.Activated)
//	{
//		if ((menuMFD.ButtonStatus >> button) & 1)
//		{
//			ChangeStatus(button);
//		}
//		menuMFD.ButtonStatus &= ~((std::uint8_t)(1 << button));
//	}
//}

void CMFDMenu::ChangeStatus(std::uint8_t button)
{
	#pragma region "Enter button"
	if (button == static_cast<std::uint8_t>(Button::Enter))
	{
		switch (menuMFD.PageStatus)
		{
			case 0: //main
				switch (menuMFD.CursorStatus)
				{
					case 0:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 1;
						ShowPageOnOff();
						break;
					case 1:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 2;
						ShowPageLight(menuMFD.GlobalLight);
						break;
					case 2:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 3;
						ShowPageLight(menuMFD.MFDLight);
						break;
					case 3:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 4;
						ShowPageHour(false, static_cast<char>(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[0].Minutes < 0) ? -1 : 1) * menuMFD.Hour[0].Minutes) % 60, menuMFD.Hour[0]._24h);
						break;
					case 4:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 5;
						ShowPageHour(false, static_cast<char>(menuMFD.Hour[1].Minutes / 60), (((menuMFD.Hour[1].Minutes < 0) ? -1 : 1) * menuMFD.Hour[1].Minutes) % 60, menuMFD.Hour[1]._24h);
						break;
					case 5:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 6;
						ShowPageHour(false, static_cast<char>(menuMFD.Hour[2].Minutes / 60), (((menuMFD.Hour[2].Minutes < 0) ? -1 : 1) * menuMFD.Hour[2].Minutes) % 60, menuMFD.Hour[2]._24h);
						break;
					case 6:
						CloseMenu();
						break;
				}
				break;
			case 1: //pedals
				menuMFD.IsNXTActivated = (menuMFD.CursorStatus == 0);
				menuMFD.CursorStatus = 0;
				menuMFD.PageStatus = 0;
				ShowPage1();
				break;
			case 2: // global light
				menuMFD.GlobalLight = menuMFD.CursorStatus;
				CX52Write::Get().Light_Global(menuMFD.GlobalLight);
				menuMFD.CursorStatus = 1;
				menuMFD.PageStatus = 0;
				ShowPage1();
				break;
			case 3: //mfd light
				menuMFD.MFDLight = menuMFD.CursorStatus;
				menuMFD.CursorStatus = 2;
				menuMFD.PageStatus = 0;
				ShowPage1();
				break;
			case 4: //hour 1
			case 5: //hour 2
			case 6: //hour 3
				if (menuMFD.CursorStatus == 3)
				{
					menuMFD.CursorStatus = menuMFD.PageStatus - 1;
					menuMFD.PageStatus = 0;
					ShowPage1();
				}
				else
				{
					menuMFD.PageStatus = (menuMFD.PageStatus * 10 + menuMFD.CursorStatus);
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[0].Minutes < 0) ? -1 : 1) * menuMFD.Hour[0].Minutes) % 60, menuMFD.Hour[0]._24h);
				}
				break;
			case 40:
			case 41:
			case 42:
				menuMFD.PageStatus = 4;
				ShowPageHour(false, static_cast<char>(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[0].Minutes < 0) ? -1 : 1) * menuMFD.Hour[0].Minutes) % 60, menuMFD.Hour[0]._24h);
				break;
			case 50:
			case 51:
			case 52:
				{
					std::array<std::uint8_t, 3> buffer = { 2, 0, 0 };
					if (menuMFD.Hour[1].Minutes < 0)
					{
						buffer[1] = static_cast<std::uint8_t>(((-menuMFD.Hour[1].Minutes) >> 8) + 4);
						buffer[2] = static_cast<std::uint8_t>((-menuMFD.Hour[1].Minutes) & 0xff);
					}
					else
					{
						buffer[1] = static_cast<std::uint8_t>(menuMFD.Hour[1].Minutes >> 8);
						buffer[2] = static_cast<std::uint8_t>(menuMFD.Hour[1].Minutes & 0xff);
					}
					if (menuMFD.Hour[1]._24h)
					{
						CX52Write::Get().Set_Hour24(buffer);
					}
					else
					{
						CX52Write::Get().Set_Hour24(buffer);
					}

					menuMFD.PageStatus = 5;
					ShowPageHour(false, static_cast<char>(menuMFD.Hour[1].Minutes / 60), (((menuMFD.Hour[1].Minutes < 0) ? -1 : 1) * menuMFD.Hour[1].Minutes) % 60, menuMFD.Hour[1]._24h);
				}
				break;
			case 60:
			case 61:
			case 62:
				{
					std::array<std::uint8_t, 3> buffer = { 3, 0, 0 };
					if (menuMFD.Hour[2].Minutes < 0)
					{
						buffer[1] = static_cast<std::uint8_t>(((-menuMFD.Hour[2].Minutes) >> 8) + 4);
						buffer[2] = static_cast<std::uint8_t>((-menuMFD.Hour[2].Minutes) & 0xff);
					}
					else
					{
						buffer[1] = static_cast<std::uint8_t>(menuMFD.Hour[2].Minutes >> 8);
						buffer[2] = static_cast<std::uint8_t>(menuMFD.Hour[2].Minutes & 0xff);
					}
					if (menuMFD.Hour[2]._24h)
					{
						CX52Write::Get().Set_Hour24(buffer);
					}
					else
					{
						CX52Write::Get().Set_Hour24(buffer);
					}
					menuMFD.PageStatus = 6;
					ShowPageHour(false, static_cast<char>(menuMFD.Hour[2].Minutes / 60), (((menuMFD.Hour[2].Minutes < 0) ? -1 : 1) * menuMFD.Hour[2].Minutes) % 60, menuMFD.Hour[2]._24h);
				}
				break;
		}
	}
	#pragma endregion

	#pragma region "button up"
	else if (button == static_cast<std::uint8_t>(Button::Up))
	{
		switch (menuMFD.PageStatus)
		{
			case 0:
				if (menuMFD.CursorStatus != 0)
				{
					menuMFD.CursorStatus--;
				}
				ShowPage1();
				break;
			case 1:
				if (menuMFD.CursorStatus != 0)
				{
					menuMFD.CursorStatus--;
				}
				ShowPageOnOff();
				break;
			case 2:
			case 3:
				if (menuMFD.CursorStatus != 0)
				{
					menuMFD.CursorStatus--;
				}
				ShowPageLight(menuMFD.CursorStatus);
				break;
			case 4:
			case 5:
			case 6:
				if (menuMFD.CursorStatus != 0)
				{
					menuMFD.CursorStatus--;
				}
				ShowPageHour(false, static_cast<char>(menuMFD.Hour[menuMFD.PageStatus - 4].Minutes / 60), (((menuMFD.Hour[menuMFD.PageStatus - 4].Minutes < 0) ? -1 : 1) * menuMFD.Hour[menuMFD.PageStatus - 4].Minutes) % 60, menuMFD.Hour[menuMFD.PageStatus - 4]._24h);
				break;
			case 40:
			case 50:
			case 60:
				{
					char idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes / 60) != 23)
					{
						menuMFD.Hour[idx].Minutes += 60;
					}
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 41:
			case 51:
			case 61:
				{
					char idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes % 60) != 59)
					{
						menuMFD.Hour[idx].Minutes++;
					}
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 42:
			case 52:
			case 62:
				{
					char idx = (menuMFD.PageStatus / 10) - 4;
					menuMFD.Hour[idx]._24h = !menuMFD.Hour[idx]._24h;
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
		}
	}
	#pragma endregion

	#pragma region "button down"
	else if (button == static_cast<std::uint8_t>(Button::Down))
	{
		switch (menuMFD.PageStatus)
		{
			case 0:
				if (menuMFD.CursorStatus != 6)
				{
					menuMFD.CursorStatus++;
				}
				ShowPage1();
				break;
			case 1:
				if (menuMFD.CursorStatus != 1)
				{
					menuMFD.CursorStatus++;
				}
				ShowPageOnOff();
				break;
			case 2:
			case 3:
				if (menuMFD.CursorStatus != 2)
				{
					menuMFD.CursorStatus++;
				}
				ShowPageLight((menuMFD.PageStatus == 2) ? menuMFD.GlobalLight : menuMFD.MFDLight);
				break;
			case 4:
			case 5:
			case 6:
				if (menuMFD.CursorStatus != 3)
				{
					menuMFD.CursorStatus++;
				}
				ShowPageHour(false, static_cast<char>(menuMFD.Hour[menuMFD.PageStatus - 4].Minutes / 60), (((menuMFD.Hour[menuMFD.PageStatus - 4].Minutes < 0) ? -1 : 1) * menuMFD.Hour[menuMFD.PageStatus - 4].Minutes) % 60, menuMFD.Hour[menuMFD.PageStatus - 4]._24h);
				break;
			case 40:
			case 50:
			case 60:
				{
					char idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes / 60) != -23)
					{
						menuMFD.Hour[idx].Minutes -= 60;
					}
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 41:
			case 51:
			case 61:
				{
					char idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes % 60) != 0)
					{
						menuMFD.Hour[idx].Minutes--;
					}
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 42:
			case 52:
			case 62:
				{
					char idx = (menuMFD.PageStatus / 10) - 4;
					menuMFD.Hour[idx]._24h = !menuMFD.Hour[idx]._24h;
					ShowPageHour(true, static_cast<char>(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
		}
	}
	#pragma endregion
}

void CMFDMenu::CloseMenu()
{
	//clear screen
	static constexpr std::uint8_t row1[2] = { 1, 0 };
	CX52Write::Get().Set_Text(row1);
	static constexpr std::uint8_t row2[2] = { 2, 0 };
	CX52Write::Get().Set_Text(row2);
	static constexpr std::uint8_t row3[2] = { 3, 0 };
	CX52Write::Get().Set_Text(row3);

	//turn of light
	{
		CX52Write::Get().Light_Info(0);
	}

	CX52Write::Get().Light_MFD(menuMFD.MFDLight);

	SaveConfiguration();
	menuMFD.Activated = false;
}
#pragma endregion

#pragma region "Pages"
void  CMFDMenu::ShowPage1() const
{
	std::uint8_t cursor = menuMFD.CursorStatus % 3;
	std::uint8_t page = menuMFD.CursorStatus / 3;
	char f1[17] = " Gladiator NXT  ";
	char f2[17] = " Buttons light  ";
	char f3[17] = " MFD light      ";
	char f4[17] = " Hour 1         ";
	char f5[17] = " Hour 2         ";
	char f6[17] = " Hour 3         "; //251
	char f7[17] = " Exit           "; //252
	char f8[17] = "                ";
	char* rows[9]{f1, f2, f3, f4, f5, f6, f7, f8, f8};

	rows[cursor + (page * 3)][0] = '>';

	for (std::uint8_t i = 0; i < 3; i++)
	{
		char* text = rows[i + (page * 3)];
		std::uint8_t buffer[17]{};
		for (std::uint8_t c = 0; c < 16; c++) // Must be 16, strings in buffer are NOT \0 terminated
		{
			buffer[c + 1] = text[c];
		}

		buffer[0] = i + 1;

		CX52Write::Get().Set_Text(buffer);
	}
}

void CMFDMenu::ShowPageOnOff()
{
	std::uint8_t cursor = menuMFD.CursorStatus;
	std::uint8_t f1[9] = " On     ";
	std::uint8_t f2[9] = " Off    ";
	std::uint8_t* rows[2]{ f1, f2 };

	rows[cursor][0] = '>';

	rows[(menuMFD.IsNXTActivated) ? 0 : 1][5] = '(';
	rows[(menuMFD.IsNXTActivated) ? 0 : 1][6] = '*';
	rows[(menuMFD.IsNXTActivated) ? 0 : 1][7] = ')';

	for (char i = 0; i < 2; i++)
	{
		std::uint8_t* text = rows[i];
		std::uint8_t buffer[9]{};
		for (char c = 0; c < 8; c++) // Must be 8, strings in buffer are NOT \0 terminated
		{
			buffer[c + 1] = text[c];
		}
		buffer[0] = i + 1;
		CX52Write::Get().Set_Text(buffer);
	}
	static constexpr std::uint8_t row3[1] = {3}; //row 3 empty
	CX52Write::Get().Set_Text(row3);
}

void CMFDMenu::ShowPageLight(std::uint8_t status) const
{
	std::uint8_t cursor = menuMFD.CursorStatus;
	char f1[11] = " Low      ";
	char f2[11] = " Medium   ";
	char f3[11] = " High     ";
	char* rows[3]{f1, f2, f3};

	rows[cursor][0] = '>';

	rows[status][7] = '(';
	rows[status][8] = '*';
	rows[status][9] = ')';

	for (char i = 0; i < 3; i++)
	{
		char* text = rows[i];
		uint8_t buffer[11]{};
		for (char c = 0; c < 10; c++) // Must be 10, strings in buffer are NOT \0 terminated
		{
			buffer[c + 1] = text[c];
		}
		buffer[0] = (i + 1);

		CX52Write::Get().Set_Text(buffer);
	}
}

void CMFDMenu::ShowPageHour(bool sel, char hour, std::uint8_t minute, bool h24) const
{
	std::uint8_t cursor = menuMFD.CursorStatus % 3;
	std::uint8_t page = menuMFD.CursorStatus / 3;
	char f1[12] = " Hour:     ";
	char f2[12] = " Minute:   ";
	char f3[12] = " AM/PM:    ";
	char f4[12] = " Back      ";
	char f5[12] = "           ";
	char* rows[6]{ f1, f2, f3, f4, f5};

	rows[cursor + (page * 3)][0] = (sel) ? '*' : '>';

	std::snprintf(&f1[7], 2, "%02d", hour);
	std::snprintf(&f2[9], 2, "%02u", minute);
	memcpy(&f3[8], (h24) ? "No " : "Yes", 3);

	for (std::uint8_t i = 0; i < 3; i++)
	{
		char* text = rows[i + (page * 3)];
		std::uint8_t buffer[12]{};

		buffer[0] = i + 1;
		for (char c = 0; c < 11; c++) // Must be 11, strings in buffer are NOT \0 terminated
		{
			buffer[c + 1] = text[c];
		}

		CX52Write::Get().Set_Text(buffer);
	}
}
#pragma endregion
