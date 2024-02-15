#include "../framework.h"
#include "MFDMenu.h"
#include "USBX52Write.h"

CMFDMenu* CMFDMenu::pLocal = nullptr;

CMFDMenu::CMFDMenu()
{
	pLocal = this;
	RtlZeroMemory(&menuMFD, sizeof(menuMFD));
	ReadConfiguration();
	menuMFD.TimerMenu = CreateThreadpoolTimer(EvtTickMenu, this, NULL);
	menuMFD.TimerHour = CreateThreadpoolTimer(EvtTickHour, this, NULL);

	LARGE_INTEGER t{};
	t.QuadPart = -40000000LL; //4 seconds
	FILETIME timeout{};
	timeout.dwHighDateTime = t.HighPart;
	timeout.dwLowDateTime = t.LowPart;
	SetThreadpoolTimer(menuMFD.TimerHour, &timeout, 4000, 0);
}

CMFDMenu::~CMFDMenu()
{
	pLocal = nullptr;
	SetThreadpoolTimer(menuMFD.TimerHour, NULL, 0, 0);
	CloseThreadpoolTimer(menuMFD.TimerHour);
	SetThreadpoolTimer(menuMFD.TimerMenu, NULL, 0, 0);
	CloseThreadpoolTimer(menuMFD.TimerMenu);
	if (menuMFD.Activated)
	{
		CloseMenu();
	}
}

void CMFDMenu::SetWelcome()
{
	UCHAR row1[] = "\x01  Saitek X-52";
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(row1, static_cast<BYTE>(strnlen_s((char*)row1, 17)));
	UCHAR row2[] = "\x02  Driver v11.0";
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(row2, static_cast<BYTE>(strnlen_s((char*)row2, 17)));
	UCHAR row3[] = "\x03 ";
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(row3, static_cast<BYTE>(strnlen_s((char*)row3, 17)));

	if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_Global(&menuMFD.GlobalLight);
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_MFD(&menuMFD.MFDLight);
}

