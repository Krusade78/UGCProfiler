#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "mapa.h"

#define _CONTROL_
#include "control.h"
#undef _CONTROL_

#ifdef ALLOC_PRAGMA
    #pragma alloc_text( PAGE, HF_Control)
#endif /* ALLOC_PRAGMA */

#define IOCTL_USR_CALIBRADO		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0100, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_CALIBRAR		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0101, METHOD_BUFFERED, FILE_READ_ACCESS)
#define IOCTL_USR_DESCALIBRAR	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0102, METHOD_BUFFERED, FILE_READ_ACCESS)
#define IOCTL_USR_MAPA			CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0103, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_COMANDOS		CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0104, METHOD_BUFFERED, FILE_WRITE_ACCESS)
#define IOCTL_USR_CONPEDALES	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0105, METHOD_BUFFERED, FILE_READ_ACCESS)
#define IOCTL_USR_SINPEDALES	CTL_CODE(FILE_DEVICE_UNKNOWN, 0x0106, METHOD_BUFFERED, FILE_READ_ACCESS)

VOID
HF_Control(
			__in  WDFQUEUE Queue,
			__in  WDFREQUEST Request,
			__in  size_t OutputBufferLength,
			__in  size_t InputBufferLength,
			__in  ULONG IoControlCode
			)
{
	NTSTATUS			status	= STATUS_SUCCESS;
    PDEVICE_EXTENSION   devExt	= NULL;

    UNREFERENCED_PARAMETER(OutputBufferLength);
    UNREFERENCED_PARAMETER(InputBufferLength);

	PAGED_CODE();

    devExt = GetControlExtension(WdfIoQueueGetDevice(Queue))->devExt;

	switch(IoControlCode)
	{
		case IOCTL_USR_COMANDOS:
			status = HF_IoEscribirComandos(Request, devExt);
			break;
		case IOCTL_USR_MAPA:
			status = HF_IoEscribirMapa(Request, devExt);
			break;
		case IOCTL_USR_CALIBRADO:
			status = HF_IoEscribirCalibrado(Request, devExt);
			break;
		case IOCTL_USR_CALIBRAR:
			devExt->itfExt.descalibrar = FALSE;
			break;
		case IOCTL_USR_DESCALIBRAR:
			devExt->itfExt.descalibrar = TRUE;
			break;
		case IOCTL_USR_CONPEDALES:
			devExt->PedalesActivados = TRUE;
			break;
		case IOCTL_USR_SINPEDALES:
			devExt->PedalesActivados = FALSE;
			break;
		default:
			status = STATUS_NOT_SUPPORTED;
	}

	WdfRequestComplete(Request, status);
}

NTSTATUS
HF_IoEscribirCalibrado(
    IN WDFREQUEST Request,
    IN PDEVICE_EXTENSION DeviceExtension
	)
{
    PITFDEVICE_EXTENSION	devExt = &DeviceExtension->itfExt;
    NTSTATUS                status = STATUS_SUCCESS;
	UCHAR					i;
	PCALIBRADO				bufCal = NULL;

    status =  WdfRequestRetrieveInputBuffer(Request, (sizeof(CALIBRADO) * 4), &bufCal, NULL);
	WdfRequestSetInformation(Request, sizeof(CALIBRADO) * 4);
	if(!NT_SUCCESS(status))	return status;

	for(i = 0; i < 4; i++)
	{
		WdfSpinLockAcquire(devExt->slCalibrado);
			devExt->limites[i].c = bufCal[i].c;
			devExt->limites[i].i = bufCal[i].i;
			devExt->limites[i].d = bufCal[i].d;
			devExt->limites[i].n = bufCal[i].n;
			devExt->limites[i].cal = bufCal[i].cal;
			devExt->jitter[i].Margen = bufCal[i].Margen;
			devExt->jitter[i].Resistencia = bufCal[i].Resistencia;
			devExt->jitter[i].antiv = bufCal[i].antiv;
		WdfSpinLockRelease(devExt->slCalibrado);
	}

	return status;
}