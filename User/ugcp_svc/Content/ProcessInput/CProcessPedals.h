#pragma once
#include "../Profile/CProfile.h"
#include "../ProcessOutput/CVirtualHID.h"

class CProcesarPedales
{
public:
	CProcesarPedales(CProfile* perfil);
	~CProcesarPedales() {}

	void Procesar(PVHID_INPUT_DATA p_hidData);

private:
	typedef struct
	{
		CProfile* Perfil;
		struct
		{
			VHID_INPUT_DATA	DeltaHidData;
		} UltimoEstado;
	} USB_HIDPEDALES_CONTEXT;

	USB_HIDPEDALES_CONTEXT USBaHID;
};

