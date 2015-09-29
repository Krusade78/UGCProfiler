#pragma once
//#include <ntddk.h>
#include <wdf.h>

typedef struct _PEDALES_EXTENSION
{
	WDFIOTARGET		IoTarget;
	WDFWAITLOCK		WaitLockIoTarget;
	WDFSPINLOCK		SpinLockPosicion;
	BOOLEAN			Activado;
	UCHAR			PedalSel;
	INT16			Posicion;
} PEDALES_EXTENSION;

typedef struct _DEVICE_EXTENSION
{
	WDFDEVICE			Self;
	PVOID				PnPNotifyHandle;
	PEDALES_EXTENSION	Pedales;
	//USHORT			fecha;
	//WDFDEVICE		ControlDevice;
	//WDFUSBDEVICE    UsbDevice;
} DEVICE_EXTENSION, *PDEVICE_EXTENSION;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(DEVICE_EXTENSION, GetDeviceExtension);

//typedef struct _CONTROL_EXTENSION
//{
//	PDEVICE_EXTENSION devExt;
//} CONTROL_EXTENSION, *PCONTROL_EXTENSION;
//WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(CONTROL_EXTENSION, GetControlExtension);