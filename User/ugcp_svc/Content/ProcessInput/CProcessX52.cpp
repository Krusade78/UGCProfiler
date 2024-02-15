#include "../framework.h"
#include "CProcessX52.h"
#include "GenerateEvents/CGenerateEvents.h"
#include "GenerateEvents/ProcessUSBs_Buttons-Hats.h"
#include "GenerateEvents/ProcessUSBs_Axes.h"
#include "../X52/MFDMenu.h"

CProcesarX52::CProcesarX52(CProfile* perfil)
{
	RtlZeroMemory(&USBaHID, sizeof(USB_HIDX52_CONTEXT));
	USBaHID.Perfil = perfil;
}

void CProcesarX52::PreProcesarModos(UCHAR estadoModos)
{
	//Obligar x52 asegurar que primero se suelta el modo y luego se pulsa (se usa para porder usarlos como botones normales)
	if (USBaHID.UltimoEstadoModos != estadoModos)
	{
		VHID_INPUT_DATA viejohidData;
		RtlCopyMemory(&viejohidData, &USBaHID.UltimoEstadoJ.DeltaHidData, sizeof(VHID_INPUT_DATA));
		viejohidData.Buttons[1] &= 0xf8;

		USBaHID.UltimoEstadoModos = estadoModos;
		Procesar_Joy(&viejohidData);
	}
}

void CProcesarX52::Procesar_Joy(PVHID_INPUT_DATA p_hidData)
{
	USB_HIDX52_CONTEXT* devExt = &USBaHID;
	VHID_INPUT_DATA viejohidData;

	if (CMFDMenu::Get() != nullptr)
	{
		if (!CMFDMenu::Get()->X52Joy())
		{
			return;
		}
	}

	RtlCopyMemory(&viejohidData, &devExt->UltimoEstadoJ.DeltaHidData, sizeof(VHID_INPUT_DATA));
	RtlCopyMemory(&devExt->UltimoEstadoJ.DeltaHidData, p_hidData, sizeof(VHID_INPUT_DATA));

	if (!devExt->Perfil->GetRawMode() && !devExt->Perfil->GetCalibrationMode())
	{
		UCHAR idx;

		//	Botones

		for (idx = 0; idx < 2; idx++)
		{
			UCHAR cambios = p_hidData->Buttons[idx] ^ viejohidData.Buttons[idx];
			if (cambios != 0)
			{
				UCHAR exp;
				for (exp = 0; exp < 8; exp++)
				{
					if ((cambios >> exp) & 1)
					{ // Si ha cambiado
						if ((p_hidData->Buttons[idx] >> exp) & 1)
							CBotonesSetas::PulsarBoton(devExt->Perfil, TipoJoy::X52_Joy, (idx * 8) + exp);
						else
							CBotonesSetas::SoltarBoton(devExt->Perfil, TipoJoy::X52_Joy, (idx * 8) + exp);
					}
				}
			}
		}

		// Setas

		for (idx = 0; idx < 2; idx++)
		{
			if (p_hidData->Hats[idx] != viejohidData.Hats[idx])
			{
				if (viejohidData.Hats[idx] != 0)
					CBotonesSetas::SoltarSeta(devExt->Perfil, TipoJoy::X52_Joy, (idx * 8) + viejohidData.Hats[idx] - 1);
				if (p_hidData->Hats[idx] != 0)
					CBotonesSetas::PulsarSeta(devExt->Perfil, TipoJoy::X52_Joy, (idx * 8) + p_hidData->Hats[idx] - 1);
			}
		}

		// Ejes
		if (p_hidData->Axes[0] != viejohidData.Axes[0])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Joy, 0, p_hidData->Axes[0]);
		if (p_hidData->Axes[1] != viejohidData.Axes[1])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Joy, 1, p_hidData->Axes[1]);
		if (p_hidData->Axes[3] != viejohidData.Axes[3])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Joy, 3, p_hidData->Axes[3]);

		// Sensibilidad y mapeado
		CEjes::SensibilidadYMapeado(devExt->Perfil, TipoJoy::X52_Joy, &viejohidData, p_hidData);
	}
	else
	{
		CGenerarEventos::DirectX(static_cast<UCHAR>(TipoJoy::Raw_Joy), 0xff, p_hidData);
	}
}

