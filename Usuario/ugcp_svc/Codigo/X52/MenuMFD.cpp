#include "../framework.h"
#include "MenuMFD.h"
#include "EscribirUSBX52.h"

CMenuMFD* CMenuMFD::pLocal = nullptr;

CMenuMFD::CMenuMFD()
{
	pLocal = this;
	RtlZeroMemory(&menuMFD, sizeof(menuMFD));
	LeerConfiguracion();
	menuMFD.TimerMenu = CreateThreadpoolTimer(EvtTickMenu, this, NULL);
	menuMFD.TimerHora = CreateThreadpoolTimer(EvtTickHora, this, NULL);

	LARGE_INTEGER t{};
	t.QuadPart = -40000000LL; //4 segundos
	FILETIME timeout{};
	timeout.dwHighDateTime = t.HighPart;
	timeout.dwLowDateTime = t.LowPart;
	SetThreadpoolTimer(menuMFD.TimerHora, &timeout, 4000, 0);
}

CMenuMFD::~CMenuMFD()
{
	pLocal = nullptr;
	SetThreadpoolTimer(menuMFD.TimerHora, NULL, 0, 0);
	CloseThreadpoolTimer(menuMFD.TimerHora);
	SetThreadpoolTimer(menuMFD.TimerMenu, NULL, 0, 0);
	CloseThreadpoolTimer(menuMFD.TimerMenu);
	if (menuMFD.Activado)
	{
		MenuCerrar();
	}
}

void CMenuMFD::SetInicio()
{
	UCHAR fila1[] = "\x01  Saitek X-52";
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(fila1, static_cast<BYTE>(strnlen_s((char*)fila1, 17)));
	UCHAR fila2[] = "\x02  Driver v10.0";
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(fila2, static_cast<BYTE>(strnlen_s((char*)fila2, 17)));
	UCHAR fila3[] = "\x03 ";
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(fila3, static_cast<BYTE>(strnlen_s((char*)fila3, 17)));

	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_Global(&menuMFD.LuzGlobal);
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_MFD(&menuMFD.LuzMFD);
}

