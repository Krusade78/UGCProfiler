#pragma once
#include "../Perfil/CPerfil.h"
#include "../ProcesarSalida/CVirtualHID.h"

class CProcesarPedales
{
public:
	CProcesarPedales(CPerfil* perfil);
	~CProcesarPedales() {}

	void Procesar(PVHID_INPUT_DATA p_hidData);

private:
	typedef struct
	{
		CPerfil* Perfil;
		struct
		{
			VHID_INPUT_DATA	DeltaHidData;
		} UltimoEstado;
	} USB_HIDPEDALES_CONTEXT;

	USB_HIDPEDALES_CONTEXT USBaHID;
};