void CProcesarX52::Procesar_Ace(PVHID_INPUT_DATA p_hidData)
{
	USB_HIDX52_CONTEXT* devExt = &USBaHID;
	VHID_INPUT_DATA viejohidData;

	//aqui no hace falta proteger con WaitLock
	RtlCopyMemory(&viejohidData, &devExt->UltimoEstadoA.DeltaHidData, sizeof(VHID_INPUT_DATA));
	RtlCopyMemory(&devExt->UltimoEstadoA.DeltaHidData, p_hidData, sizeof(VHID_INPUT_DATA));

	if (!devExt->Perfil->GetCalibrationMode())
	{
		//	Botones menu
		UCHAR cambios = p_hidData->Buttons[0] ^ viejohidData.Buttons[0];
		if (cambios != 0)
		{
			UCHAR exp;
			for (exp = 2; exp < 5; exp++)
			{
				if ((cambios >> exp) & 1)
				{ // Si ha cambiado
					if ((p_hidData->Buttons[0] >> exp) & 1)
					{
						if (CMFDMenu::Get() != nullptr) CMFDMenu::Get()->MenuPressButton(exp - 2);
					}
					else
					{
						if (CMFDMenu::Get() != nullptr) CMFDMenu::Get()->MenuReleaseButton(exp - 2);
					}
				}
			}
		}
	}
	if (!devExt->Perfil->GetRawMode() && !devExt->Perfil->GetCalibrationMode())
	{
		UCHAR idx;

		//	Botones

		for (idx = 0; idx < 2; idx++)
		{
			UCHAR cambios = p_hidData->Buttons[idx] ^ viejohidData.Buttons[idx];
			if (cambios != 0)
			{
				UCHAR exp;
				for (exp = 0; exp < 8; exp++)
				{
					if ((cambios >> exp) & 1)
					{ // Si ha cambiado
						UCHAR bt = (idx * 8) + exp;
						if ((p_hidData->Buttons[idx] >> exp) & 1)
						{
							if (CMFDMenu::Get() != nullptr)
							{
								if (CMFDMenu::Get()->IsActivated() && ((bt == 2) || (bt == 3) || (bt == 4)))
									continue;
							}
							CBotonesSetas::PulsarBoton(devExt->Perfil, TipoJoy::X52_Ace, bt);
						}
						else
						{
							if (CMFDMenu::Get() != nullptr)
							{
								if (CMFDMenu::Get()->IsActivated() && ((bt == 2) || (bt == 3) || (bt == 4)))
									continue;
							}
							CBotonesSetas::SoltarBoton(devExt->Perfil, TipoJoy::X52_Ace, bt);
						}
					}
				}
			}
		}

		// Setas

		for (idx = 0; idx < 2; idx++)
		{
			if (p_hidData->Hats[idx] != viejohidData.Hats[idx])
			{
				if (viejohidData.Hats[idx] != 0)
					CBotonesSetas::SoltarSeta(devExt->Perfil, TipoJoy::X52_Ace, (idx * 8) + viejohidData.Hats[idx] - 1);
				if (p_hidData->Hats[idx] != 0)
					CBotonesSetas::PulsarSeta(devExt->Perfil, TipoJoy::X52_Ace, (idx * 8) + p_hidData->Hats[idx] - 1);
			}
		}

		// Ejes

		for (idx = 0; idx < 7; idx++)
		{
			if (p_hidData->Axes[idx] != viejohidData.Axes[idx])
				CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Ace, idx, p_hidData->Axes[idx]);
		}

		// Sensibilidad y mapeado
		CEjes::SensibilidadYMapeado(devExt->Perfil, TipoJoy::X52_Ace, &viejohidData, p_hidData);
	}
	else
	{
		CGenerarEventos::DirectX(static_cast<UCHAR>(TipoJoy::RawX52_Ace), 0xff, p_hidData);
	}
}
