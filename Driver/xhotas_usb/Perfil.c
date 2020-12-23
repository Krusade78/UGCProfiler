/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

Eventos.c

Abstract:

Cargar/Descargar datos del perfil

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "EventosProcesar.h"
#define _PRIVATE_
#include "Perfil.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, LimpiarPerfil)
#pragma alloc_text(PAGE, HF_IoEscribirMapa)
#endif

VOID LimpiarPerfil(WDFDEVICE device)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT* pdevExt = &GetDeviceContext(device)->Perfil;
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;

	WdfWaitLockAcquire(devExt->WaitLockEventos, NULL);
	{
		WdfWaitLockAcquire(devExt->Estado.WaitLockEstado, NULL);
		{
			RtlZeroMemory(devExt->Estado.Teclado, sizeof(devExt->Estado.Teclado));
			RtlZeroMemory(devExt->Estado.Raton, sizeof(devExt->Estado.Raton));
			RtlZeroMemory(&devExt->Estado.DirectX, sizeof(HID_INPUT_DATA));
			devExt->Estado.Pinkie = FALSE;
			devExt->Estado.Modos = 0;
		}
		WdfWaitLockRelease(devExt->Estado.WaitLockEstado);

		WdfWaitLockAcquire(pdevExt->WaitLockMapas, NULL);
		{
			RtlZeroMemory(pdevExt->MapaBotones, sizeof(pdevExt->MapaBotones));
			RtlZeroMemory(pdevExt->MapaSetas, sizeof(pdevExt->MapaSetas));
			RtlZeroMemory(pdevExt->MapaEjes, sizeof(pdevExt->MapaEjes));
			RtlZeroMemory(pdevExt->MapaEjesPeque, sizeof(pdevExt->MapaEjesPeque));
			RtlZeroMemory(pdevExt->MapaEjesMini, sizeof(pdevExt->MapaEjesMini));
		}
		WdfWaitLockRelease(pdevExt->WaitLockMapas);

		WdfWaitLockAcquire(GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado, NULL);
		{
			RtlZeroMemory(&GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental, sizeof(GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental));
			RtlZeroMemory(&GetDeviceContext(device)->USBaHID.UltimoEstado.Banda, sizeof(GetDeviceContext(device)->USBaHID.UltimoEstado.Banda));
			RtlZeroMemory(&GetDeviceContext(device)->USBaHID.UltimoEstado.DeltaHidData, sizeof(HID_INPUT_DATA));
		}
		WdfWaitLockRelease(GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado);

		WdfWaitLockAcquire(pdevExt->WaitLockAcciones, NULL);
		{
			while (WdfCollectionGetCount(pdevExt->Acciones) > 0)
			{
				WdfObjectDelete(WdfCollectionGetFirstItem(pdevExt->Acciones));
				WdfCollectionRemoveItem(pdevExt->Acciones, 0);
			}
		}
		WdfWaitLockRelease(pdevExt->WaitLockAcciones);
	}
	WdfWaitLockRelease(devExt->WaitLockEventos);
}

NTSTATUS HF_IoEscribirMapa(_In_ WDFDEVICE device, _In_ WDFREQUEST Request)
{
	PAGED_CODE();

    PROGRAMADO_CONTEXT*		devExt = &GetDeviceContext(device)->Perfil;
    NTSTATUS                status = STATUS_SUCCESS;
	PUCHAR					SystemBuffer;
	size_t					tam;

	status =  WdfRequestRetrieveInputBuffer(Request, 1 + (sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini) + sizeof(devExt->MapaBotones)+sizeof(devExt->MapaSetas)), &SystemBuffer, &tam);
	if(!NT_SUCCESS(status))
		return status;

	if( tam == (1 + sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini) + sizeof(devExt->MapaBotones)+sizeof(devExt->MapaSetas)) )
	{
		WdfWaitLockAcquire(devExt->WaitLockMapas, NULL);
		{
			RtlCopyMemory(&devExt->TickRaton,	SystemBuffer, 1);
			RtlCopyMemory(devExt->MapaBotones,	SystemBuffer + 1, sizeof(devExt->MapaBotones));
			RtlCopyMemory(devExt->MapaSetas,	SystemBuffer + 1 + sizeof(devExt->MapaBotones), sizeof(devExt->MapaSetas));
			RtlCopyMemory(devExt->MapaEjes,		SystemBuffer + 1 + sizeof(devExt->MapaBotones) + sizeof(devExt->MapaSetas), sizeof(devExt->MapaEjes));
			RtlCopyMemory(devExt->MapaEjesPeque,SystemBuffer + 1 + sizeof(devExt->MapaBotones) + sizeof(devExt->MapaSetas) + sizeof(devExt->MapaEjes), sizeof(devExt->MapaEjesPeque));
			RtlCopyMemory(devExt->MapaEjesMini,	SystemBuffer + 1 + sizeof(devExt->MapaBotones) + sizeof(devExt->MapaSetas) + sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque), sizeof(devExt->MapaEjesMini));
		}
		WdfWaitLockRelease(devExt->WaitLockMapas);

		WdfRequestSetInformation(Request, tam);
	}
	else
	{
		LimpiarPerfil(device);
		status = STATUS_BUFFER_TOO_SMALL;
		WdfRequestSetInformation(Request, 0);
	}

	return status;
}

