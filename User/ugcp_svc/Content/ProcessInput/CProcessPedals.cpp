#include "../framework.h"
#include "CProcessPedals.h"
#include "GenerateEvents/CGenerateEvents.h"
#include "GenerateEvents/ProcessUSBs_Axes.h"

CProcesarPedales::CProcesarPedales(CProfile* perfil)
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

	if (!devExt->Perfil->GetRawMode() && !devExt->Perfil->GetCalibrationMode())
	{
		// Ejes

		if (p_hidData->Axes[3] != viejohidData.Axes[3])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::Pedales, 3, p_hidData->Axes[3]);
		if (p_hidData->Axes[0] != viejohidData.Axes[0])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::Pedales, 0, p_hidData->Axes[0]);
		if (p_hidData->Axes[1] != viejohidData.Axes[1])
			CEjes::MoverEje(devExt->Perfil, TipoJoy::Pedales, 1, p_hidData->Axes[1]);

		// Sensibilidad y mapeado
		CEjes::SensibilidadYMapeado(devExt->Perfil, TipoJoy::Pedales, &viejohidData, p_hidData);
	}
	else
	{
		CGenerarEventos::DirectX(static_cast<UCHAR>(TipoJoy::RawPedales), 0xff, p_hidData);
	}
}
