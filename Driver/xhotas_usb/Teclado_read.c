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
#include "Teclado_read.h"

VOID EvtTecladoListo(_In_ WDFQUEUE Queue, _In_ WDFCONTEXT Context)
{
	UNREFERENCED_PARAMETER(Context);

	ProcesarAcciones(WdfIoQueueGetDevice(Queue));
}

BOOLEAN ProcesarEventoTeclado_HOTAS(WDFDEVICE device, UCHAR tipo, UCHAR dato)
{
	HID_CONTEXT	devExt = GetDeviceContext(device)->HID;
	WDFREQUEST	request;
	BOOLEAN		soltar;
	NTSTATUS	status;
	PVOID		buffer;

	soltar = ((tipo >> 5) == 1) ? TRUE : FALSE;
	tipo &= 0x1f;

	if (!soltar)
		devExt.stTeclado[dato / 8] |= 1 << (dato % 8);
	else
		devExt.stTeclado[dato / 8] &= ~(1 << (dato % 8));

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt.stTeclado) + 1, &buffer, NULL);
	if (NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 3;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt.stTeclado, sizeof(devExt.stTeclado));
		WdfRequestSetInformation(request, sizeof(devExt.stTeclado) + 1);
	}
	WdfRequestComplete(request, status);
}