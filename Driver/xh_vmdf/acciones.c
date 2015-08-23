#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"

#define _ACCIONES_
#include "acciones.h"
#undef _ACCIONES_

VOID AccionarHOTAS
(
	PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA accion)
{
	PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(HID_INPUT_DATA), (ULONG)'vepV');
	if(evt != NULL)
	{
		RtlCopyMemory(evt, accion, sizeof(HID_INPUT_DATA));
		WdfSpinLockAcquire(devExt->SpinLockAcciones);

			if(!ColaPush(&devExt->ColaAccionesHOTAS, evt))
				ExFreePoolWithTag(evt, (ULONG)'vepV');
			else
				WdfDpcEnqueue(devExt->DpcRequest);

		WdfSpinLockRelease(devExt->SpinLockAcciones);
	}
}

VOID AccionarRaton
(
	PDEVICE_EXTENSION devExt,
	PUCHAR accion)
{
	PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
	if(evt != NULL)
	{
		RtlCopyMemory(evt, accion, sizeof(UCHAR) * 2);

		WdfSpinLockAcquire(devExt->SpinLockAcciones);

			if(!ColaPush(&devExt->ColaAccionesRaton, evt))
				ExFreePoolWithTag(evt, (ULONG)'vepV');
			else
				WdfDpcEnqueue(devExt->DpcRequest);

		WdfSpinLockRelease(devExt->SpinLockAcciones);
	}
}

VOID AccionarComando
(
	PDEVICE_EXTENSION devExt,
	UINT16 accionId,
	UCHAR boton
	)
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	
	if(accionId != 0)
	{
		PCOLA eventos = ColaCrear();
		if(eventos != NULL)
		{
			BOOLEAN ok = TRUE;
			WdfSpinLockAcquire(idevExt->slComandos);
				if(idevExt->nComandos == 0 || idevExt->Comandos == NULL)
				{
					ColaBorrar(eventos); eventos = NULL;
					ok = FALSE;
				}
				else
				{
					UCHAR idx;
					for(idx = 0; idx < idevExt->Comandos[accionId - 1].tam; idx++)
					{
						PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
						if(evt != NULL)
						{
							RtlCopyMemory(evt, &(idevExt->Comandos[accionId - 1].datos[idx]), sizeof(UCHAR) * 2);
							if(*((PUCHAR)evt) == 11 || *((PUCHAR)evt) == 12)
								((PUCHAR)evt)[1] = boton;

							if(!ColaPush(eventos,evt))
							{
								ExFreePoolWithTag(evt, (ULONG)'vepV');
								ColaBorrar(eventos); eventos = NULL;
								ok = FALSE;
								break;
							}
						}
						else
						{
							ColaBorrar(eventos); eventos = NULL;
							ok = FALSE;
							break;
						}
					}
				}
			WdfSpinLockRelease(idevExt->slComandos);
			if(ok)
			{
				WdfSpinLockAcquire(devExt->SpinLockAcciones);
					if(!ColaPush(&devExt->ColaAccionesComando, eventos))
					{
						ColaBorrar(eventos); eventos = NULL;
					}
					else
						WdfDpcEnqueue(devExt->DpcRequest);
				WdfSpinLockRelease(devExt->SpinLockAcciones);
			}
		}
	}
}