#pragma region "Configuration"
void CMFDMenu::ReadConfiguration()
{
	menuMFD.IsHourActivated = true;
	menuMFD.IsDateActivated = true;

	HANDLE f = CreateFile(L"mfdconf.dat", GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
	if (f != INVALID_HANDLE_VALUE)
	{
		DWORD size;
		SHORT value = 0;
		if (ReadFile(f, &value, 2, &size, NULL))
		{
			menuMFD.IsNXTActivated = static_cast<bool>(value);
			if (ReadFile(f, &value, 2, &size, NULL))
			{
				menuMFD.GlobalLight = static_cast<UCHAR>(value);
				if (ReadFile(f, &value, 2, &size, NULL))
				{
					menuMFD.MFDLight = static_cast<UCHAR>(value);
					if (ReadFile(f, &value, 2, &size, NULL))
					{
						menuMFD.Hour[0].Minutes = value;
						if (ReadFile(f, &value, 2, &size, NULL))
						{
							menuMFD.Hour[1].Minutes = value;
							if (ReadFile(f, &value, 2, &size, NULL))
							{
								menuMFD.Hour[2].Minutes = value;
								if (ReadFile(f, &value, 2, &size, NULL))
								{
									menuMFD.Hour[0]._24h = static_cast<bool>(value);
									if (ReadFile(f, &value, 2, &size, NULL))
									{
										menuMFD.Hour[1]._24h = static_cast<bool>(value);
										if (ReadFile(f, &value, 2, &size, NULL))
											menuMFD.Hour[2]._24h = static_cast<bool>(value);
									}
								}
							}
						}
					}
				}
			}
		}
		CloseHandle(f);
	}
}

void CMFDMenu::SaveConfiguration() const
{
	HANDLE f = CreateFile(L"mfdconf.dat", GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);
	if (f != INVALID_HANDLE_VALUE)
	{
		DWORD size;
		SHORT value = 0;
		value = static_cast<short>(menuMFD.IsNXTActivated);
		if (WriteFile(f, &value, 2, &size, NULL))
		{
			value = static_cast<short>(menuMFD.GlobalLight);
			if (WriteFile(f, &value, 2, &size, NULL))
			{
				value = static_cast<short>(menuMFD.MFDLight);
				if (WriteFile(f, &value, 2, &size, NULL))
				{
					value = menuMFD.Hour[0].Minutes;
					if (WriteFile(f, &value, 2, &size, NULL))
					{
						value = menuMFD.Hour[1].Minutes;
						if (WriteFile(f, &value, 2, &size, NULL))
						{
							value = menuMFD.Hour[2].Minutes;
							if (WriteFile(f, &value, 2, &size, NULL))
							{
								value = static_cast<short>(menuMFD.Hour[0]._24h);
								if (WriteFile(f, &value, 2, &size, NULL))
								{
									value = static_cast<short>(menuMFD.Hour[1]._24h);
									if (WriteFile(f, &value, 2, &size, NULL))
									{
										value = static_cast<short>(menuMFD.Hour[2]._24h);
										WriteFile(f, &value, 2, &size, NULL);
									}
								}
							}
						}
					}
				}
			}
		}
		CloseHandle(f);
	}
}
#pragma endregion

void CALLBACK CMFDMenu::EvtTickHour(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CMFDMenu* local = static_cast<CMFDMenu*>(Context);
		UCHAR bf[3] = { 0, 0, 0 };
		SYSTEMTIME now;
		GetLocalTime(&now);

		if (local->menuMFD.IsDateActivated)
		{
			bf[0] = 1; bf[1] = (UCHAR)now.wDay;
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Date(bf);
			bf[0] = 2; bf[1] = (UCHAR)now.wMonth;
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Date(bf);
			bf[0] = 3; bf[1] = (UCHAR)(now.wYear % 100);
			if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Date(bf);
		}

		if (local->menuMFD.IsHourActivated)
		{
			LONG hour = local->menuMFD.Hour[0].Minutes * 60;
			__int64 absHour = ((hour < 0) ? -1 : 1) * static_cast<__int64>(hour);
			FILETIME nowFT;
			SystemTimeToFileTime(&now, &nowFT);
			LARGE_INTEGER liNowFT{};
			liNowFT.HighPart = nowFT.dwHighDateTime;
			liNowFT.LowPart = nowFT.dwLowDateTime;

			(hour < 0) ? liNowFT.QuadPart -= (absHour * 10000000LL) : liNowFT.QuadPart += (absHour * 10000000LL);
			nowFT.dwHighDateTime = liNowFT.HighPart;
			nowFT.dwLowDateTime = liNowFT.LowPart;
			FileTimeToSystemTime(&nowFT, &now);
			bf[0] = 1;
			bf[1] = (UCHAR)now.wHour;
			bf[2] = (UCHAR)now.wMinute;
			if (local->menuMFD.Hour[0]._24h)
			{
				if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour24(bf);
			}
			else
			{
				if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour(bf);
			}
		}
	}
}

void CALLBACK CMFDMenu::EvtTickMenu(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CMFDMenu* local = static_cast<CMFDMenu*>(Context);
		UCHAR param = 1;

		local->menuMFD.Activated = true;
		local->menuMFD.TimerWaiting = false;
		if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_Info(&param);
		param = 2;
		if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_MFD(&param);
		local->menuMFD.ButtonStatus = 0;
		local->menuMFD.CursorStatus = 0;
		local->menuMFD.PageStatus = 0;
		local->ShowPage1();
	}
}

#pragma region "Menu"
void CMFDMenu::MenuPressButton(UCHAR button)
{
	if (button == static_cast<UCHAR>(Button::Enter))
	{
		if (!menuMFD.TimerWaiting && !menuMFD.Activated)
		{
			menuMFD.TimerWaiting = true;
			LARGE_INTEGER t{};
			t.QuadPart = -30000000LL; //3 segundos
			FILETIME timeout{};
			timeout.dwHighDateTime = t.HighPart;
			timeout.dwLowDateTime = t.LowPart;
			SetThreadpoolTimer(menuMFD.TimerMenu, &timeout, 0, 0);
		}
	}
	if (menuMFD.Activated)
	{
		menuMFD.ButtonStatus |= 1 << button;
	}

}

