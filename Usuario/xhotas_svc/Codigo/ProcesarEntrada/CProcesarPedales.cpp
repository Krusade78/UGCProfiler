#include "../framework.h"
#include "CProcesarPedales.h"
#include "GenerarEventos/CGenerarEventos.h"
#include "GenerarEventos/ProcesarUSBs_Ejes.h"

CProcesarPedales::CProcesarPedales(CPerfil* perfil)
{
	RtlZeroMemory(&USBaHID, sizeof(USB_HIDPEDALES_CONTEXT));
	USBaHID.Perfil = perfil;
}

void CProcesarPedales::Procesar(PVHID_INPUT_DATA p_hidData)
{
	USB_HIDPEDALES_CONTEXT* devExt = &USBaHID;
	VHID_INPUT_DATA viejohidData;


	RtlCopyMemory(&viejohidData, &devExt->UltimoEstado.DeltaHidData, sizeof(VHID_INPUT_DATA));

	RtlCopyMemory(&devExt->UltimoEstado.DeltaHidData, p_hidData, sizeof(VHID_INPUT_DATA));

	if (!devExt->Perfil->GetModoRaw())
	{
		// Ejes

		if (p_hidData->Ejes[3] != viejohidData.Ejes[3])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::Pedales, 3, p_hidData->Ejes[3]);
		if (p_hidData->Ejes[6] != viejohidData.Ejes[6])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::Pedales, 6, p_hidData->Ejes[6]);
		if (p_hidData->Ejes[7] != viejohidData.Ejes[7])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::Pedales, 7, p_hidData->Ejes[7]);

		// Sensibilidad y mapeado
		CEjes::SensibilidadYMapeado(devExt->Perfil, TipoJoy::Pedales, &viejohidData, p_hidData);
	}
	else
	{
		CGenerarEventos::DirectX(static_cast<UCHAR>(TipoJoy::RawPedales), 0xff, p_hidData);
	}
}
