/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

ProcesarUSBs_Botones-Setas.c

Abstract:

Convertir datos originales del USB a eventos.

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "EventosGenerar.h"
#include "MenuMFD.h"
#define _PRIVATE_
#include "ProcesarUSBs_Botones-setas.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, PulsarBoton)
#pragma alloc_text (PAGE, SoltarBoton)
#pragma alloc_text (PAGE, PulsarSeta)
#pragma alloc_text (PAGE, SoltarSeta)
#endif

VOID PulsarBoton(WDFDEVICE device, UCHAR idx)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT*	pdevExt = &GetDeviceContext(device)->Perfil;
	UINT16 accionId;

	if ((idx == 11) || (idx == 12) || (idx == 13))
	{
		MenuPulsarBoton(device, idx - 11);
	}
	if ((GetDeviceContext(device)->MenuMFD.Activado && ((idx == 11) || (idx == 12) || (idx == 13))) ||
		(GetDeviceContext(device)->USBaHID.ModoRaw))
	{
		return;
	}
	else
	{
		WdfWaitLockAcquire(pdevExt->WaitLockMapas, NULL);
		{
			UCHAR pinkie;
			UCHAR modos;
			WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
			{
				pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
				modos = GetDeviceContext(device)->HID.Estado.Modos;
			}
			WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);

			accionId = pdevExt->MapaBotones[pinkie][modos][idx].Indices[pdevExt->MapaBotones[pinkie][modos][idx].PosActual];
			if (pdevExt->MapaBotones[pinkie][modos][idx].TamIndices > 0)
			{
				pdevExt->MapaBotones[pinkie][modos][idx].PosActual++;
			}
			if (pdevExt->MapaBotones[pinkie][modos][idx].PosActual == pdevExt->MapaBotones[pinkie][modos][idx].TamIndices)
			{
				pdevExt->MapaBotones[pinkie][modos][idx].PosActual = 0;
			}
		}
		WdfWaitLockRelease(pdevExt->WaitLockMapas);

		GenerarEventoComando(device, accionId, idx, Origen_Boton, NULL);
	}
}

VOID SoltarBoton(WDFDEVICE device, UCHAR idx)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT* pdevExt = &GetDeviceContext(device)->Perfil;
	UINT16 accionId;

	if ((idx == 11) || (idx == 12) || (idx == 13))
	{
		MenuSoltarBoton(device, idx - 11);
	}
	if ((GetDeviceContext(device)->MenuMFD.Activado && ((idx == 11) || (idx == 12) || (idx == 13))) ||
		(GetDeviceContext(device)->USBaHID.ModoRaw))
	{
		return;
	}
	else
	{
		WdfWaitLockAcquire(pdevExt->WaitLockMapas, NULL);
		{
			UCHAR pinkie;
			UCHAR modos;
			WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
			{
				pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
				modos = GetDeviceContext(device)->HID.Estado.Modos;
			}
			WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);

			if (pdevExt->MapaBotones[pinkie][modos][idx].TamIndices == 0)
			{
				accionId = pdevExt->MapaBotones[pinkie][modos][idx].Indices[1];
				WdfWaitLockRelease(pdevExt->WaitLockMapas);

				GenerarEventoComando(device, accionId, idx, Origen_Boton, NULL);
			}
			else
			{
				WdfWaitLockRelease(pdevExt->WaitLockMapas);
			}
		}
	}
}

VOID PulsarSeta(WDFDEVICE device, UCHAR idx)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT* pdevExt = &GetDeviceContext(device)->Perfil;
	UINT16 accionId;

	WdfWaitLockAcquire(pdevExt->WaitLockMapas, NULL);
	{
		UCHAR pinkie;
		UCHAR modos;
		WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
		{
			pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
			modos = GetDeviceContext(device)->HID.Estado.Modos;
		}
		WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);

		accionId = pdevExt->MapaSetas[pinkie][modos][idx].Indices[pdevExt->MapaSetas[pinkie][modos][idx].PosActual];
		if (pdevExt->MapaSetas[pinkie][modos][idx].TamIndices > 0)
		{
			pdevExt->MapaSetas[pinkie][modos][idx].PosActual++;
		}
		if (pdevExt->MapaSetas[pinkie][modos][idx].PosActual == pdevExt->MapaSetas[pinkie][modos][idx].TamIndices)
		{
			pdevExt->MapaSetas[pinkie][modos][idx].PosActual = 0;
		}

	}
	WdfWaitLockRelease(pdevExt->WaitLockMapas);

	GenerarEventoComando(device, accionId, idx, Origen_Seta, NULL);
}

VOID SoltarSeta(WDFDEVICE device, UCHAR idx)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT*	pdevExt = &GetDeviceContext(device)->Perfil;
	UINT16 accionId;

	WdfWaitLockAcquire(pdevExt->WaitLockMapas, NULL);
	{
		UCHAR pinkie;
		UCHAR modos;
		WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
		{
			pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
			modos = GetDeviceContext(device)->HID.Estado.Modos;
		}
		WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);

		if (pdevExt->MapaSetas[pinkie][modos][idx].TamIndices == 0)
		{
			accionId = pdevExt->MapaSetas[pinkie][modos][idx].Indices[1];
			WdfWaitLockRelease(pdevExt->WaitLockMapas);

			GenerarEventoComando(device, accionId, idx, Origen_Seta, NULL);
		}
		else
		{
			WdfWaitLockRelease(pdevExt->WaitLockMapas);
		}
	}
}