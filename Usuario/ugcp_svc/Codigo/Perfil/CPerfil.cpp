#include "../framework.h"
#include <setupapi.h>
#include <initguid.h>
#include "CPerfil.h"
#include "../ColaEventos/CColaEventos.h"
#include "../ProcesarSalida/CProcesarSalida.h"
#include "../X52/EscribirUSBX52.h"
#include "../X52/MenuMFD.h"
#include "../ProcesarSalida/CVirtualHID.h"

static const UCHAR vJoyReport[142] = {
	0x05, 0x01,        // Usage Page (Generic Desktop Ctrls)
	0x15, 0x00,        // Logical Minimum (0)
	0x09, 0x04,        // Usage (Joystick)
	0xA1, 0x01,        // Collection (Application)
	0x05, 0x01,        //   Usage Page (Generic Desktop Ctrls)
	0x85, 0x01,        //   Report ID (1)
	0x09, 0x01,        //   Usage (Pointer)
	0x15, 0x00,        //   Logical Minimum (0)
	0x75, 0x20,        //   Report Size (32)
	0x95, 0x01,        //   Report Count (1)
	0xA1, 0x00,        //   Collection (Physical)
	0x09, 0x30,        //     Usage (X)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x31,        //     Usage (Y)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x32,        //     Usage (Z)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x33,        //     Usage (Rx)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x34,        //     Usage (Ry)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x35,        //     Usage (Rz)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x36,        //     Usage (Slider)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x37,        //     Usage (Dial)
	0x26, 0xFF, 0x7F,  //   Logical Maximum (32767)
	0x81, 0x02,        //     Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0xC0,              //   End Collection
	0x15, 0x00,        //   Logical Minimum (0)
	0x27, 0x3C, 0x8C, 0x00, 0x00,  //   Logical Maximum (35899)
	0x35, 0x00,        //   Physical Minimum (0)
	0x47, 0x3C, 0x8C, 0x00, 0x00,  //   Physical Maximum (35899)
	0x65, 0x14,        //   Unit (System: English Rotation, Length: Centimeter)
	0x75, 0x20,        //   Report Size (32)
	0x95, 0x01,        //   Report Count (1)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x09, 0x39,        //   Usage (Hat switch)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x05, 0x09,        //   Usage Page (Button)
	0x15, 0x00,        //   Logical Minimum (0)
	0x25, 0x01,        //   Logical Maximum (1)
	0x55, 0x00,        //   Unit Exponent (0)
	0x65, 0x00,        //   Unit (None)
	0x19, 0x01,        //   Usage Minimum (0x01)
	0x29, 0x20,        //   Usage Maximum (0x20)
	0x75, 0x01,        //   Report Size (1)
	0x95, 0x20,        //   Report Count (32)
	0x81, 0x02,        //   Input (Data,Var,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0x75, 0x60,        //   Report Size (96)
	0x95, 0x01,        //   Report Count (1)
	0x81, 0x01,        //   Input (Const,Array,Abs,No Wrap,Linear,Preferred State,No Null Position)
	0xC0,              // End Collection
};

CPerfil::CPerfil(void* pVHID)
{
	this->pVHID = pVHID;
	hMutexPrograma = CreateMutex(NULL, FALSE, NULL);
	hMutexCalibrado = CreateMutex(NULL, FALSE, NULL);
	hMutexEstado = CreateMutex(NULL, FALSE, NULL);
	RtlZeroMemory(&perfil, sizeof(PROGRAMADO));
	RtlZeroMemory(&calibrado, sizeof(CALIBRADO));
	RtlZeroMemory(&estado, sizeof(ESTADO));
	modoRaw = FALSE;
	modoCalibrado = FALSE;
}

CPerfil::~CPerfil()
{
	WaitForSingleObject(hMutexCalibrado, INFINITE);
	RtlZeroMemory(&calibrado, sizeof(CALIBRADO));
	CloseHandle(hMutexCalibrado);
	LimpiarPerfil();
	CloseHandle(hMutexPrograma);
	CloseHandle(hMutexEstado);
}

