/*++
Copyright (c) 2021 Alfredo Costalago

Module Name:

EscribirUSBX52.c

Abstract:

Escritura de datos al USB del X52.

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include <usbdi.h>
#include <wdfusb.h>
#include "Context.h"
#define _PUBLIC_
#define _PRIVATE_
#include "EscribirUSBX52.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text(PAGE, Set_Texto)
#pragma alloc_text(PAGE, EnviarOrden)
#pragma alloc_text(PAGE, EnviarOrdenHilo)
#pragma alloc_text(PAGE, LimpiarSalidaX52)
#endif

NTSTATUS Set_Texto(_In_ WDFDEVICE DeviceObject, _In_ PUCHAR SystemBuffer, _In_ size_t tamBuffer)
{
	UCHAR params[3 * 17];
	UCHAR texto[16];
	UCHAR nparams = 1;
	UCHAR paramIdx = 0;

	PAGED_CODE();

	if ((tamBuffer - 1) > 16)
		return STATUS_INVALID_BUFFER_SIZE;

	RtlZeroMemory(texto, 16);
	RtlCopyMemory(texto, &(SystemBuffer)[1], tamBuffer - 1);


	params[0] = 0x0; params[1] = 0;
	switch (*(SystemBuffer)) //linea
	{
	case 1:
		params[2] = 0xd9;
		paramIdx = 0xd1;
		break;
	case 2:
		params[2] = 0xda;
		paramIdx = 0xd2;
		break;
	case 3:
		params[2] = 0xdc;
		paramIdx = 0xd4;
	}
	for (UCHAR i = 0; i < 16; i += 2)
	{
		if (texto[i] == 0)
			break;
		params[0 + (3 * nparams)] = texto[i];
		params[1 + (3 * nparams)] = texto[i + 1];
		params[2 + (3 * nparams)] = paramIdx;
		nparams++;
	}

	return EnviarOrden(DeviceObject, params, nparams);
}

NTSTATUS EnviarOrden(_In_ WDFDEVICE device, _In_ UCHAR* params, _In_ UCHAR nparams)
{
	NTSTATUS				status = STATUS_SUCCESS;
	WDF_OBJECT_ATTRIBUTES	attributes;

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes, NULL);
	{
		if (GetDeviceContext(device)->SalidaX52.Ordenes == NULL)
		{
			WdfWaitLockRelease(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes);
			return status;
		}
		else
		{
			WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = GetDeviceContext(device)->SalidaX52.Ordenes;

			UCHAR procesados = 0;
			BOOLEAN crearHilo = (WdfCollectionGetCount(GetDeviceContext(device)->SalidaX52.Ordenes) == 0);
			for (procesados = 0; procesados < nparams; procesados++)
			{
				PORDEN_X52 porden = NULL;
				WDFMEMORY mem;
				status = WdfMemoryCreate(&attributes, PagedPool, (ULONG)'ordX', sizeof(ORDEN_X52), &mem, &porden);
				if (!NT_SUCCESS(status))
				{
					break;
				}
				else
				{
					porden->valor = *((USHORT*)&params[procesados * 3]);
					porden->idx = params[2 + (procesados * 3)];
					status = WdfCollectionAdd(GetDeviceContext(device)->SalidaX52.Ordenes, mem);
					if (!NT_SUCCESS(status))
					{
						WdfObjectDelete(mem);
						break;
					}
				}
			}

			if (crearHilo && NT_SUCCESS(status))
			{
				HANDLE hilo;
				if (GetDeviceContext(device)->SalidaX52.Hilo != NULL)
				{
					PVOID hiloAntiguo = GetDeviceContext(device)->SalidaX52.Hilo;
					ObReferenceObject(hiloAntiguo);
					WdfWaitLockRelease(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes);
					KeWaitForSingleObject(hiloAntiguo, Executive, KernelMode, FALSE, NULL);
					ObDereferenceObject(hiloAntiguo);
					WdfWaitLockAcquire(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes, NULL);
				}
				status = PsCreateSystemThread(&hilo, (ACCESS_MASK)0, NULL, (HANDLE)NULL, NULL, EnviarOrdenHilo, GetDeviceContext(device));
				if (NT_SUCCESS(status))
				{
					ObReferenceObjectByHandle(hilo, THREAD_ALL_ACCESS, NULL, KernelMode, &GetDeviceContext(device)->SalidaX52.Hilo, NULL);
					ZwClose(hilo);
				}
			}

			if (!NT_SUCCESS(status))
			{
				for (; procesados > 0; procesados--)
				{
					WdfObjectDelete(WdfCollectionGetLastItem(GetDeviceContext(device)->SalidaX52.Ordenes));
					WdfCollectionRemoveItem(GetDeviceContext(device)->SalidaX52.Ordenes, WdfCollectionGetCount(GetDeviceContext(device)->SalidaX52.Ordenes) - 1);
				}
			}
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes);
	
	return status;
}

VOID EnviarOrdenHilo(PVOID context)
{
	PAGED_CODE();

	PDEVICE_CONTEXT devExt = (PDEVICE_CONTEXT)context;
	BOOLEAN salir = FALSE;

	KeSetPriorityThread(KeGetCurrentThread(), LOW_REALTIME_PRIORITY);

	while (!salir)
	{

		WDFMEMORY mem = NULL;
		WdfWaitLockAcquire(devExt->SalidaX52.WaitLockOrdenes, NULL);
		{
			if (devExt->SalidaX52.Ordenes != NULL)
			{
				mem = WdfCollectionGetFirstItem(devExt->SalidaX52.Ordenes);
				WdfCollectionRemoveItem(devExt->SalidaX52.Ordenes, 0);
				if (WdfCollectionGetCount(devExt->SalidaX52.Ordenes) == 0)
				{
					salir = TRUE;
				}
			}
			else
			{
				salir = TRUE;
			}
		}
		WdfWaitLockRelease(devExt->SalidaX52.WaitLockOrdenes);
		if (mem != NULL)
		{
			WDF_USB_CONTROL_SETUP_PACKET controlSetupPacket;
			ORDEN_X52 orden;
			RtlCopyMemory(&orden, WdfMemoryGetBuffer(mem, NULL), sizeof(ORDEN_X52));
			WdfObjectDelete(mem);

			WDF_USB_CONTROL_SETUP_PACKET_INIT_VENDOR(&controlSetupPacket, BmRequestHostToDevice, BmRequestToDevice,
														0x91, // Request
														orden.valor, // Value
														orden.idx); // Index  
			WdfUsbTargetDeviceSendControlTransferSynchronously(devExt->UsbDevice, NULL, NULL, &controlSetupPacket, NULL, NULL);
		}
	}

	WdfWaitLockAcquire(devExt->SalidaX52.WaitLockOrdenes, NULL);
	{
		ObDereferenceObject(devExt->SalidaX52.Hilo);
		devExt->SalidaX52.Hilo = NULL;
	}
	WdfWaitLockRelease(devExt->SalidaX52.WaitLockOrdenes);
	PsTerminateSystemThread(STATUS_SUCCESS);
}

VOID LimpiarSalidaX52(WDFOBJECT  Object)
{
	WDFDEVICE device = (WDFDEVICE)Object;
	PVOID philo = NULL;

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes, NULL);
	{
		if (GetDeviceContext(device)->SalidaX52.Ordenes != NULL)
		{
			while (WdfCollectionGetCount(GetDeviceContext(device)->SalidaX52.Ordenes) > 0)
			{
				WdfObjectDelete(WdfCollectionGetFirstItem(GetDeviceContext(device)->SalidaX52.Ordenes));
				WdfCollectionRemoveItem(GetDeviceContext(device)->SalidaX52.Ordenes, 0);
			}
			philo = GetDeviceContext(device)->SalidaX52.Hilo;
			if (philo != NULL)
			{
				ObReferenceObject(philo);
			}
			WdfObjectDelete(GetDeviceContext(device)->SalidaX52.Ordenes);
			GetDeviceContext(device)->SalidaX52.Ordenes = NULL;
		}
	}
	WdfWaitLockRelease(GetDeviceContext(device)->SalidaX52.WaitLockOrdenes);
	if (philo != NULL)
	{
		KeWaitForSingleObject(philo, Executive, KernelMode, FALSE, NULL);
		ObDereferenceObject(philo);
	}
}
