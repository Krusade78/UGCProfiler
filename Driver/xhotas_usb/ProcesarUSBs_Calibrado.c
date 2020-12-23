/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

ProcesarUSBs_Calibrado.c

Abstract:

Calibrar entrada USB.

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#define _PUBLIC_
#define _PRIVATE_
#include "ProcesarUSBs_Calibrado.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, ConfigurarCalibrado)
#pragma alloc_text (PAGE, ConfigurarAntivibracion)
#pragma alloc_text (PAGE, Calibrar)
#endif

VOID Calibrar(WDFDEVICE device, PHID_INPUT_DATA datosHID)
{
	PAGED_CODE();

	UCHAR		idx;
	LONG		pollEje[4];
	STLIMITES	limites[4];
	STJITTER	jitter[4];

	WdfWaitLockAcquire(GetDeviceContext(device)->Calibrado.WaitLockCalibrado, NULL);
	{
		RtlCopyMemory(limites, GetDeviceContext(device)->Calibrado.Limites, sizeof(STLIMITES) * 4);
		RtlCopyMemory(jitter, GetDeviceContext(device)->Calibrado.Jitter, sizeof(STJITTER) * 4);
	}
	WdfWaitLockRelease(GetDeviceContext(device)->Calibrado.WaitLockCalibrado);

	// Filtrado de ejes
	for (idx = 0; idx < 4; idx++)
	{
		pollEje[idx] = (LONG)datosHID->Ejes[idx];

		if (jitter[idx].Antiv) {
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

		if (limites[idx].Cal)
		{
			// Calibrado
			UINT16 ancho1, ancho2;
			ancho1 = (limites[idx].Cen - limites[idx].Nulo) - limites[idx].Izq;
			ancho2 = limites[idx].Der - (limites[idx].Cen + limites[idx].Nulo);
			if (((pollEje[idx] >= (limites[idx].Cen - limites[idx].Nulo)) && (pollEje[idx] <= (limites[idx].Cen + limites[idx].Nulo))))
			{
				//Zona nula
				pollEje[idx] = 1024;
				continue;
			}
			else
			{
				if (pollEje[idx] < limites[idx].Izq)
					pollEje[idx] = limites[idx].Izq;
				if (pollEje[idx] > limites[idx].Der)
					pollEje[idx] = limites[idx].Der;

				if (pollEje[idx] < limites[idx].Cen)
				{
					pollEje[idx] = ((limites[idx].Cen - limites[idx].Nulo) - pollEje[idx]);
					if (ancho1 > ancho2)
					{
						pollEje[idx] = ((pollEje[idx] * ancho2) / ancho1);
					}
					pollEje[idx] = 0 - pollEje[idx];
				}
				else
				{
					pollEje[idx] -= (limites[idx].Cen + limites[idx].Nulo);
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
	{
		datosHID->Ejes[idx] = (UINT16)pollEje[idx];
	}
}

NTSTATUS ConfigurarCalibrado(_In_ WDFDEVICE device, _In_ WDFREQUEST request)
{
	PAGED_CODE();

	CALIBRADO_CONTEXT*	devExt = &GetDeviceContext(device)->Calibrado;
	NTSTATUS            status = STATUS_SUCCESS;
	UCHAR				i;
	PSTLIMITES			bufCal = NULL;

	status = WdfRequestRetrieveInputBuffer(request, (sizeof(STLIMITES) * 4), &bufCal, NULL);
	WdfRequestSetInformation(request, sizeof(STLIMITES) * 4);
	if (!NT_SUCCESS(status))
		return status;

	WdfWaitLockAcquire(devExt->WaitLockCalibrado, NULL);
	{
		for (i = 0; i < 4; i++)
		{
				devExt->Limites[i].Cal = bufCal[i].Cal;
				devExt->Limites[i].Nulo = bufCal[i].Nulo;
				devExt->Limites[i].Izq = bufCal[i].Izq;
				devExt->Limites[i].Cen = bufCal[i].Cen;
				devExt->Limites[i].Der = bufCal[i].Der;
		}
	}
	WdfWaitLockRelease(devExt->WaitLockCalibrado);


	return status;
}

NTSTATUS ConfigurarAntivibracion(_In_ WDFDEVICE device, _In_ WDFREQUEST request)
{
	PAGED_CODE();

	CALIBRADO_CONTEXT* devExt = &GetDeviceContext(device)->Calibrado;
	NTSTATUS            status = STATUS_SUCCESS;
	UCHAR				i;
	PSTJITTER			bufCal = NULL;

	status = WdfRequestRetrieveInputBuffer(request, (sizeof(STJITTER) * 4), &bufCal, NULL);
	WdfRequestSetInformation(request, sizeof(STJITTER) * 4);
	if (!NT_SUCCESS(status))
		return status;

	WdfWaitLockAcquire(devExt->WaitLockCalibrado, NULL);
	{
		for (i = 0; i < 4; i++)
		{
			devExt->Jitter[i].Margen = bufCal[i].Margen;
			devExt->Jitter[i].Resistencia = bufCal[i].Resistencia;
			devExt->Jitter[i].Antiv = bufCal[i].Antiv;
		}
	}
	WdfWaitLockRelease(devExt->WaitLockCalibrado);

	return status;
}