void CPerfil::LimpiarPerfil()
{
	WaitForSingleObject(hMutexPrograma, INFINITE);
	{
		if (CColaEventos::Get() != nullptr)
		{
			CColaEventos::Get()->Vaciar();
		}
		if (CProcesarSalida::Get() != nullptr)
		{
			CProcesarSalida::Get()->LimpiarEventos();
		}

		if (perfil.Acciones != nullptr)
		{
			while (!perfil.Acciones->empty())
			{
				delete perfil.Acciones->front();
				perfil.Acciones->pop_front();
			}
			delete perfil.Acciones;
			perfil.Acciones = nullptr;
		}
		RtlZeroMemory(&perfil, sizeof(PROGRAMADO));

		LockEstado();
		{
			RtlZeroMemory(&estado, sizeof(ESTADO));
		}
		UnlockEstado();
		perfilNuevoIn = true;
		perfilNuevoOut = true;
	}
	ReleaseMutex(hMutexPrograma);
}

bool  CPerfil::HF_IoEscribirMapa(BYTE* SystemBuffer, DWORD tam)
{
	if (!resetComandos)
	{
		return false;
	}
	resetComandos = false;
	if (tam == (17 + 1 + sizeof(perfil.MapaEjes) + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas)))
	{
		BYTE txt[17];
		RtlCopyMemory(txt, SystemBuffer, 17);
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(txt, 17);
		RtlZeroMemory(txt, 17);
		txt[0] = 2;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(txt, 2);
		txt[0] = 3;
		if (CX52Salida::Get() != nullptr) CX52Salida::Get()->Set_Texto(txt, 2);
		WaitForSingleObject(hMutexPrograma, INFINITE);
		{
			RtlCopyMemory(&perfil.TickRaton, SystemBuffer + 17 , 1);
			RtlCopyMemory(perfil.MapaBotones, SystemBuffer + 17 + 1, sizeof(perfil.MapaBotones));
			RtlCopyMemory(perfil.MapaSetas, SystemBuffer + 17 + 1 + sizeof(perfil.MapaBotones), sizeof(perfil.MapaSetas));
			RtlCopyMemory(perfil.MapaEjes, SystemBuffer + 17 + 1 + sizeof(perfil.MapaBotones) + sizeof(perfil.MapaSetas), sizeof(perfil.MapaEjes));		
		}
		ReleaseMutex(hMutexPrograma);
	}
	else
	{
		LimpiarPerfil();
		return false;
	}

	return RangoHIDvJoy();
}

