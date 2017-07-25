#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#define _PRIVATE_
#include "CalibradoHID.h"
#undef _PRIVATE_

NTSTATUS EscribirCalibrado(_In_ WDFDEVICE device, _In_ WDFREQUEST request)
{
	PROGRAMADO_CONTEXT*	devExt = &GetDeviceContext(device)->Programacion;
	NTSTATUS            status = STATUS_SUCCESS;
	UCHAR				i;
	PCALIBRADO			bufCal = NULL;

	status = WdfRequestRetrieveInputBuffer(request, (sizeof(CALIBRADO) * 4), &bufCal, NULL);
	WdfRequestSetInformation(request, sizeof(CALIBRADO) * 4);
	if (!NT_SUCCESS(status))
		return status;

	for (i = 0; i < 4; i++)
	{
		WdfSpinLockAcquire(devExt->slCalibrado);
		{
			devExt->limites[i].c = bufCal[i].c;
			devExt->limites[i].i = bufCal[i].i;
			devExt->limites[i].d = bufCal[i].d;
			devExt->limites[i].n = bufCal[i].n;
			devExt->limites[i].cal = bufCal[i].cal;
			devExt->jitter[i].Margen = bufCal[i].Margen;
			devExt->jitter[i].Resistencia = bufCal[i].Resistencia;
			devExt->jitter[i].antiv = bufCal[i].antiv;
		}
		WdfSpinLockRelease(devExt->slCalibrado);
	}

	return status;
}

VOID Calibrar(WDFDEVICE device, PHID_INPUT_DATA datosHID)
{
	UCHAR		idx;
	LONG		pollEje[4];
	STLIMITES	limites[4];
	STJITTER	jitter[4];

	WdfSpinLockAcquire(GetDeviceContext(device)->Programacion.slCalibrado);
	RtlCopyMemory(limites, GetDeviceContext(device)->Programacion.limites, sizeof(STLIMITES) * 4);
	RtlCopyMemory(jitter, GetDeviceContext(device)->Programacion.jitter, sizeof(STJITTER) * 4);
	WdfSpinLockRelease(GetDeviceContext(device)->Programacion.slCalibrado);

	// Filtrado de ejes
	for (idx = 0; idx < 4; idx++)
	{
		pollEje[idx] = *((UINT16*)&datosHID->Ejes[idx * 2]);

		if (jitter[idx].antiv) {
			// Antivibraciones
			if ((pollEje[idx] < (jitter[idx].PosElegida - jitter[idx].Margen)) || (pollEje[idx] > (jitter[idx].PosElegida + jitter[idx].Margen)))
			{
				jitter[idx].PosRepetida = 0;
				jitter[idx].PosElegida = (UINT16)pollEje[idx];
			}
			else
			{
				if (jitter[idx].PosRepetida < jitter[idx].Resistencia) {
					jitter[idx].PosRepetida++;
					pollEje[idx] = jitter[idx].PosElegida;
				}
				else
				{
					jitter[idx].PosRepetida = 0;
					jitter[idx].PosElegida = (UINT16)pollEje[idx];
				}
			}
		}

		if (limites[idx].cal)
		{
			// Calibrado
			UINT16 ancho1, ancho2;
			ancho1 = (limites[idx].c - limites[idx].n) - limites[idx].i;
			ancho2 = limites[idx].d - (limites[idx].c + limites[idx].n);
			if (((pollEje[idx] >= (limites[idx].c - limites[idx].n)) && (pollEje[idx] <= (limites[idx].c + limites[idx].n))))
			{
				//Zona nula
				pollEje[idx] = 1024;
				continue;
			}
			else
			{
				if (pollEje[idx] < limites[idx].i)
					pollEje[idx] = limites[idx].i;
				if (pollEje[idx] > limites[idx].d)
					pollEje[idx] = limites[idx].d;

				if (pollEje[idx] < limites[idx].c)
				{
					pollEje[idx] = ((limites[idx].c - limites[idx].n) - pollEje[idx]);
					if (ancho1 > ancho2)
					{
						pollEje[idx] = ((pollEje[idx] * ancho2) / ancho1);
					}
					pollEje[idx] = 0 - pollEje[idx];
				}
				else
				{
					pollEje[idx] -= (limites[idx].c + limites[idx].n);
					if (ancho2 > ancho1)
						pollEje[idx] = ((pollEje[idx] * ancho1) / ancho2);
				}
			}
			if (ancho2 > ancho1)
				pollEje[idx] = ((pollEje[idx] + ancho1) * (2048)) / (2 * ancho1);
			else
				pollEje[idx] = ((pollEje[idx] + ancho2) * (2048)) / (2 * ancho2);
		}
	}

	for (idx = 0; idx < 4; idx++)
		*((UINT16*)&datosHID->Ejes[idx * 2]) = (UINT16)pollEje[idx];
}