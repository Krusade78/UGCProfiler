#pragma once
//#include <ntddk.h>
#include <wdf.h>

typedef struct _HID_INPUT_DATA
{
	UCHAR   Ejes[16];
	UCHAR	Setas[4];
	UCHAR	Botones[4];
	UCHAR   MiniStick;
} HID_INPUT_DATA, *PHID_INPUT_DATA;

typedef struct _PEDALES_EXTENSION
{
	PVOID			PnPNotifyHandle;
	WDFIOTARGET		IoTarget;
	WDFWAITLOCK		WaitLockIoTarget;
	WDFSPINLOCK		SpinLockPosicion;
	BOOLEAN			Activado;
	UCHAR			PedalSel;
	INT16			Posicion;
} PEDALES_EXTENSION;

typedef struct _X52_EXTENSION
{
	WDFCOLLECTION	ListaRequest;
	WDFSPINLOCK		SpinLockRequest;
	WDFSPINLOCK		SpinLockPosicion;
	HID_INPUT_DATA	Posicion;
} X52_EXTENSION;

typedef struct _X52USB_EXTENSION
{
	USHORT			fecha;
} X52USB_EXTENSION;

typedef struct _DEVICE_EXTENSION
{
	WDFDEVICE			Self;
	X52_EXTENSION		X52;
	PEDALES_EXTENSION	Pedales;
	X52USB_EXTENSION	SalidaX52;
	WDFDEVICE			UsrIOControlDevice;
	//WDFUSBDEVICE    UsbDevice;
} DEVICE_EXTENSION, *PDEVICE_EXTENSION;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_EXTENSION, GetDeviceExtension);