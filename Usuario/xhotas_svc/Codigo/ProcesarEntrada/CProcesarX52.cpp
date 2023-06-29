#include "../framework.h"
#include "CProcesarX52.h"
#include "GenerarEventos/CGenerarEventos.h"
#include "GenerarEventos/ProcesarUSBs_Botones-Setas.h"
#include "GenerarEventos/ProcesarUSBs_Ejes.h"
#include "../X52/MenuMFD.h"

CProcesarX52::CProcesarX52(CPerfil* perfil)
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
		viejohidData.Botones[1] &= 0xf8;

		USBaHID.UltimoEstadoModos = estadoModos;
		Procesar_Joy(&viejohidData);
	}
}

void CProcesarX52::Procesar_Joy(PVHID_INPUT_DATA p_hidData)
{
	USB_HIDX52_CONTEXT* devExt = &USBaHID;
	VHID_INPUT_DATA viejohidData;

	if (CMenuMFD::Get() != nullptr)
	{
		if (!CMenuMFD::Get()->X52Joy())
		{
			return;
		}
	}

	RtlCopyMemory(&viejohidData, &devExt->UltimoEstadoJ.DeltaHidData, sizeof(VHID_INPUT_DATA));
	RtlCopyMemory(&devExt->UltimoEstadoJ.DeltaHidData, p_hidData, sizeof(VHID_INPUT_DATA));

	if (!devExt->Perfil->GetModoRaw() && !devExt->Perfil->GetModoCalibrado())
	{
		UCHAR idx;

		//	Botones

		for (idx = 0; idx < 2; idx++)
		{
			UCHAR cambios = p_hidData->Botones[idx] ^ viejohidData.Botones[idx];
			if (cambios != 0)
			{
				UCHAR exp;
				for (exp = 0; exp < 8; exp++)
				{
					if ((cambios >> exp) & 1)
					{ // Si ha cambiado
						if ((p_hidData->Botones[idx] >> exp) & 1)
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
			if (p_hidData->Setas[idx] != viejohidData.Setas[idx])
			{
				if (viejohidData.Setas[idx] != 0)
					CBotonesSetas::SoltarSeta(devExt->Perfil, TipoJoy::X52_Joy, (idx * 8) + viejohidData.Setas[idx] - 1);
				if (p_hidData->Setas[idx] != 0)
					CBotonesSetas::PulsarSeta(devExt->Perfil, TipoJoy::X52_Joy, (idx * 8) + p_hidData->Setas[idx] - 1);
			}
		}

		// Ejes
		if (p_hidData->Ejes[0] != viejohidData.Ejes[0])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Joy, 0, p_hidData->Ejes[0]);
		if (p_hidData->Ejes[1] != viejohidData.Ejes[1])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Joy, 1, p_hidData->Ejes[1]);
		if (p_hidData->Ejes[3] != viejohidData.Ejes[3])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Joy, 3, p_hidData->Ejes[3]);

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

	if (!devExt->Perfil->GetModoCalibrado())
	{
		//	Botones menu
		UCHAR cambios = p_hidData->Botones[0] ^ viejohidData.Botones[0];
		if (cambios != 0)
		{
			UCHAR exp;
			for (exp = 2; exp < 5; exp++)
			{
				if ((cambios >> exp) & 1)
				{ // Si ha cambiado
					if ((p_hidData->Botones[0] >> exp) & 1)
					{
						if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->MenuPulsarBoton(exp - 2);
					}
					else
					{
						if (CMenuMFD::Get() != nullptr) CMenuMFD::Get()->MenuSoltarBoton(exp - 2);
					}
				}
			}
		}
	}
	if (!devExt->Perfil->GetModoRaw() && !devExt->Perfil->GetModoCalibrado())
	{
		UCHAR idx;

		//	Botones

		for (idx = 0; idx < 2; idx++)
		{
			UCHAR cambios = p_hidData->Botones[idx] ^ viejohidData.Botones[idx];
			if (cambios != 0)
			{
				UCHAR exp;
				for (exp = 0; exp < 8; exp++)
				{
					if ((cambios >> exp) & 1)
					{ // Si ha cambiado
						UCHAR bt = (idx * 8) + exp;
						if ((p_hidData->Botones[idx] >> exp) & 1)
						{
							if (CMenuMFD::Get() != nullptr)
							{
								if (CMenuMFD::Get()->EstaActivado() && ((bt == 2) || (bt == 3) || (bt == 4)))
									continue;
							}
							CBotonesSetas::PulsarBoton(devExt->Perfil, TipoJoy::X52_Ace, bt);
						}
						else
						{
							if (CMenuMFD::Get() != nullptr)
							{
								if (CMenuMFD::Get()->EstaActivado() && ((bt == 2) || (bt == 3) || (bt == 4)))
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
			if (p_hidData->Setas[idx] != viejohidData.Setas[idx])
			{
				if (viejohidData.Setas[idx] != 0)
					CBotonesSetas::SoltarSeta(devExt->Perfil, TipoJoy::X52_Ace, (idx * 8) + viejohidData.Setas[idx] - 1);
				if (p_hidData->Setas[idx] != 0)
					CBotonesSetas::PulsarSeta(devExt->Perfil, TipoJoy::X52_Ace, (idx * 8) + p_hidData->Setas[idx] - 1);
			}
		}

		// Ejes

		for (idx = 0; idx < 7; idx++)
		{
			if (p_hidData->Ejes[idx] != viejohidData.Ejes[idx])
				CEjes::MoverEje(devExt->Perfil, TipoJoy::X52_Ace, idx, p_hidData->Ejes[idx]);
		}

		// Sensibilidad y mapeado
		CEjes::SensibilidadYMapeado(devExt->Perfil, TipoJoy::X52_Ace, &viejohidData, p_hidData);
	}
	else
	{
		CGenerarEventos::DirectX(static_cast<UCHAR>(TipoJoy::RawX52_Ace), 0xff, p_hidData);
	}
}
