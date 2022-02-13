#include "../framework.h"
#include "CProcesarNXT.h"
#include "GenerarEventos/CGenerarEventos.h"
#include "GenerarEventos/ProcesarUSBs_Botones-Setas.h"
#include "GenerarEventos/ProcesarUSBs_Ejes.h"
#include "../X52/MenuMFD.h"

CProcesarNXT::CProcesarNXT(CPerfil* perfil)
{
	RtlZeroMemory(&USBaHID, sizeof(USB_HIDNXT_CONTEXT));
	USBaHID.Perfil = perfil;
}
UCHAR CProcesarNXT::ConvertirSeta(UCHAR pos)
{
	switch (pos)
	{
		case 1:
			return 5;
		case 2:
			return 1;
		case 4:
			return 7;
		case 8:
			return 3;
		default:
			return 0;
	}
}

void CProcesarNXT::Procesar(PVHID_INPUT_DATA p_hidData)
{
	USB_HIDNXT_CONTEXT* devExt = &USBaHID;
	VHID_INPUT_DATA viejohidData;

	if (CMenuMFD::Get() != nullptr)
	{
		if (CMenuMFD::Get()->X52Joy())
		{
			return;
		}
	}

	RtlCopyMemory(&viejohidData, &devExt->UltimoEstado.DeltaHidData, sizeof(VHID_INPUT_DATA));
	RtlCopyMemory(&devExt->UltimoEstado.DeltaHidData, p_hidData, sizeof(VHID_INPUT_DATA));

	bool cambio = false;
	for (UCHAR i = 0; i < sizeof(VHID_INPUT_DATA); i++)
	{
		if (((UCHAR*)&viejohidData)[i] != ((UCHAR*)p_hidData)[i])
		{
			cambio = true;
			break;
		}
	}
	if (!cambio)
	{
		return;
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
						if ((p_hidData->Botones[idx] >> exp) & 1)
							CBotonesSetas::PulsarBoton(devExt->Perfil, TipoJoy::NXT, (idx * 8) + exp);
						else
							CBotonesSetas::SoltarBoton(devExt->Perfil, TipoJoy::NXT, (idx * 8) + exp);
					}
				}
			}
		}

		// Setas

		for (idx = 0; idx < 4; idx++)
		{
			if (p_hidData->Setas[idx] != viejohidData.Setas[idx])
			{
				if (viejohidData.Setas[idx] != 0)
					CBotonesSetas::SoltarSeta(devExt->Perfil, TipoJoy::NXT, (idx * 8) + viejohidData.Setas[idx] - 1);
				if (p_hidData->Setas[idx] != 0)
					CBotonesSetas::PulsarSeta(devExt->Perfil, TipoJoy::NXT, (idx * 8) + p_hidData->Setas[idx] - 1);
			}
		}

		// Ejes
		if (p_hidData->Ejes[0] != viejohidData.Ejes[0])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::NXT, 0, p_hidData->Ejes[0]);
		if (p_hidData->Ejes[1] != viejohidData.Ejes[1])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::NXT, 1, p_hidData->Ejes[1]);
		if (p_hidData->Ejes[2] != viejohidData.Ejes[2])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::NXT, 2, p_hidData->Ejes[2]);
		if (p_hidData->Ejes[3] != viejohidData.Ejes[3])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::NXT, 3, p_hidData->Ejes[3]);
		if (p_hidData->Ejes[6] != viejohidData.Ejes[6])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::NXT, 6, p_hidData->Ejes[6]);
		if (p_hidData->Ejes[7] != viejohidData.Ejes[7])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::NXT, 7, p_hidData->Ejes[7]);

		// Sensibilidad y mapeado
		CEjes::SensibilidadYMapeado(devExt->Perfil, TipoJoy::NXT, &viejohidData, p_hidData);
	}
	else
	{
		CGenerarEventos::DirectX(static_cast<UCHAR>(TipoJoy::Raw_Joy), 0xff, p_hidData);
	}
}