#pragma region "Configuración"
void CMenuMFD::LeerConfiguracion()
{
	menuMFD.HoraActivada = true;
	menuMFD.FechaActivada = true;

	HANDLE f = CreateFile(L"mfdconf.dat", GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);
	if (f != INVALID_HANDLE_VALUE)
	{
		DWORD tam;
		SHORT valor = 0;
		if (ReadFile(f, &valor, 2, &tam, NULL))
		{
			menuMFD.NXTActivado = static_cast<bool>(valor);
			if (ReadFile(f, &valor, 2, &tam, NULL))
			{
				menuMFD.LuzGlobal = static_cast<UCHAR>(valor);
				if (ReadFile(f, &valor, 2, &tam, NULL))
				{
					menuMFD.LuzMFD = static_cast<UCHAR>(valor);
					if (ReadFile(f, &valor, 2, &tam, NULL))
					{
						menuMFD.Hora[0].Minutos = valor;
						if (ReadFile(f, &valor, 2, &tam, NULL))
						{
							menuMFD.Hora[1].Minutos = valor;
							if (ReadFile(f, &valor, 2, &tam, NULL))
							{
								menuMFD.Hora[2].Minutos = valor;
								if (ReadFile(f, &valor, 2, &tam, NULL))
								{
									menuMFD.Hora[0]._24h = static_cast<bool>(valor);
									if (ReadFile(f, &valor, 2, &tam, NULL))
									{
										menuMFD.Hora[1]._24h = static_cast<bool>(valor);
										if (ReadFile(f, &valor, 2, &tam, NULL))
											menuMFD.Hora[2]._24h = static_cast<bool>(valor);
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

void CMenuMFD::GuardarConfiguracion()
{
	HANDLE f = CreateFile(L"mfdconf.dat", GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, NULL);
	if (f != INVALID_HANDLE_VALUE)
	{
		DWORD tam;
		SHORT valor = 0;
		valor = static_cast<short>(menuMFD.NXTActivado);
		if (WriteFile(f, &valor, 2, &tam, NULL))
		{
			valor = static_cast<short>(menuMFD.LuzGlobal);
			if (WriteFile(f, &valor, 2, &tam, NULL))
			{
				valor = static_cast<short>(menuMFD.LuzMFD);
				if (WriteFile(f, &valor, 2, &tam, NULL))
				{
					valor = menuMFD.Hora[0].Minutos;
					if (WriteFile(f, &valor, 2, &tam, NULL))
					{
						valor = menuMFD.Hora[1].Minutos;
						if (WriteFile(f, &valor, 2, &tam, NULL))
						{
							valor = menuMFD.Hora[2].Minutos;
							if (WriteFile(f, &valor, 2, &tam, NULL))
							{
								valor = static_cast<short>(menuMFD.Hora[0]._24h);
								if (WriteFile(f, &valor, 2, &tam, NULL))
								{
									valor = static_cast<short>(menuMFD.Hora[1]._24h);
									if (WriteFile(f, &valor, 2, &tam, NULL))
									{
										valor = static_cast<short>(menuMFD.Hora[2]._24h);
										WriteFile(f, &valor, 2, &tam, NULL);
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

void CALLBACK CMenuMFD::EvtTickHora(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CMenuMFD* local = static_cast<CMenuMFD*>(Context);
		UCHAR bf[3] = { 0, 0, 0 };
		SYSTEMTIME ahora;
		GetLocalTime(&ahora);

		if (local->menuMFD.FechaActivada)
		{
			bf[0] = 1; bf[1] = (UCHAR)ahora.wDay;
			if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Fecha(bf);
			bf[0] = 2; bf[1] = (UCHAR)ahora.wMonth;
			if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Fecha(bf);
			bf[0] = 3; bf[1] = (UCHAR)(ahora.wYear % 100);
			if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Fecha(bf);
		}

		if (local->menuMFD.HoraActivada)
		{
			LONG hora = local->menuMFD.Hora[0].Minutos * 60;
			__int64 absHora = ((hora < 0) ? -1 : 1) * static_cast<__int64>(hora);
			FILETIME ahoraft;
			SystemTimeToFileTime(&ahora, &ahoraft);
			LARGE_INTEGER uahoraft{};
			uahoraft.HighPart = ahoraft.dwHighDateTime;
			uahoraft.LowPart = ahoraft.dwLowDateTime;

			(hora < 0) ? uahoraft.QuadPart -= (absHora * 10000000LL) : uahoraft.QuadPart += (absHora * 10000000LL);
			ahoraft.dwHighDateTime = uahoraft.HighPart;
			ahoraft.dwLowDateTime = uahoraft.LowPart;
			FileTimeToSystemTime(&ahoraft, &ahora);
			bf[0] = 1;
			bf[1] = (UCHAR)ahora.wHour;
			bf[2] = (UCHAR)ahora.wMinute;
			if (local->menuMFD.Hora[0]._24h)
			{
				if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Hora24(bf);
			}
			else
			{
				if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Hora(bf);
			}
		}
	}
}

void CALLBACK CMenuMFD::EvtTickMenu(_Inout_ PTP_CALLBACK_INSTANCE Instance, _Inout_opt_ PVOID Context, _Inout_ PTP_TIMER Timer)
{
	if (Context != NULL)
	{
		CMenuMFD* local = static_cast<CMenuMFD*>(Context);
		UCHAR param = 1;

		local->menuMFD.Activado = true;
		local->menuMFD.TimerEsperando = false;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_Info(&param);
		param = 2;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_MFD(&param);
		local->menuMFD.EstadoBotones = 0;
		local->menuMFD.EstadoCursor = 0;
		local->menuMFD.EstadoPagina = 0;
		local->VerPantalla1();
	}
}

#pragma region "Menu"
void CMenuMFD::MenuPulsarBoton(UCHAR boton)
{
	if (boton == static_cast<UCHAR>(Boton::Intro))
	{
		if (!menuMFD.TimerEsperando && !menuMFD.Activado)
		{
			menuMFD.TimerEsperando = true;
			LARGE_INTEGER t{};
			t.QuadPart = -30000000LL; //3 segundos
			FILETIME timeout{};
			timeout.dwHighDateTime = t.HighPart;
			timeout.dwLowDateTime = t.LowPart;
			SetThreadpoolTimer(menuMFD.TimerMenu, &timeout, 0, 0);
		}
	}
	if (menuMFD.Activado)
	{
		menuMFD.EstadoBotones |= 1 << boton;
	}

}

void CMenuMFD::MenuSoltarBoton(UCHAR boton)
{
	if (boton == static_cast<UCHAR>(Boton::Intro))
	{
		if (menuMFD.TimerEsperando && !menuMFD.Activado)
		{
			menuMFD.TimerEsperando = false;
			SetThreadpoolTimer(menuMFD.TimerMenu, NULL, 0, 0);
		}
	}
	if (menuMFD.Activado)
	{
		if ((menuMFD.EstadoBotones >> boton) & 1)
		{
			CambiarEstado(boton);
		}
		menuMFD.EstadoBotones &= ~((UCHAR)(1 << boton));
	}
}

void CMenuMFD::CambiarEstado(UCHAR boton)
{
	#pragma region "Boton intro"
	if (boton == static_cast<UCHAR>(Boton::Intro))
	{
		switch (menuMFD.EstadoPagina)
		{
			case 0: //principal
				switch (menuMFD.EstadoCursor)
				{
					case 0:
						menuMFD.EstadoCursor = 0;
						menuMFD.EstadoPagina = 1;
						VerPantallaOnOff();
						break;
					case 1:
						menuMFD.EstadoCursor = 0;
						menuMFD.EstadoPagina = 2;
						VerPantallaLuz(menuMFD.LuzGlobal);
						break;
					case 2:
						menuMFD.EstadoCursor = 0;
						menuMFD.EstadoPagina = 3;
						VerPantallaLuz(menuMFD.LuzMFD);
						break;
					case 3:
						menuMFD.EstadoCursor = 0;
						menuMFD.EstadoPagina = 4;
						VerPantallaHora(false, (CHAR)(menuMFD.Hora[0].Minutos / 60), (((menuMFD.Hora[0].Minutos < 0) ? -1 : 1) * menuMFD.Hora[0].Minutos) % 60, menuMFD.Hora[0]._24h);
						break;
					case 4:
						menuMFD.EstadoCursor = 0;
						menuMFD.EstadoPagina = 5;
						VerPantallaHora(false, (CHAR)(menuMFD.Hora[1].Minutos / 60), (((menuMFD.Hora[1].Minutos < 0) ? -1 : 1) * menuMFD.Hora[1].Minutos) % 60, menuMFD.Hora[1]._24h);
						break;
					case 5:
						menuMFD.EstadoCursor = 0;
						menuMFD.EstadoPagina = 6;
						VerPantallaHora(false, (CHAR)(menuMFD.Hora[0].Minutos / 60), (((menuMFD.Hora[2].Minutos < 0) ? -1 : 1) * menuMFD.Hora[2].Minutos) % 60, menuMFD.Hora[2]._24h);
						break;
					case 6:
						MenuCerrar();
						break;
				}
				break;
			case 1: //pedales
				menuMFD.NXTActivado = (menuMFD.EstadoCursor == 0);
				menuMFD.EstadoCursor = 0;
				menuMFD.EstadoPagina = 0;
				VerPantalla1();
				break;
			case 2: // luz global
				menuMFD.LuzGlobal = menuMFD.EstadoCursor;
				if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_Global(&menuMFD.LuzGlobal);
				menuMFD.EstadoCursor = 1;
				menuMFD.EstadoPagina = 0;
				VerPantalla1();
				break;
			case 3: //luz mfd
				menuMFD.LuzMFD = menuMFD.EstadoCursor;
				menuMFD.EstadoCursor = 2;
				menuMFD.EstadoPagina = 0;
				VerPantalla1();
				break;
			case 4: //hora 1
			case 5: //hora 2
			case 6: //hora 3
				if (menuMFD.EstadoCursor == 3)
				{
					menuMFD.EstadoCursor = menuMFD.EstadoPagina - 1;
					menuMFD.EstadoPagina = 0;
					VerPantalla1();
				}
				else
				{
					menuMFD.EstadoPagina = (menuMFD.EstadoPagina * 10 + menuMFD.EstadoCursor);
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[0].Minutos / 60), (((menuMFD.Hora[0].Minutos < 0) ? -1 : 1) * menuMFD.Hora[0].Minutos) % 60, menuMFD.Hora[0]._24h);
				}
				break;
			case 40:
			case 41:
			case 42:
				menuMFD.EstadoPagina = 4;
				VerPantallaHora(false, (CHAR)(menuMFD.Hora[0].Minutos / 60), (((menuMFD.Hora[0].Minutos < 0) ? -1 : 1) * menuMFD.Hora[0].Minutos) % 60, menuMFD.Hora[0]._24h);
				break;
			case 50:
			case 51:
			case 52:
				{
					UCHAR buffer[3] = { 2, 0, 0 };
					if (menuMFD.Hora[1].Minutos < 0)
					{
						buffer[1] = (UCHAR)(((-menuMFD.Hora[1].Minutos) >> 8) + 4);
						buffer[2] = (UCHAR)((-menuMFD.Hora[1].Minutos) & 0xff);
					}
					else
					{
						buffer[1] = (UCHAR)(menuMFD.Hora[1].Minutos >> 8);
						buffer[2] = (UCHAR)(menuMFD.Hora[1].Minutos & 0xff);
					}
					if (menuMFD.Hora[1]._24h)
					{
						if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Hora24(buffer);
					}
					else
					{
						if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Hora24(buffer);
					}

					menuMFD.EstadoPagina = 5;
					VerPantallaHora(false, (CHAR)(menuMFD.Hora[1].Minutos / 60), (((menuMFD.Hora[1].Minutos < 0) ? -1 : 1) * menuMFD.Hora[1].Minutos) % 60, menuMFD.Hora[1]._24h);
				}
				break;
			case 60:
			case 61:
			case 62:
				{
					UCHAR buffer[3] = { 3, 0, 0 };
					if (menuMFD.Hora[2].Minutos < 0)
					{
						buffer[1] = (UCHAR)(((-menuMFD.Hora[2].Minutos) >> 8) + 4);
						buffer[2] = (UCHAR)((-menuMFD.Hora[2].Minutos) & 0xff);
					}
					else
					{
						buffer[1] = (UCHAR)(menuMFD.Hora[2].Minutos >> 8);
						buffer[2] = (UCHAR)(menuMFD.Hora[2].Minutos & 0xff);
					}
					if (menuMFD.Hora[2]._24h)
					{
						if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Hora24(buffer);
					}
					else
					{
						if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Hora24(buffer);
					}
					menuMFD.EstadoPagina = 6;
					VerPantallaHora(false, (CHAR)(menuMFD.Hora[2].Minutos / 60), (((menuMFD.Hora[2].Minutos < 0) ? -1 : 1) * menuMFD.Hora[2].Minutos) % 60, menuMFD.Hora[2]._24h);
				}
				break;
		}
	}
	#pragma endregion

	#pragma region "botón arriba"
	else if (boton == static_cast<UCHAR>(Boton::Arriba))
	{
		switch (menuMFD.EstadoPagina)
		{
			case 0:
				if (menuMFD.EstadoCursor != 0)
				{
					menuMFD.EstadoCursor--;
				}
				VerPantalla1();
				break;
			case 1:
				if (menuMFD.EstadoCursor != 0)
				{
					menuMFD.EstadoCursor--;
				}
				VerPantallaOnOff();
				break;
			case 2:
			case 3:
				if (menuMFD.EstadoCursor != 0)
				{
					menuMFD.EstadoCursor--;
				}
				VerPantallaLuz(menuMFD.EstadoCursor);
				break;
			case 4:
			case 5:
			case 6:
				if (menuMFD.EstadoCursor != 0)
				{
					menuMFD.EstadoCursor--;
				}
				VerPantallaHora(false, (CHAR)(menuMFD.Hora[menuMFD.EstadoPagina - 4].Minutos / 60), (((menuMFD.Hora[menuMFD.EstadoPagina - 4].Minutos < 0) ? -1 : 1) * menuMFD.Hora[menuMFD.EstadoPagina - 4].Minutos) % 60, menuMFD.Hora[menuMFD.EstadoPagina - 4]._24h);
				break;
			case 40:
			case 50:
			case 60:
				{
					CHAR idx = (menuMFD.EstadoPagina / 10) - 4;
					if ((menuMFD.Hora[0].Minutos / 60) != 23)
					{
						menuMFD.Hora[idx].Minutos += 60;
					}
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[idx].Minutos / 60), (((menuMFD.Hora[idx].Minutos < 0) ? -1 : 1) * menuMFD.Hora[idx].Minutos) % 60, menuMFD.Hora[idx]._24h);
				}
				break;
			case 41:
			case 51:
			case 61:
				{
					CHAR idx = (menuMFD.EstadoPagina / 10) - 4;
					if ((menuMFD.Hora[0].Minutos % 60) != 59)
					{
						menuMFD.Hora[idx].Minutos++;
					}
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[idx].Minutos / 60), (((menuMFD.Hora[idx].Minutos < 0) ? -1 : 1) * menuMFD.Hora[idx].Minutos) % 60, menuMFD.Hora[idx]._24h);
				}
				break;
			case 42:
			case 52:
			case 62:
				{
					CHAR idx = (menuMFD.EstadoPagina / 10) - 4;
					menuMFD.Hora[idx]._24h = !menuMFD.Hora[idx]._24h;
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[idx].Minutos / 60), (((menuMFD.Hora[idx].Minutos < 0) ? -1 : 1) * menuMFD.Hora[idx].Minutos) % 60, menuMFD.Hora[idx]._24h);
				}
				break;
		}
	}
	#pragma endregion

	#pragma region "botón abajo"
	else if (boton == static_cast<UCHAR>(Boton::Abajo))
	{
		switch (menuMFD.EstadoPagina)
		{
			case 0:
				if (menuMFD.EstadoCursor != 6)
				{
					menuMFD.EstadoCursor++;
				}
				VerPantalla1();
				break;
			case 1:
				if (menuMFD.EstadoCursor != 1)
				{
					menuMFD.EstadoCursor++;
				}
				VerPantallaOnOff();
				break;
			case 2:
			case 3:
				if (menuMFD.EstadoCursor != 2)
				{
					menuMFD.EstadoCursor++;
				}
				VerPantallaLuz((menuMFD.EstadoPagina == 2) ? menuMFD.LuzGlobal : menuMFD.LuzMFD);
				break;
			case 4:
			case 5:
			case 6:
				if (menuMFD.EstadoCursor != 3)
				{
					menuMFD.EstadoCursor++;
				}
				VerPantallaHora(false, (CHAR)(menuMFD.Hora[menuMFD.EstadoPagina - 4].Minutos / 60), (((menuMFD.Hora[menuMFD.EstadoPagina - 4].Minutos < 0) ? -1 : 1) * menuMFD.Hora[menuMFD.EstadoPagina - 4].Minutos) % 60, menuMFD.Hora[menuMFD.EstadoPagina - 4]._24h);
				break;
			case 40:
			case 50:
			case 60:
				{
					CHAR idx = (menuMFD.EstadoPagina / 10) - 4;
					if ((menuMFD.Hora[0].Minutos / 60) != -23)
					{
						menuMFD.Hora[idx].Minutos -= 60;
					}
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[idx].Minutos / 60), (((menuMFD.Hora[idx].Minutos < 0) ? -1 : 1) * menuMFD.Hora[idx].Minutos) % 60, menuMFD.Hora[idx]._24h);
				}
				break;
			case 41:
			case 51:
			case 61:
				{
					CHAR idx = (menuMFD.EstadoPagina / 10) - 4;
					if ((menuMFD.Hora[0].Minutos % 60) != 0)
					{
						menuMFD.Hora[idx].Minutos--;
					}
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[idx].Minutos / 60), (((menuMFD.Hora[idx].Minutos < 0) ? -1 : 1) * menuMFD.Hora[idx].Minutos) % 60, menuMFD.Hora[idx]._24h);
				}
				break;
			case 42:
			case 52:
			case 62:
				{
					CHAR idx = (menuMFD.EstadoPagina / 10) - 4;
					menuMFD.Hora[idx]._24h = !menuMFD.Hora[idx]._24h;
					VerPantallaHora(true, (CHAR)(menuMFD.Hora[idx].Minutos / 60), (((menuMFD.Hora[idx].Minutos < 0) ? -1 : 1) * menuMFD.Hora[idx].Minutos) % 60, menuMFD.Hora[idx]._24h);
				}
				break;
		}
	}
	#pragma endregion
}

void CMenuMFD::MenuCerrar()
{
	//Limpiar pantalla
	UCHAR fila[2] = { 1, 0 };
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(fila, 2);
	fila[0] = 2;
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(fila, 2);
	fila[0] = 3;
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(fila, 2);

	//Apagar luz
	fila[0] = 0;
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_Info(&fila[0]);

	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Luz_MFD(&menuMFD.LuzMFD);

	GuardarConfiguracion();
	menuMFD.Activado = false;
}
#pragma endregion

#pragma region "Pantallas"
void  CMenuMFD::VerPantalla1()
{
	UCHAR cursor = menuMFD.EstadoCursor % 3;
	UCHAR pagina = menuMFD.EstadoCursor / 3;
	CHAR f1[16], f2[16], f3[16], f4[16], f5[16], f6[16], f7[16], f8[16];
	CHAR* filas[9]{f1, f2, f3, f4, f5, f6, f7, f8, f8};

	RtlCopyMemory(f1, " Gladiator NXT  ", 16);
	RtlCopyMemory(f2, " Luz botones    ", 16);
	RtlCopyMemory(f3, " Luz MFD        ", 16);
	RtlCopyMemory(f4, " Hora 1         ", 16);
	RtlCopyMemory(f5, " Hora 2         ", 16);
	RtlCopyMemory(f6, " Hora 3         ", 16); //251
	RtlCopyMemory(f7, " Salir          ", 16); //252
	RtlCopyMemory(f8, "                ", 16);

	filas[cursor + (pagina * 3)][0] = '>';

	for (UCHAR i = 0; i < 3; i++)
	{
		CHAR* texto = filas[i + (pagina * 3)];
		CHAR buffer[17]{};
		for (UCHAR c = 0; c < 16; c++)
		{
			buffer[c + 1] = texto[c];
		}

		buffer[0] = i + 1;

		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto((UCHAR*)buffer, 17);
	}
}

void CMenuMFD::VerPantallaOnOff()
{
	UCHAR cursor = menuMFD.EstadoCursor;
	CHAR f1[8], f2[8];
	CHAR* filas[2]{ f1, f2 };

	RtlCopyMemory(f1, " On     ", 8);
	RtlCopyMemory(f2, " Off    ", 8);

	filas[cursor][0] = '>';

	filas[(menuMFD.NXTActivado) ? 0 : 1][5] = '(';
	filas[(menuMFD.NXTActivado) ? 0 : 1][6] = '*';
	filas[(menuMFD.NXTActivado) ? 0 : 1][7] = ')';

	for (UCHAR i = 0; i < 2; i++)
	{
		CHAR* texto = filas[i];
		CHAR buffer[9]{};
		for (UCHAR c = 0; c < 8; c++)
		{
			buffer[c + 1] = texto[c];
		}
		buffer[0] = i + 1;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto((UCHAR*)buffer, 9);
	}
	f1[0] = 3; //file 3 vacía
	if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto((UCHAR*)f1, 1);
}

void CMenuMFD::VerPantallaLuz(UCHAR estado)
{
	UCHAR cursor = menuMFD.EstadoCursor;
	CHAR f1[10], f2[10], f3[10];
	CHAR* filas[3]{f1, f2, f3};

	RtlCopyMemory(f1, " Bajo     ", 10);
	RtlCopyMemory(f2, " Medio    ", 10);
	RtlCopyMemory(f3, " Alto     ", 10);

	filas[cursor][0] = '>';

	filas[estado][7] = '(';
	filas[estado][8] = '*';
	filas[estado][9] = ')';

	for (UCHAR i = 0; i < 3; i++)
	{
		CHAR* texto = filas[i];
		CHAR buffer[11]{};
		for (UCHAR c = 0; c < 10; c++)
		{
			buffer[c + 1] = texto[c];
		}
		buffer[0] = (i + 1);

		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto((UCHAR*)buffer, 11);
	}
}

void CMenuMFD::VerPantallaHora(bool sel, CHAR hora, UCHAR minuto, bool h24)
{
	UCHAR cursor = menuMFD.EstadoCursor % 3;
	UCHAR pagina = menuMFD.EstadoCursor / 3;
	CHAR f1[11], f2[11], f3[11], f4[11], f5[11];
	CHAR* filas[6]{ f1, f2, f3, f4, f5};

	RtlCopyMemory(f1, " Hora:     ", 11);
	RtlCopyMemory(f2, " Minuto:   ", 11);
	RtlCopyMemory(f3, " AM/PM:    ", 11);
	RtlCopyMemory(f4, " Volver    ", 11);
	RtlCopyMemory(f5, "           ", 11);

	filas[cursor + (pagina * 3)][0] = (sel) ? '*' : '>';

	sprintf_s(&f1[7], 2, "%02d", hora);
	sprintf_s(&f2[9], 2, "%02u", minuto);
	RtlCopyMemory(&f3[8], (h24) ? "No" : "Si", 2);

	for (UCHAR i = 0; i < 3; i++)
	{
		CHAR* texto = filas[i + (pagina * 3)];
		CHAR buffer[12]{};

		buffer[0] = i + 1;
		for (UCHAR c = 0; c < 11; c++)
		{
			buffer[c + 1] = texto[c];
		}

		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto((UCHAR*)buffer, 12);
	}
}
#pragma endregion
