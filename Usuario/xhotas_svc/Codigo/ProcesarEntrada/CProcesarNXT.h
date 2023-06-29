#pragma once
#include "../Perfil/CPerfil.h"

class CProcesarNXT
{
public:
	CProcesarNXT(CPerfil* perfil);

	void Procesar(PVHID_INPUT_DATA p_hidData);
	UCHAR ConvertirSeta(UCHAR pos);
private:
	typedef struct
	{
		CPerfil* Perfil;
		struct
		{
			VHID_INPUT_DATA	DeltaHidData;
		} UltimoEstado;
	} USB_HIDNXT_CONTEXT;

	USB_HIDNXT_CONTEXT USBaHID;
};