void CMFDMenu::MenuReleaseButton(UCHAR button)
{
	if (button == static_cast<UCHAR>(Button::Enter))
	{
		if (menuMFD.TimerWaiting && !menuMFD.Activated)
		{
			menuMFD.TimerWaiting = false;
			SetThreadpoolTimer(menuMFD.TimerMenu, NULL, 0, 0);
		}
	}
	if (menuMFD.Activated)
	{
		if ((menuMFD.ButtonStatus >> button) & 1)
		{
			ChangeStatus(button);
		}
		menuMFD.ButtonStatus &= ~((UCHAR)(1 << button));
	}
}

void CMFDMenu::ChangeStatus(UCHAR button)
{
	#pragma region "Enter button"
	if (button == static_cast<UCHAR>(Button::Enter))
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
						ShowPageHour(false, (CHAR)(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[0].Minutes < 0) ? -1 : 1) * menuMFD.Hour[0].Minutes) % 60, menuMFD.Hour[0]._24h);
						break;
					case 4:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 5;
						ShowPageHour(false, (CHAR)(menuMFD.Hour[1].Minutes / 60), (((menuMFD.Hour[1].Minutes < 0) ? -1 : 1) * menuMFD.Hour[1].Minutes) % 60, menuMFD.Hour[1]._24h);
						break;
					case 5:
						menuMFD.CursorStatus = 0;
						menuMFD.PageStatus = 6;
						ShowPageHour(false, (CHAR)(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[2].Minutes < 0) ? -1 : 1) * menuMFD.Hour[2].Minutes) % 60, menuMFD.Hour[2]._24h);
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
				if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_Global(&menuMFD.GlobalLight);
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
					ShowPageHour(true, (CHAR)(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[0].Minutes < 0) ? -1 : 1) * menuMFD.Hour[0].Minutes) % 60, menuMFD.Hour[0]._24h);
				}
				break;
			case 40:
			case 41:
			case 42:
				menuMFD.PageStatus = 4;
				ShowPageHour(false, (CHAR)(menuMFD.Hour[0].Minutes / 60), (((menuMFD.Hour[0].Minutes < 0) ? -1 : 1) * menuMFD.Hour[0].Minutes) % 60, menuMFD.Hour[0]._24h);
				break;
			case 50:
			case 51:
			case 52:
				{
					UCHAR buffer[3] = { 2, 0, 0 };
					if (menuMFD.Hour[1].Minutes < 0)
					{
						buffer[1] = (UCHAR)(((-menuMFD.Hour[1].Minutes) >> 8) + 4);
						buffer[2] = (UCHAR)((-menuMFD.Hour[1].Minutes) & 0xff);
					}
					else
					{
						buffer[1] = (UCHAR)(menuMFD.Hour[1].Minutes >> 8);
						buffer[2] = (UCHAR)(menuMFD.Hour[1].Minutes & 0xff);
					}
					if (menuMFD.Hour[1]._24h)
					{
						if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour24(buffer);
					}
					else
					{
						if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour24(buffer);
					}

					menuMFD.PageStatus = 5;
					ShowPageHour(false, (CHAR)(menuMFD.Hour[1].Minutes / 60), (((menuMFD.Hour[1].Minutes < 0) ? -1 : 1) * menuMFD.Hour[1].Minutes) % 60, menuMFD.Hour[1]._24h);
				}
				break;
			case 60:
			case 61:
			case 62:
				{
					UCHAR buffer[3] = { 3, 0, 0 };
					if (menuMFD.Hour[2].Minutes < 0)
					{
						buffer[1] = (UCHAR)(((-menuMFD.Hour[2].Minutes) >> 8) + 4);
						buffer[2] = (UCHAR)((-menuMFD.Hour[2].Minutes) & 0xff);
					}
					else
					{
						buffer[1] = (UCHAR)(menuMFD.Hour[2].Minutes >> 8);
						buffer[2] = (UCHAR)(menuMFD.Hour[2].Minutes & 0xff);
					}
					if (menuMFD.Hour[2]._24h)
					{
						if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour24(buffer);
					}
					else
					{
						if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Hour24(buffer);
					}
					menuMFD.PageStatus = 6;
					ShowPageHour(false, (CHAR)(menuMFD.Hour[2].Minutes / 60), (((menuMFD.Hour[2].Minutes < 0) ? -1 : 1) * menuMFD.Hour[2].Minutes) % 60, menuMFD.Hour[2]._24h);
				}
				break;
		}
	}
	#pragma endregion

	#pragma region "button up"
	else if (button == static_cast<UCHAR>(Button::Up))
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
				ShowPageHour(false, (CHAR)(menuMFD.Hour[menuMFD.PageStatus - 4].Minutes / 60), (((menuMFD.Hour[menuMFD.PageStatus - 4].Minutes < 0) ? -1 : 1) * menuMFD.Hour[menuMFD.PageStatus - 4].Minutes) % 60, menuMFD.Hour[menuMFD.PageStatus - 4]._24h);
				break;
			case 40:
			case 50:
			case 60:
				{
					CHAR idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes / 60) != 23)
					{
						menuMFD.Hour[idx].Minutes += 60;
					}
					ShowPageHour(true, (CHAR)(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 41:
			case 51:
			case 61:
				{
					CHAR idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes % 60) != 59)
					{
						menuMFD.Hour[idx].Minutes++;
					}
					ShowPageHour(true, (CHAR)(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 42:
			case 52:
			case 62:
				{
					CHAR idx = (menuMFD.PageStatus / 10) - 4;
					menuMFD.Hour[idx]._24h = ~menuMFD.Hour[idx]._24h;
					ShowPageHour(true, (CHAR)(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
		}
	}
	#pragma endregion

	#pragma region "button down"
	else if (button == static_cast<UCHAR>(Button::Down))
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
				ShowPageHour(false, (CHAR)(menuMFD.Hour[menuMFD.PageStatus - 4].Minutes / 60), (((menuMFD.Hour[menuMFD.PageStatus - 4].Minutes < 0) ? -1 : 1) * menuMFD.Hour[menuMFD.PageStatus - 4].Minutes) % 60, menuMFD.Hour[menuMFD.PageStatus - 4]._24h);
				break;
			case 40:
			case 50:
			case 60:
				{
					CHAR idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes / 60) != -23)
					{
						menuMFD.Hour[idx].Minutes -= 60;
					}
					ShowPageHour(true, (CHAR)(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 41:
			case 51:
			case 61:
				{
					CHAR idx = (menuMFD.PageStatus / 10) - 4;
					if ((menuMFD.Hour[0].Minutes % 60) != 0)
					{
						menuMFD.Hour[idx].Minutes--;
					}
					ShowPageHour(true, (CHAR)(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
			case 42:
			case 52:
			case 62:
				{
					CHAR idx = (menuMFD.PageStatus / 10) - 4;
					menuMFD.Hour[idx]._24h = ~menuMFD.Hour[idx]._24h;
					ShowPageHour(true, (CHAR)(menuMFD.Hour[idx].Minutes / 60), (((menuMFD.Hour[idx].Minutes < 0) ? -1 : 1) * menuMFD.Hour[idx].Minutes) % 60, menuMFD.Hour[idx]._24h);
				}
				break;
		}
	}
	#pragma endregion
}

void CMFDMenu::CloseMenu()
{
	//clear screen
	UCHAR row[2] = { 1, 0 };
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(row, 2);
	row[0] = 2;
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(row, 2);
	row[0] = 3;
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text(row, 2);

	//turn of light
	row[0] = 0;
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_Info(&row[0]);

	if (CX52Write::Get() != nullptr) CX52Write::Get()->Light_MFD(&menuMFD.MFDLight);

	SaveConfiguration();
	menuMFD.Activated = false;
}
#pragma endregion

#pragma region "Pages"
void  CMFDMenu::ShowPage1() const
{
	UCHAR cursor = menuMFD.CursorStatus % 3;
	UCHAR page = menuMFD.CursorStatus / 3;
	CHAR f1[16], f2[16], f3[16], f4[16], f5[16], f6[16], f7[16], f8[16];
	CHAR* rows[9]{f1, f2, f3, f4, f5, f6, f7, f8, f8};

	RtlCopyMemory(f1, " Gladiator NXT  ", 16);
	RtlCopyMemory(f2, " Luz botones    ", 16);
	RtlCopyMemory(f3, " Luz MFD        ", 16);
	RtlCopyMemory(f4, " Hora 1         ", 16);
	RtlCopyMemory(f5, " Hora 2         ", 16);
	RtlCopyMemory(f6, " Hora 3         ", 16); //251
	RtlCopyMemory(f7, " Salir          ", 16); //252
	RtlCopyMemory(f8, "                ", 16);

	rows[cursor + (page * 3)][0] = '>';

	for (UCHAR i = 0; i < 3; i++)
	{
		CHAR* text = rows[i + (page * 3)];
		CHAR buffer[17]{};
		for (UCHAR c = 0; c < 16; c++)
		{
			buffer[c + 1] = text[c];
		}

		buffer[0] = i + 1;

		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text((UCHAR*)buffer, 17);
	}
}

void CMFDMenu::ShowPageOnOff()
{
	UCHAR cursor = menuMFD.CursorStatus;
	CHAR f1[8], f2[8];
	CHAR* rows[2]{ f1, f2 };

	RtlCopyMemory(f1, " On     ", 8);
	RtlCopyMemory(f2, " Off    ", 8);

	rows[cursor][0] = '>';

	rows[(menuMFD.IsNXTActivated) ? 0 : 1][5] = '(';
	rows[(menuMFD.IsNXTActivated) ? 0 : 1][6] = '*';
	rows[(menuMFD.IsNXTActivated) ? 0 : 1][7] = ')';

	for (UCHAR i = 0; i < 2; i++)
	{
		CHAR* text = rows[i];
		CHAR buffer[9]{};
		for (UCHAR c = 0; c < 8; c++)
		{
			buffer[c + 1] = text[c];
		}
		buffer[0] = i + 1;
		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text((UCHAR*)buffer, 9);
	}
	f1[0] = 3; //row 3 empty
	if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text((UCHAR*)f1, 1);
}

void CMFDMenu::ShowPageLight(UCHAR status) const
{
	UCHAR cursor = menuMFD.CursorStatus;
	CHAR f1[10], f2[10], f3[10];
	CHAR* rows[3]{f1, f2, f3};

	RtlCopyMemory(f1, " Low      ", 10);
	RtlCopyMemory(f2, " Medium   ", 10);
	RtlCopyMemory(f3, " High     ", 10);

	rows[cursor][0] = '>';

	rows[status][7] = '(';
	rows[status][8] = '*';
	rows[status][9] = ')';

	for (UCHAR i = 0; i < 3; i++)
	{
		CHAR* text = rows[i];
		CHAR buffer[11]{};
		for (UCHAR c = 0; c < 10; c++)
		{
			buffer[c + 1] = text[c];
		}
		buffer[0] = (i + 1);

		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text((UCHAR*)buffer, 11);
	}
}

void CMFDMenu::ShowPageHour(bool sel, CHAR hour, UCHAR minute, bool h24) const
{
	UCHAR cursor = menuMFD.CursorStatus % 3;
	UCHAR page = menuMFD.CursorStatus / 3;
	CHAR f1[11], f2[11], f3[11], f4[11], f5[11];
	CHAR* rows[6]{ f1, f2, f3, f4, f5};

	RtlCopyMemory(f1, " Hour:     ", 11);
	RtlCopyMemory(f2, " Minute:   ", 11);
	RtlCopyMemory(f3, " AM/PM:    ", 11);
	RtlCopyMemory(f4, " Back      ", 11);
	RtlCopyMemory(f5, "           ", 11);

	rows[cursor + (page * 3)][0] = (sel) ? '*' : '>';

	sprintf_s(&f1[7], 2, "%02d", hour);
	sprintf_s(&f2[9], 2, "%02u", minute);
	RtlCopyMemory(&f3[8], (h24) ? "No " : "Yes", 3);

	for (UCHAR i = 0; i < 3; i++)
	{
		CHAR* text = rows[i + (page * 3)];
		CHAR buffer[12]{};

		buffer[0] = i + 1;
		for (UCHAR c = 0; c < 11; c++)
		{
			buffer[c + 1] = text[c];
		}

		if (CX52Write::Get() != nullptr) CX52Write::Get()->Set_Text((UCHAR*)buffer, 12);
	}
}
#pragma endregion
