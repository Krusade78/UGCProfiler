#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "acciones.h"
#include "botones.h"

VOID GenerarPulsarBoton
(	PDEVICE_EXTENSION devExt,
	UCHAR idx)
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);
		if((idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado & 0xf) > 0)
		{
			accionId = idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado >> 4];
			idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado += 16;
			if((idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado >> 4) == (idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado & 0xf))
				idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado &= 0xf;
		}
		else
		{
			accionId = idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[0];
		}
	WdfSpinLockRelease(idevExt->slMapas);

	AccionarComando(devExt, accionId, idx);
}

VOID GenerarSoltarBoton
(	PDEVICE_EXTENSION devExt,
	UCHAR idx)
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);

	if((idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado & 0xf) == 0)
	{
		accionId = idevExt->MapaBotones[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[1];
		WdfSpinLockRelease(idevExt->slMapas);

		AccionarComando(devExt, accionId, idx);
	}
	else
	{
		WdfSpinLockRelease(idevExt->slMapas);
	}
}

VOID GenerarPulsarSeta
(	PDEVICE_EXTENSION devExt,
	UCHAR idx)
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);

	if((idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado & 0xf) > 0)
	{
		accionId = idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado >> 4];
		idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado += 16;
		if((idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado >> 4) == (idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado & 0xf))
			idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado &= 0xf;
	}
	else
	{
		accionId = idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[0];
	}

	WdfSpinLockRelease(idevExt->slMapas);

	AccionarComando(devExt, accionId, idx + 101);
}

VOID GenerarSoltarSeta
(	PDEVICE_EXTENSION devExt,
	UCHAR idx)
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	UINT16 accionId;

	WdfSpinLockAcquire(idevExt->slMapas);

	if((idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Estado & 0xf) == 0)
	{

		accionId=idevExt->MapaSetas[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[1];
		WdfSpinLockRelease(idevExt->slMapas);

		AccionarComando(devExt, accionId, idx + 101);
	}
	else
	{
		WdfSpinLockRelease(idevExt->slMapas);
	}
}