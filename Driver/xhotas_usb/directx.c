#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "AccionesGenerar.h"
#define _PRIVATE_
#include "directx.h"
#undef _PRIVATE_

VOID GenerarDirectX(WDFDEVICE device, _In_ PHID_INPUT_DATA inputData)
{
	AccionarDirectX(device, inputData);
}