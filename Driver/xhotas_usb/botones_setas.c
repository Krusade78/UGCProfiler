#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "AccionesGenerar.h"
#define _PRIVATE_
#include "botones_setas.h"
#undef _PRIVATE_

VOID GenerarPulsarBoton(WDFDEVICE device, UCHAR idx)
{
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT*		hidCtx = &GetDeviceContext(device)->HID;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);
	{
		if ((idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado & 0xf) > 0)
		{
			accionId = idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado >> 4];
			idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado += 16;
			if ((idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado >> 4) == (idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado & 0xf))
				idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado &= 0xf;
		}
		else
		{
			accionId = idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[0];
		}
	}
	WdfSpinLockRelease(idevExt->slMapas);

	AccionarComando(device, accionId, idx);
}

VOID GenerarSoltarBoton(WDFDEVICE device, UCHAR idx)
{
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT*		hidCtx = &GetDeviceContext(device)->HID;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);
	{
		if ((idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado & 0xf) == 0)
		{
			accionId = idevExt->MapaBotones[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[1];
			WdfSpinLockRelease(idevExt->slMapas);

			AccionarComando(device, accionId, idx);
		}
		else
		{
			WdfSpinLockRelease(idevExt->slMapas);
		}
	}
}

VOID GenerarPulsarSeta(WDFDEVICE device, UCHAR idx)
{
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT	*		hidCtx = &GetDeviceContext(device)->HID;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);
	{
		if ((idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado & 0xf) > 0)
		{
			accionId = idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado >> 4];
			idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado += 16;
			if ((idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado >> 4) == (idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado & 0xf))
				idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado &= 0xf;
		}
		else
		{
			accionId = idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[0];
		}
	}
	WdfSpinLockRelease(idevExt->slMapas);

	AccionarComando(device, accionId, idx + 101);
}

VOID GenerarSoltarSeta(WDFDEVICE device, UCHAR idx)
{
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT	*		hidCtx = &GetDeviceContext(device)->HID;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);
	{
		if ((idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Estado & 0xf) == 0)
		{

			accionId = idevExt->MapaSetas[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[1];
			WdfSpinLockRelease(idevExt->slMapas);

			AccionarComando(device, accionId, idx + 101);
		}
		else
		{
			WdfSpinLockRelease(idevExt->slMapas);
		}
	}
}