/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

Eventos.c

Abstract:

Generar eventos que luego se leen al HID

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "EventosProcesar.h"
#undef _PUBLIC
#define _PRIVATE_
#include "EventosGenerar.h"
#undef _PRIVATE_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, GenerarEventoRaton)
#pragma alloc_text (PAGE, GenerarEventoComando)
#pragma alloc_text (PAGE, GenerarEventoDirectX)
#endif

VOID GenerarEventoRaton(WDFDEVICE device, PVOID pev_comando)
{
	PAGED_CODE();

	NTSTATUS				status;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFCOLLECTION			eventos;
	HID_CONTEXT*			devExt = &GetDeviceContext(device)->HID;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;
	status = WdfCollectionCreate(&attributes, &eventos);
	if (NT_SUCCESS(status))
	{
		WDFMEMORY mem;
		attributes.ParentObject = eventos;
		status = WdfMemoryCreate(&attributes, PagedPool, (ULONG)'evtH', sizeof(EV_COMANDO), &mem, NULL);
		if (!NT_SUCCESS(status))
		{
			WdfObjectDelete(eventos);
		}
		else
		{
			RtlCopyMemory(WdfMemoryGetBuffer(mem, NULL), pev_comando, sizeof(EV_COMANDO));
			status = WdfCollectionAdd(eventos, mem);
			if (!NT_SUCCESS(status))
			{
				WdfObjectDelete(eventos);
			}
			else
			{
				WdfWaitLockAcquire(devExt->WaitLockEventos, NULL);
				{
					status = WdfCollectionAdd(devExt->ColaEventos, eventos);
					if (!NT_SUCCESS(status))
					{
						WdfObjectDelete(eventos);
					}
				}
				WdfWaitLockRelease(devExt->WaitLockEventos);
			}
		}
	}
}

VOID GenerarEventoComando(WDFDEVICE device, UINT16 accionId, UCHAR origen, UCHAR tipoOrigen, PVOID datosEje)
{
	PAGED_CODE();

	HID_CONTEXT*		devExt = &GetDeviceContext(device)->HID;
	PROGRAMADO_CONTEXT*	pdevExt = &GetDeviceContext(device)->Perfil;

	if (tipoOrigen == Origen_Seta)
	{
		origen += 64;
	}
	else  if (tipoOrigen == Origen_Eje)
	{
		origen += 128;
	}
	
	if(accionId != 0)
	{
		NTSTATUS				status;
		WDF_OBJECT_ATTRIBUTES	attributes;
		WDFCOLLECTION			eventos;

		WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = device;
		status = WdfCollectionCreate(&attributes, &eventos);
		if (NT_SUCCESS(status))
		{
			BOOLEAN ok = TRUE;
			WdfWaitLockAcquire(pdevExt->WaitLockAcciones, NULL);
			{
				if (WdfCollectionGetCount(pdevExt->Acciones) == 0)
				{
					WdfObjectDelete(eventos);
					ok = FALSE;
				}
				else
				{
					WDFCOLLECTION comandos = (WDFCOLLECTION)WdfCollectionGetItem(pdevExt->Acciones, accionId - 1);
					if (comandos == NULL)
					{
						WdfObjectDelete(eventos);
						ok = FALSE;
					}
					else
					{
						UCHAR idx;
						for (idx = 0; idx < WdfCollectionGetCount(comandos); idx++)
						{
							WDFMEMORY evtMem;
							attributes.ParentObject = eventos;
							status = WdfMemoryCreate(&attributes, PagedPool, (ULONG)'evtH', (tipoOrigen != Origen_Eje) ? sizeof(EV_COMANDO) : sizeof(EV_COMANDO_EXTENDIDO) , &evtMem, NULL);
							if (!NT_SUCCESS(status))
							{
								WdfObjectDelete(eventos);
								ok = FALSE;
								break;
							}
							else
							{
								PEV_COMANDO pEvt = WdfMemoryGetBuffer(evtMem, NULL);
								RtlCopyMemory(pEvt, WdfMemoryGetBuffer(WdfCollectionGetItem(comandos, idx), NULL), sizeof(EV_COMANDO));
								if ((pEvt->Tipo == TipoComando_Hold) || ((pEvt->Tipo & 0x7f) == TipoComando_Repeat))
								{
									pEvt->Dato = origen;
									if (tipoOrigen == Origen_Eje)
									{
										PEV_COMANDO_EXTENDIDO pEvtEx =(PEV_COMANDO_EXTENDIDO)pEvt;
										pEvtEx->Pinkie = ((PEV_COMANDO_EXTENDIDO)datosEje)->Pinkie;
										pEvtEx->Modo = ((PEV_COMANDO_EXTENDIDO)datosEje)->Modo;
										pEvtEx->Incremental = ((PEV_COMANDO_EXTENDIDO)datosEje)->Incremental;
										pEvtEx->Banda = ((PEV_COMANDO_EXTENDIDO)datosEje)->Banda;
									}
								}
								status = WdfCollectionAdd(eventos, evtMem);
								if (!NT_SUCCESS(status))
								{
									WdfObjectDelete(eventos);
									ok = FALSE;
									break;
								}
							}
						}
					}
				}
			}
			WdfWaitLockRelease(pdevExt->WaitLockAcciones);
			if(ok)
			{
				WdfWaitLockAcquire(devExt->WaitLockEventos, NULL);
				{
					status = WdfCollectionAdd(devExt->ColaEventos, eventos);
					if (!NT_SUCCESS(status))
					{
						WdfObjectDelete(eventos);
					}
				}
				WdfWaitLockRelease(devExt->WaitLockEventos);
			}
		}
	}
}

VOID GenerarEventoDirectX(WDFDEVICE device, PHID_INPUT_DATA inputData)
{
	PAGED_CODE();

	HID_CONTEXT*			devExt = &GetDeviceContext(device)->HID;
	NTSTATUS				status;
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFCOLLECTION			eventos;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
	attributes.ParentObject = device;
	status = WdfCollectionCreate(&attributes, &eventos);
	if (NT_SUCCESS(status))
	{
		WDFMEMORY mem;
		attributes.ParentObject = eventos;
		status = WdfMemoryCreate(&attributes, PagedPool, (ULONG)'evtH', sizeof(HID_INPUT_DATA) + 1, &mem, NULL);
		if (!NT_SUCCESS(status))
		{
			WdfObjectDelete(eventos);
		}
		else
		{
			*(PUCHAR)WdfMemoryGetBuffer(mem, NULL) = TipoComando_Reservado_DxPosicion;
			status = WdfMemoryCopyFromBuffer(mem, 1, inputData, sizeof(HID_INPUT_DATA));
			if (!NT_SUCCESS(status))
			{
				WdfObjectDelete(eventos);
			}
			else
			{
				status = WdfCollectionAdd(eventos, mem);
				if (!NT_SUCCESS(status))
				{
					WdfObjectDelete(eventos);
				}
				else
				{
					WdfWaitLockAcquire(devExt->WaitLockEventos, NULL);
					{
						status = WdfCollectionAdd(devExt->ColaEventos, eventos);
						if (!NT_SUCCESS(status))
						{
							WdfObjectDelete(eventos);
						}
					}
					WdfWaitLockRelease(devExt->WaitLockEventos);
				}
			}
		}
	}
}