bool  CPerfil::HF_IoEscribirComandos(BYTE* SystemBuffer, DWORD InputBufferLength)
{
	BYTE* bufIn;
	size_t tamPrevisto = 0;

	LimpiarPerfil();

	if (InputBufferLength != 0)
	{
		//Comprobar OK
		bufIn = SystemBuffer;
		while (tamPrevisto < InputBufferLength)
		{
			tamPrevisto += (static_cast<size_t>(*bufIn) * 2) + 1;
			bufIn += (static_cast<size_t>(*bufIn) * 2) + 1;
		}
		if (tamPrevisto != InputBufferLength)
		{
			return false;
		}
	}

	resetComandos = true;

	if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetHoraActivada(true);
	if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetFechaActivada(true);

	if (InputBufferLength == 0)
	{
		return true;
	}

	WaitForSingleObject(hMutexPrograma, INFINITE);
	{
		perfil.Acciones = new std::deque<CPaqueteEvento*>();
		bufIn = SystemBuffer;
		tamPrevisto = 0;
		while (tamPrevisto < InputBufferLength)
		{
			CPaqueteEvento* colaComandos = new CPaqueteEvento();
			UCHAR tamAccion = *bufIn;
			UCHAR i = 0;

			bufIn++;
			tamPrevisto++;

			for (i = 0; i < tamAccion; i++)
			{
				PEV_COMANDO mem = new EV_COMANDO;
				RtlZeroMemory(mem, sizeof(EV_COMANDO));
				mem->Tipo = *bufIn;
				mem->Dato = *(bufIn + 1);
				if ((((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdHora) || (((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdHora24))
				{
					if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetHoraActivada(false);
				}
				else if (((PEV_COMANDO)bufIn)->Tipo == TipoComando::MfdFecha)
				{
					if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->SetFechaActivada(false);
				}
				bufIn += 2;
				tamPrevisto += 2;
				colaComandos->AñadirComando(mem);
			}
			perfil.Acciones->push_back(colaComandos);
		}
	}
	ReleaseMutex(hMutexPrograma);

	return true;
}

void CPerfil::EscribirAntivibracion(BYTE* datos)
{
	WaitForSingleObject(hMutexCalibrado, INFINITE);
	RtlCopyMemory(calibrado.Jitter, datos, sizeof(calibrado.Jitter));
	ReleaseMutex(hMutexCalibrado);
}

void CPerfil::EscribirCalibrado(BYTE* datos)
{
	WaitForSingleObject(hMutexCalibrado, INFINITE);
	RtlCopyMemory(calibrado.Limites, datos, sizeof(calibrado.Limites));
	ReleaseMutex(hMutexCalibrado);
}

bool CPerfil::RangoHIDvJoy()
{
	USHORT rangos[3][8];
	bool usados[3][8];

	ZeroMemory(rangos, sizeof(rangos));
	ZeroMemory(usados, sizeof(usados));

	for (char i = 0; i < 4; i++)
	{
		for (char j = 0; j < 8; j++)
		{
			for (char m = 0; m < 3; m++)
			{
				for (char p = 0; p < 2; p++)
				{
					if ((perfil.MapaEjes[i][p][m][j].TipoEje & 7) && !usados[perfil.MapaEjes[i][p][m][j].JoySalida][perfil.MapaEjes[i][p][m][j].Eje])
					{
						usados[perfil.MapaEjes[i][p][m][j].JoySalida][perfil.MapaEjes[i][p][m][j].Eje] = true;
						rangos[perfil.MapaEjes[i][p][m][j].JoySalida][perfil.MapaEjes[i][p][m][j].Eje] = calibrado.Limites[i][j].Rango;
					}
				}
			}
		}
	}

	BYTE* buf = new BYTE[142];
	DWORD tam = 142;
	for (char i = 1; i < 4; i++)
	{
		wchar_t dev[] = L"SYSTEM\\CurrentControlSet\\Services\\vjoy\\Parameters\\Device01";
		_itow_s(i, &dev[57], 2, 10);
		memcpy(buf, vJoyReport, 142);
		buf[11] = i;
		*((USHORT*)&buf[25]) = rangos[i - 1][0] == 0 ? 32767 : rangos[i - 1][0];
		*((USHORT*)&buf[32]) = rangos[i - 1][1] == 0 ? 32767 : rangos[i - 1][1];
		*((USHORT*)&buf[39]) = rangos[i - 1][2] == 0 ? 32767 : rangos[i - 1][2];
		*((USHORT*)&buf[46]) = rangos[i - 1][3] == 0 ? 32767 : rangos[i - 1][3];
		*((USHORT*)&buf[53]) = rangos[i - 1][4] == 0 ? 32767 : rangos[i - 1][4];
		*((USHORT*)&buf[60]) = rangos[i - 1][5] == 0 ? 32767 : rangos[i - 1][5];
		*((USHORT*)&buf[67]) = rangos[i - 1][6] == 0 ? 32767 : rangos[i - 1][6];
		*((USHORT*)&buf[74]) = rangos[i - 1][7] == 0 ? 32767 : rangos[i - 1][7];

		if (ERROR_SUCCESS != RegSetKeyValueW(HKEY_LOCAL_MACHINE, dev, L"HidReportDesctiptorSize", REG_DWORD, &tam, 4))
		{
			delete[] buf;
			return false;
		}
		if (ERROR_SUCCESS != RegSetKeyValue(HKEY_LOCAL_MACHINE, dev, L"HidReportDesctiptor", REG_BINARY, buf, 142))
		{
			delete[] buf;
			return false;
		}
	}
	delete[] buf;

	ActualizarvJoy();
	return true;
}

void CPerfil::ActualizarvJoy()
{
	CONST GUID guidHid = { 0x745a17a0,0x74d3,0x11d0,{0xb6,0xfe,0x00,0xa0,0xc9,0x0f,0x57,0xda} };
	DWORD idx = 0;
	HDEVINFO di = SetupDiGetClassDevs(&guidHid, NULL, NULL, DIGCF_PRESENT);
	if (di != INVALID_HANDLE_VALUE)
	{
		SP_DEVINFO_DATA dev;
		ZeroMemory(&dev, sizeof(SP_DEVINFO_DATA));
		dev.cbSize = sizeof(SP_DEVINFO_DATA);
		DWORD idx = 0;
		while (SetupDiEnumDeviceInfo(di, idx, &dev))
		{
			idx++;
			DWORD tam = 0;
			SetupDiGetDeviceRegistryPropertyA(di, &dev, SPDRP_HARDWAREID, NULL, NULL, 0, &tam);
			if (tam != 0)
			{
				BYTE* desc = new BYTE[tam];
				if (SetupDiGetDeviceRegistryPropertyA(di, &dev, SPDRP_HARDWAREID, NULL, desc, tam, NULL))
				{
					if ((_stricmp((char*)desc, "HID\\VID_1234&PID_BEAD&REV_0219&Col01") == 0) ||
						(_stricmp((char*)desc, "HID\\VID_1234&PID_BEAD&REV_0219&Col02") == 0) ||
						(_stricmp((char*)desc, "HID\\VID_1234&PID_BEAD&REV_0219&Col03") == 0))
					{
						SetupDiCallClassInstaller(DIF_REMOVE, di, &dev);
					}
				}
				delete[]desc; desc = NULL;
			}
		}
		idx = 0;
		while (SetupDiEnumDeviceInfo(di, idx, &dev))
		{
			idx++;
			DWORD tam = 0;
			SetupDiGetDeviceRegistryPropertyW(di, &dev, SPDRP_HARDWAREID, NULL, NULL, 0, &tam);
			if (tam != 0)
			{
				BYTE* desc = new BYTE[tam];
				if (SetupDiGetDeviceRegistryPropertyW(di, &dev, SPDRP_HARDWAREID, NULL, desc, tam, NULL))
				{
					if (_wcsicmp((wchar_t*)desc, L"root\\VID_1234&PID_BEAD&REV_0219") == 0)
					{
						SP_PROPCHANGE_PARAMS params
						{
							params.ClassInstallHeader.cbSize = sizeof(SP_CLASSINSTALL_HEADER),
							params.ClassInstallHeader.InstallFunction = DIF_PROPERTYCHANGE,
							params.Scope = DICS_FLAG_GLOBAL,
							params.StateChange = DICS_DISABLE,
							params.HwProfile = 0
						};          
						if (SetupDiSetClassInstallParamsW(di, &dev, &params.ClassInstallHeader, sizeof(params)))
						{
							SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, di, &dev);
							params.StateChange = DICS_ENABLE;
							Sleep(1000);
							if (SetupDiSetClassInstallParamsW(di, &dev, &params.ClassInstallHeader, sizeof(params)))
							{
								SetupDiCallClassInstaller(DIF_PROPERTYCHANGE, di, &dev);
							}
						}
						DWORD err = GetLastError();
						delete[]desc; desc = NULL;
						break;
					}
				}
				delete[]desc; desc = NULL;
			}
		}
		SetupDiDestroyDeviceInfoList(di);
	}
}