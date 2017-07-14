/*++
Copyright (c) 2017 Alfredo Costalago

Module Name:

teclado_read.c

Abstract:

Pasar datos del teclado al HID.

--*/
#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#include "acciones.h"
#include "Raton_read.h"

VOID EvtRatonListo(_In_ WDFQUEUE Queue, _In_ WDFCONTEXT Context)
{
	UNREFERENCED_PARAMETER(Context);

	ProcesarAcciones(WdfIoQueueGetDevice(Queue), FALSE);
}

BOOLEAN ProcesarEventoRaton(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT	devExt = GetDeviceContext(device)->HID;
	WDFREQUEST request;
	BOOLEAN soltar;
	NTSTATUS status;
	PVOID buffer;

	soltar = ((tipo & 32) == 32) ? TRUE : FALSE;

	switch (tipo & 0x1f)
	{
	case 1:
		if (!soltar)
			devExt.stRaton[0] |= 1;
		else
			devExt.stRaton[0] &= 254;
		break;
	case 2:
		if (!soltar)
			devExt.stRaton[0] |= 2;
		else
			devExt.stRaton[0] &= 253;
		break;
	case 3:
		if (!soltar)
			devExt.stRaton[0] |= 4;
		else
			devExt.stRaton[0] &= 251;
		break;
	case 4: //Eje -x
		if (!soltar)
			devExt.stRaton[1] = -dato;
		else
			devExt.stRaton[1] = 0;
		break;
	case 5: //Eje x
		if (!soltar)
			devExt.stRaton[1] = dato;
		else
			devExt.stRaton[1] = 0;
		break;
	case 6: //Eje -y
		if (!soltar)
			devExt.stRaton[2] = -dato;
		else
			devExt.stRaton[2] = 0;
		break;
	case 7: //Eje y
		if (!soltar)
			devExt.stRaton[2] = dato;
		else
			devExt.stRaton[2] = 0;
		break;
	case 8: // Wheel up
		if (!soltar)
			devExt.stRaton[3] = 127;
		else
			devExt.stRaton[3] = 0;
		break;
	case 9: // Wheel down
		if (!soltar)
			devExt.stRaton[3] = (UCHAR)-127;
		else
			devExt.stRaton[3] = 0;
		break;
	}

	status = WdfIoQueueRetrieveNextRequest(GetDeviceContext(device)->ColaRaton, &request);
	if (((status == STATUS_NO_MORE_ENTRIES) || NT_SUCCESS(status)))
	{
		if (request == NULL)
			return FALSE;
	}
	if (!NT_SUCCESS(status))
		return FALSE;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt.stRaton) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 2;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt.stRaton, sizeof(devExt.stRaton));
		WdfRequestSetInformation(request, sizeof(devExt.stRaton) + 1);
	}
	WdfRequestComplete(request, status);

	//ActualizarTimerRaton(devExt);
	return TRUE;
}




