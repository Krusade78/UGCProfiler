#pragma once
#include "../Perfil/CPerfil.h"
#include "../ProcesarSalida/CVirtualHID.h"

//HID_INPUT_DATA modificado
typedef struct _HID_INPUT_DATA
{
	INT16  Ejes[7];
	UCHAR	Setas[4];
	UCHAR	Botones[4];
	UCHAR   MiniStick[2];
} HID_INPUT_DATA, * PHID_INPUT_DATA;

class CProcesarX52
{
public:
	CProcesarX52(CPerfil* perfil);
	~CProcesarX52() {}

	void PreProcesarModos(UCHAR estadoModos);
	void Procesar_Joy(PVHID_INPUT_DATA p_hidData);
	void Procesar_Ace(PVHID_INPUT_DATA p_hidData);
private:
	typedef struct
	{
		CPerfil* Perfil;
		UCHAR	UltimoEstadoModos;
		struct
		{
			VHID_INPUT_DATA	DeltaHidData;
		} UltimoEstadoJ;
		struct
		{
			VHID_INPUT_DATA	DeltaHidData;
		} UltimoEstadoA;
	} USB_HIDX52_CONTEXT;

	USB_HIDX52_CONTEXT USBaHID;
};