#define _CONTEXT_
#include "PnPPedales.h"
#include "LeerUSBPedales.h"
#include "LeerUSBX52.h"
#include "EscribirUSBX52.h"
//#include "ProcesarUSBs.h" Se incluye en LeerUSBX52
#include "ProcesarHID.h"
#include "ProcesarUSBs_Calibrado.h"
#include "Perfil.h"
#include "MenuMFD.h"
#undef _CONTEXT_

typedef struct _DEVICE_CONTEXT
{
	WDFUSBDEVICE		UsbDevice;
	WDFDEVICE			ControlDevice;
	PEDALES_PNP_CONTEXT	PedalesPnP;
	PEDALESREAD_CONTEXT Pedales;
	X52READ_CONTEXT		EntradaX52;
	X52WRITE_CONTEXT	SalidaX52;
	USB_HIDX52_CONTEXT	USBaHID;
	HID_CONTEXT			HID;
	CALIBRADO_CONTEXT	Calibrado;
	PROGRAMADO_CONTEXT	Perfil;
	MENU_MFD_CONTEXT	MenuMFD;
} DEVICE_CONTEXT, *PDEVICE_CONTEXT;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_CONTEXT, GetDeviceContext);
