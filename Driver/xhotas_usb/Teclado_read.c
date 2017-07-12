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
//#include "Context.h"
#include "Teclado_read.h"

VOID EvtTecladoListo(_In_ WDFQUEUE Queue, _In_ WDFCONTEXT Context)
{
	UNREFERENCED_PARAMETER(Queue);
	UNREFERENCED_PARAMETER(Context);
}


