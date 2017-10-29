#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "AccionesProcesar.h"
#define _PRIVATE_
#include "AccionesGenerar.h"
#undef _PRIVATE_


VOID AccionarRaton(WDFDEVICE device, PUCHAR accion)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	PCOLA			eventos = ColaAllocate();
	if (eventos != NULL)
	{
		PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
		if (evt != NULL)
		{
			RtlCopyMemory(evt, accion, sizeof(UCHAR) * 2);
			if (!ColaPush(eventos, evt))
			{
				ExFreePoolWithTag(evt, (ULONG)'vepV');
				ColaBorrar(eventos); eventos = NULL;
			}
			else
			{
				WdfSpinLockAcquire(devExt->SpinLockAcciones);
				{
					if (!ColaPush(&devExt->ColaAcciones, eventos))
					{
						ColaBorrar(eventos); eventos = NULL;
					}
				}
				WdfSpinLockRelease(devExt->SpinLockAcciones);
			}
		}
	}
}

VOID AccionarComando(WDFDEVICE device, UINT16 accionId, UCHAR boton)
{
	HID_CONTEXT*		devExt = &GetDeviceContext(device)->HID;
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	
	if(accionId != 0)
	{
		PCOLA eventos = ColaAllocate();
		if(eventos != NULL)
		{
			BOOLEAN ok = TRUE;
			WdfSpinLockAcquire(idevExt->slComandos);
			{
				if (idevExt->nComandos == 0 || idevExt->Comandos == NULL)
				{
					ColaBorrar(eventos); eventos = NULL;
					ok = FALSE;
				}
				else
				{
					UCHAR idx;
					for (idx = 0; idx < idevExt->Comandos[accionId - 1].tam; idx++)
					{
						PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'vepV');
						if (evt != NULL)
						{
							RtlCopyMemory(evt, &(idevExt->Comandos[accionId - 1].datos[idx]), sizeof(UCHAR) * 2);
							if (*((PUCHAR)evt) == TipoComando_Hold || *((PUCHAR)evt) == TipoComando_Repeat)
								((PUCHAR)evt)[1] = boton;

							if (!ColaPush(eventos, evt))
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
			}
			WdfSpinLockRelease(idevExt->slComandos);
			if(ok)
			{
				WdfSpinLockAcquire(devExt->SpinLockAcciones);
				{
					if (!ColaPush(&devExt->ColaAcciones, eventos))
					{
						ColaBorrar(eventos); eventos = NULL;
						ok = FALSE;
					}
				}
				WdfSpinLockRelease(devExt->SpinLockAcciones);
			}
		}
	}
}

VOID AccionarDirectX(WDFDEVICE device, PHID_INPUT_DATA inputData)
{
	HID_CONTEXT*	devExt = &GetDeviceContext(device)->HID;
	PCOLA			eventos = ColaAllocate();

	if (eventos != NULL)
	{
		PVOID evt = ExAllocatePoolWithTag(NonPagedPool, sizeof(HID_INPUT_DATA) + 1, (ULONG)'vepV');
		if (evt != NULL)
		{
			*(PUCHAR)evt = TipoComando_DxPosicion;
			RtlCopyMemory((PUCHAR)evt + 1, inputData, sizeof(HID_INPUT_DATA) + 1);
			if (!ColaPush(eventos, evt))
			{
				ExFreePoolWithTag(evt, (ULONG)'vepV');
				ColaBorrar(eventos); eventos = NULL;
			}
			else
			{
				WdfSpinLockAcquire(devExt->SpinLockAcciones);
				{
					if (!ColaPush(&devExt->ColaAcciones, eventos))
					{
						ColaBorrar(eventos); eventos = NULL;
					}
				}
				WdfSpinLockRelease(devExt->SpinLockAcciones);
			}
		}
	}
}
