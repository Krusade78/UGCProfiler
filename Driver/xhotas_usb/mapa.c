#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "acciones.h"
#include "mapa.h"

NTSTATUS HF_IoEscribirMapa(_In_ WDFDEVICE device, _In_ WDFREQUEST Request)
{
    PROGRAMADO_CONTEXT*		devExt = &GetDeviceContext(device)->Programacion;
    NTSTATUS                status = STATUS_SUCCESS;
	PUCHAR					SystemBuffer;
	size_t					tam;

	status =  WdfRequestRetrieveInputBuffer(Request, (sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini) + 1 + sizeof(devExt->MapaBotones)+sizeof(devExt->MapaSetas)), &SystemBuffer, &tam);
	if(!NT_SUCCESS(status))
		return status;

	if( tam == (sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini) + 1 + sizeof(devExt->MapaBotones)+sizeof(devExt->MapaSetas)) )
	{
		WdfSpinLockAcquire(devExt->slMapas);
		{
			RtlCopyMemory(devExt->MapaEjes,		SystemBuffer, sizeof(devExt->MapaEjes));
			RtlCopyMemory(devExt->MapaEjesPeque, SystemBuffer + sizeof(devExt->MapaEjes), sizeof(devExt->MapaEjesPeque));
			RtlCopyMemory(devExt->MapaEjesMini,	SystemBuffer + sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque), sizeof(devExt->MapaEjesMini));
			RtlCopyMemory(&devExt->TickRaton,	SystemBuffer + sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini), 1);
			RtlCopyMemory(devExt->MapaBotones,	SystemBuffer + sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini) + 1, sizeof(devExt->MapaBotones));
			RtlCopyMemory(devExt->MapaSetas,		SystemBuffer + sizeof(devExt->MapaEjes) + sizeof(devExt->MapaEjesPeque) + sizeof(devExt->MapaEjesMini) + 1 + sizeof(devExt->MapaBotones), sizeof(devExt->MapaSetas));
		}
		WdfSpinLockRelease(devExt->slMapas);

		WdfRequestSetInformation(Request, tam);
	}
	else
	{
		LimpiarMapa(device);
		status = STATUS_BUFFER_TOO_SMALL;
		WdfRequestSetInformation(Request, 0);
	}

	return status;
}

NTSTATUS HF_IoEscribirComandos(_In_ WDFDEVICE device, _In_ WDFREQUEST Request)
{
	PROGRAMADO_CONTEXT*		devExt = &GetDeviceContext(device)->Programacion;
    NTSTATUS                status = STATUS_SUCCESS;
	UINT16					npos = 0;
	UINT32					idxc = 0;
	PUCHAR					bufIn;
	PVOID					SystemBuffer;
	size_t					InputBufferLength;

	LimpiarMapa(device);

	status =  WdfRequestRetrieveInputBuffer(Request, 2, &SystemBuffer, &InputBufferLength);
	if(!NT_SUCCESS(status))
		return status;

	RtlCopyMemory(&npos, SystemBuffer, 2);
	if(npos >= (PAGE_SIZE / sizeof(COMANDO)))
	{
		status = STATUS_INSUFFICIENT_RESOURCES;
		WdfRequestSetInformation(Request, 2);
	}
	else if(npos == 0)
		WdfRequestSetInformation(Request, 2);
	else
	{

		bufIn = (PUCHAR)SystemBuffer + 2;

		WdfSpinLockAcquire(devExt->slComandos);

		devExt->Comandos = (PCOMANDO)ExAllocatePoolWithTag(NonPagedPool, sizeof(COMANDO) * npos, (ULONG)'ocpV');
		if(devExt->Comandos == NULL)
		{
			WdfSpinLockRelease(devExt->slComandos);
			status = STATUS_INSUFFICIENT_RESOURCES;
			WdfRequestSetInformation(Request, 2);
		}
		else
		{
			while(devExt->nComandos != npos)
			{
				if(InputBufferLength < (idxc + 3))
				{
					status = STATUS_BUFFER_TOO_SMALL;
					goto mal;
				}
				devExt->Comandos[devExt->nComandos].tam  =bufIn[idxc]; idxc++;
				if(InputBufferLength < (idxc + 2 + (devExt->Comandos[devExt->nComandos].tam * 2)))
				{
					status = STATUS_BUFFER_TOO_SMALL;
					goto mal;
				}
				devExt->Comandos[devExt->nComandos].datos = (PUINT16)ExAllocatePoolWithTag(NonPagedPool, 2 * devExt->Comandos[devExt->nComandos].tam, (ULONG)'ocpV');
				if(devExt->Comandos[devExt->nComandos].datos == NULL)
				{
					status = STATUS_INSUFFICIENT_RESOURCES;
					goto mal;
				}
				else
				{
					RtlCopyMemory((PUCHAR)devExt->Comandos[devExt->nComandos].datos, &bufIn[idxc], 2 * devExt->Comandos[devExt->nComandos].tam);
					idxc += 2 * devExt->Comandos[devExt->nComandos].tam;
					devExt->nComandos++;
				}
			}

			WdfSpinLockRelease(devExt->slComandos);
			goto fin;
mal:
			WdfSpinLockRelease(devExt->slComandos);
			LimpiarMapa(device);
fin:
			WdfRequestSetInformation(Request, idxc + 2);
		}

	}

	return status;
}

VOID LimpiarMapa(WDFDEVICE device)
{
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT*		devExt = &GetDeviceContext(device)->HID;
	UINT16 idx;

	WdfSpinLockAcquire(idevExt->slComandos);
	{
		WdfSpinLockAcquire(devExt->SpinLockAcciones);
		{
			WdfSpinLockAcquire(idevExt->slMapas);
			{
				RtlZeroMemory(idevExt->MapaBotones, sizeof(idevExt->MapaBotones));
				RtlZeroMemory(idevExt->MapaSetas, sizeof(idevExt->MapaSetas));
				RtlZeroMemory(idevExt->MapaEjes, sizeof(idevExt->MapaEjes));
				RtlZeroMemory(idevExt->MapaEjesPeque, sizeof(idevExt->MapaEjesPeque));
				RtlZeroMemory(idevExt->MapaEjesMini, sizeof(idevExt->MapaEjesMini));
				RtlZeroMemory(idevExt->posVieja, sizeof(idevExt->posVieja));
			}
			WdfSpinLockRelease(idevExt->slMapas);

			if (idevExt->Comandos != NULL)
			{
				for (idx = 0; idx < idevExt->nComandos; idx++)
				{
					if (idevExt->Comandos[idx].datos != NULL) ExFreePoolWithTag((PVOID)idevExt->Comandos[idx].datos, (ULONG)'ocpV');
				}
				ExFreePoolWithTag((PVOID)idevExt->Comandos, (ULONG)'ocpV');
				idevExt->Comandos = NULL;
				idevExt->nComandos = 0;
			}

			RtlZeroMemory(devExt->stTeclado, sizeof(devExt->stTeclado));
			RtlZeroMemory(devExt->stRaton, sizeof(devExt->stRaton));
			RtlZeroMemory(devExt->stBotones, sizeof(devExt->stBotones));
			RtlZeroMemory(devExt->stSetas, sizeof(devExt->stSetas));
			devExt->EstadoPinkie = FALSE;
			devExt->EstadoModos = 0;
		}
		WdfSpinLockRelease(devExt->SpinLockAcciones);

		LimpiarAcciones(device);
	}
	WdfSpinLockRelease(idevExt->slComandos);
}