NTSTATUS HF_IoEscribirComandos(_In_ WDFDEVICE device, _In_ WDFREQUEST Request, _In_ BOOLEAN vacio)
{
	PROGRAMADO_CONTEXT*		devExt = &GetDeviceContext(device)->Perfil;
    NTSTATUS                status = STATUS_SUCCESS;
	PUCHAR					bufIn;
	PVOID					SystemBuffer = NULL;
	size_t					InputBufferLength = 0;
	size_t					tamPrevisto = 0;

	if (!vacio)
	{
		status =  WdfRequestRetrieveInputBuffer(Request, 0, &SystemBuffer, &InputBufferLength);
		if(!NT_SUCCESS(status))
			return status;


		//Comprobar OK
		bufIn = (PUCHAR)SystemBuffer;
		while (tamPrevisto < InputBufferLength)
		{
			tamPrevisto += (*bufIn * sizeof(EV_COMANDO)) + 1;
			bufIn += (*bufIn * sizeof(EV_COMANDO)) + 1;
		}
		if (tamPrevisto != InputBufferLength)
		{
			WdfRequestSetInformation(Request, tamPrevisto);
			return STATUS_INVALID_BUFFER_SIZE;
		}
	}

	LimpiarEventos(device);
	LimpiarPerfil(device);
	LimpiarEventos(device);

	GetDeviceContext(device)->MenuMFD.HoraActivada = TRUE;
	GetDeviceContext(device)->MenuMFD.FechaActivada = TRUE;

	WdfWaitLockAcquire(devExt->WaitLockAcciones, NULL);
	{
		bufIn = (PUCHAR)SystemBuffer;
		tamPrevisto = 0;
		while (tamPrevisto < InputBufferLength)
		{
			WDFCOLLECTION colaComandos;
			WDF_OBJECT_ATTRIBUTES attributes;
			UCHAR tamAccion = *bufIn;
			UCHAR i = 0;

			bufIn++;
			tamPrevisto++;

			WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = devExt->Acciones;
			status = WdfCollectionCreate(&attributes, &colaComandos);
			if (!NT_SUCCESS(status))
			{
				break;
			}
			for (i = 0; i < tamAccion; i++)
			{
				WDFMEMORY mem;
				attributes.ParentObject = colaComandos;
				status = WdfMemoryCreate(&attributes, PagedPool, (ULONG)'evtP', sizeof(EV_COMANDO), &mem, NULL);
				if (!NT_SUCCESS(status))
				{
					break;
				}
				else
				{
					status = WdfMemoryCopyFromBuffer(mem, 0, bufIn, sizeof(EV_COMANDO));
					if (!NT_SUCCESS(status))
					{
						WdfObjectDelete(mem);
						break;
					}
					else
					{
						if ((((PEV_COMANDO)bufIn)->Tipo == TipoComando_MfdHora) || (((PEV_COMANDO)bufIn)->Tipo == TipoComando_MfdHora24))
						{
							GetDeviceContext(device)->MenuMFD.HoraActivada = FALSE;
						}
						else if (((PEV_COMANDO)bufIn)->Tipo == TipoComando_MfdFecha)
						{
							GetDeviceContext(device)->MenuMFD.FechaActivada = FALSE;
						}
						bufIn += sizeof(EV_COMANDO);
						tamPrevisto += sizeof(EV_COMANDO);
						status = WdfCollectionAdd(colaComandos, mem);
						if (!NT_SUCCESS(status))
						{
							WdfObjectDelete(mem);
							break;
						}
					}
				}
			}
			if (!NT_SUCCESS(status))
			{
				break;
			}
			status = WdfCollectionAdd(devExt->Acciones, colaComandos);
			if (!NT_SUCCESS(status))
			{
				WdfObjectDelete(colaComandos);
				break;
			}
		}
	}
	WdfWaitLockRelease(devExt->WaitLockAcciones);

	WdfRequestSetInformation(Request, WdfCollectionGetCount(devExt->Acciones));
	if (!NT_SUCCESS(status))
	{
		LimpiarPerfil(device);
	}

	return status;
}