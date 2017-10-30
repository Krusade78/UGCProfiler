#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "entradahid.h"
#include "initguid.h"
#include <Usbiodef.h>
#include <Ntstrsafe.h>
#include <hidport.h>

#define _IOTARGETS_
#include "IoTargets.h"
#undef _IOTARGETS_

#ifdef ALLOC_PRAGMA
    #pragma alloc_text( PAGE, IniciarIoTargets)
    #pragma alloc_text( PAGE, CerrarIoTargets)
    #pragma alloc_text( PAGE, EvIoTargetRemove)
#endif /* ALLOC_PRAGMA */

#define HARDWARE_ID_PEDALES  L"\\??\\HID#Vid_044f&Pid_b653"
#define HARDWARE_ID_X52		 L"\\??\\HID#Vid_06a3&Pid_0255"
//#define HARDWARE_ID_X36U1	 L"\\??\\HID#VID_06A3&PID_053F"
//#define HARDWARE_ID_X36U2	 L"\\??\\HID#VID_06A3&PID_803F"
//#define HARDWARE_ID_X45U1	 L"\\??\\HID#VID_06A3&PID_053C"
//#define HARDWARE_ID_X45U2	 L"\\??\\HID#VID_06A3&PID_803C"
//#define HARDWARE_ID_X45U3	 L"\\??\\HID#VID_06A3&PID_2541"
//#define HARDWARE_ID_USB_X52  L"\\??\\USB#Vid_06a3&Pid_0255"
#define REPORTPEDALES_TAM	 8
#define REPORTX36X45_TAM	 12
#define REPORTX52_TAM		 15

DECLARE_CONST_UNICODE_STRING(X52USBInterface, L"\\??\\XUSBInterface");

void IoTargetRemove(IN WDFIOTARGET IoTarget)
{
	PDEVICE_EXTENSION	devExt;

	if(IoTarget != NULL)
	{
		devExt = GetDeviceExtension(WdfIoTargetGetDevice(IoTarget));
		if(devExt->TargetHIDHOTAS == IoTarget)
			devExt->TargetHIDHOTAS = NULL;
		else if(devExt->TargetHIDPedales == IoTarget)
			devExt->TargetHIDPedales = NULL;
		else if(devExt->TargetUSBX52 == IoTarget)
			devExt->TargetUSBX52 = NULL;

		//WdfIoTargetClose(IoTarget);
		WdfObjectDelete(IoTarget);
	}
}

void IniciarIoTargets(IN WDFWORKITEM  WorkItem)
{
	WDFDEVICE	Device = WdfWorkItemGetParentObject(WorkItem);
	UCHAR		nulos = *WdfObjectGet_WI_CONTEXT(WorkItem);
	PWSTR		links, plinks;
	NTSTATUS	status;

	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceExtension(Device)->WaitLockCierre, NULL);
	if(GetDeviceExtension(Device)->D0Apagado) 
	{
		WdfWaitLockRelease(GetDeviceExtension(Device)->WaitLockCierre);
		return;
	}

	if((nulos & 1) || (nulos & 2))
	{
		status = IoGetDeviceInterfaces(&GUID_DEVINTERFACE_HID, NULL, 0, &links);
		if(NT_SUCCESS(status))
		{
			plinks = links;
			while(*plinks != 0)
			{
				size_t tamitf = 0, tamid = 0;

				RtlStringCchLengthW(plinks, NTSTRSAFE_MAX_CCH, &tamitf);
				RtlStringCchLengthW(HARDWARE_ID_X52, NTSTRSAFE_MAX_CCH, &tamid);
				if(tamitf >= tamid)
				{
					WCHAR charPrevio	= plinks[tamid];
					BOOLEAN pedales		= FALSE;
					BOOLEAN hotas		= FALSE;
					UNICODE_STRING stLink, stId; 

					plinks[tamid] = L'\0';
					RtlInitUnicodeString(&stLink, plinks);
					RtlInitUnicodeString(&stId, HARDWARE_ID_PEDALES); 
					if((RtlCompareUnicodeString(&stLink, &stId, TRUE) == 0) && (nulos & 1)) pedales = TRUE;
					RtlInitUnicodeString(&stId, HARDWARE_ID_X52); 
					if((RtlCompareUnicodeString(&stLink, &stId, TRUE) == 0) && (nulos & 2)) hotas = TRUE;
					if(pedales || hotas)
					{
						WDF_OBJECT_ATTRIBUTES attributes;
						WDFIOTARGET IoTarget;
						WDF_IO_TARGET_OPEN_PARAMS  openParams;
						//WDF_IO_TARGET_STATE state;

						plinks[tamid] = charPrevio;
						RtlInitUnicodeString(&stLink, plinks);

						WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
							attributes.ParentObject = Device;

						status = WdfIoTargetCreate(Device, &attributes, &IoTarget);
						if(NT_SUCCESS(status))
						{
							WDF_IO_TARGET_OPEN_PARAMS_INIT_OPEN_BY_NAME(&openParams, &stLink, STANDARD_RIGHTS_READ);
								openParams.ShareAccess					= FILE_SHARE_READ | FILE_SHARE_WRITE;
								openParams.EvtIoTargetRemoveComplete	= EvIoTargetRemove;

							status = WdfIoTargetOpen(IoTarget, &openParams);
							if(NT_SUCCESS(status) && ((openParams.FileInformation == FILE_SUPERSEDED) || (openParams.FileInformation == FILE_OPENED)))
							{
								if(WdfIoTargetGetState(IoTarget) == WdfIoTargetStarted)
								{
									if(pedales)
									{
										PDPCTARGETS_CONTEXT ctx = WdfObjectGet_DPCTARGETS_CONTEXT(GetDeviceExtension(Device)->DpcTargets[0]);
										ctx->target = IoTarget;
										ctx->tipo = 1;
										if(!WdfDpcEnqueue(GetDeviceExtension(Device)->DpcTargets[0]))
											WdfObjectDelete(IoTarget);
									}
									else
									{
										PDPCTARGETS_CONTEXT ctx = WdfObjectGet_DPCTARGETS_CONTEXT(GetDeviceExtension(Device)->DpcTargets[1]);
										ctx->target = IoTarget;
										ctx->tipo = 2;
										if(!WdfDpcEnqueue(GetDeviceExtension(Device)->DpcTargets[1]))
											WdfObjectDelete(IoTarget);
									}
								}
								else
								{
									WdfObjectDelete(IoTarget);
								}

							}
							else
							{
								WdfObjectDelete(IoTarget);
							}
						}
					}
				}	

				plinks = &plinks[tamitf + 1];
			}

			ExFreePool(links); links = NULL;
		}
	}
	if(nulos & 4)
	{
		WDF_OBJECT_ATTRIBUTES attributes;
		WDFIOTARGET IoTarget;
		WDF_IO_TARGET_OPEN_PARAMS  openParams;
		//WDF_IO_TARGET_STATE state;

		WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
			attributes.ParentObject = Device;

		status = WdfIoTargetCreate(Device, &attributes, &IoTarget);
		if(NT_SUCCESS(status))
		{
			WDF_IO_TARGET_OPEN_PARAMS_INIT_OPEN_BY_NAME(&openParams, &X52USBInterface, STANDARD_RIGHTS_WRITE);
				openParams.ShareAccess				 = FILE_SHARE_WRITE;
				openParams.EvtIoTargetRemoveComplete = EvIoTargetRemove;

			status = WdfIoTargetOpen(IoTarget, &openParams);
			if(NT_SUCCESS(status))
			{
				if(WdfIoTargetGetState(IoTarget) == WdfIoTargetStarted)
				{
					PDPCTARGETS_CONTEXT ctx = WdfObjectGet_DPCTARGETS_CONTEXT(GetDeviceExtension(Device)->DpcTargets[2]);
					ctx->target = IoTarget;
					ctx->tipo = 3;
					if(!WdfDpcEnqueue(GetDeviceExtension(Device)->DpcTargets[2]))
						WdfObjectDelete(IoTarget);
				}
				else
				{
					WdfObjectDelete(IoTarget);
				}

			}
			else
			{
				WdfObjectDelete(IoTarget);
			}
		}
	}

	WdfWaitLockRelease(GetDeviceExtension(Device)->WaitLockCierre);
}

void CerrarIoTargets(IN WDFDEVICE  Device)
{
	PAGED_CODE();

	WdfTimerStop(GetDeviceExtension(Device)->TimerIoTargets, TRUE);
	IoTargetRemove(GetDeviceExtension(Device)->TargetHIDPedales);
	IoTargetRemove(GetDeviceExtension(Device)->TargetHIDHOTAS);
	IoTargetRemove(GetDeviceExtension(Device)->TargetUSBX52);
}

VOID EvIoTargetRemove (IN WDFIOTARGET  IoTarget)
{
	PAGED_CODE();

	IoTargetRemove(IoTarget);
}

VOID EvDpcAbrirCerrarTargets(IN WDFDPC  Dpc)
{
	switch(WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->tipo)
	{
		case 1:
			if(GetDeviceExtension(WdfDpcGetParentObject(Dpc))->TargetHIDPedales == NULL)
			{
				GetDeviceExtension(WdfDpcGetParentObject(Dpc))->TargetHIDPedales = WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target;
				IniciarReportsPedales(WdfDpcGetParentObject(Dpc));
			}
			else
				WdfObjectDelete(WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target);

			break;
		case 2:
			if(GetDeviceExtension(WdfDpcGetParentObject(Dpc))->TargetHIDHOTAS == NULL)
			{
				GetDeviceExtension(WdfDpcGetParentObject(Dpc))->TargetHIDHOTAS = WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target;
				IniciarReportsHOTAS(WdfDpcGetParentObject(Dpc), REPORTX52_TAM);
			}
			else
				WdfObjectDelete(WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target);

			break;
		case 3:
			if(GetDeviceExtension(WdfDpcGetParentObject(Dpc))->TargetUSBX52 == NULL)
				GetDeviceExtension(WdfDpcGetParentObject(Dpc))->TargetUSBX52 = WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target;
			else
				WdfObjectDelete(WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target);

			break;
		case -1:
			IoTargetRemove(WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target);
			break;
		case -2:
			IoTargetRemove(WdfObjectGet_DPCTARGETS_CONTEXT(Dpc)->target);
			break;
	}
}

VOID TimerTickTargets(__in  WDFTIMER Timer)
{ //DISPATCH_LEVEL
	PDEVICE_EXTENSION devExt = GetDeviceExtension(WdfTimerGetParentObject(Timer));
	UCHAR nulos = 0;

	if(devExt->TargetHIDPedales == NULL)
		nulos |= 1;
	if(devExt->TargetHIDHOTAS == NULL)
		nulos |= 2;
	if(devExt->TargetUSBX52 == NULL)
		nulos |= 4;
	if(nulos)
	{
		*WdfObjectGet_WI_CONTEXT(devExt->WorkItemTargets) = nulos;
		WdfWorkItemEnqueue(devExt->WorkItemTargets);
	}
}


void IniciarReportsPedales(IN WDFDEVICE Device)
{
	WDFREQUEST	req = NULL;
	
	req = IniciarRequestHID(Device, GetDeviceExtension(Device)->TargetHIDPedales, REPORTPEDALES_TAM);
	if(req == NULL)
	{
		IoTargetRemove(GetDeviceExtension(Device)->TargetHIDPedales);
		return;
	}

	WdfRequestSetCompletionRoutine(req, CompletionPedales, NULL);

	if(WdfRequestSend(req, GetDeviceExtension(Device)->TargetHIDPedales, NULL) == FALSE)
		IoTargetRemove(GetDeviceExtension(Device)->TargetHIDPedales);
}

void IniciarReportsHOTAS(IN WDFDEVICE Device, size_t tamBuffer)
{
	WDFREQUEST	req = NULL;
	
	req = IniciarRequestHID(Device, GetDeviceExtension(Device)->TargetHIDHOTAS, tamBuffer);
	if(req == NULL)
	{
		IoTargetRemove(GetDeviceExtension(Device)->TargetHIDHOTAS);
		return;
	}

	WdfRequestSetCompletionRoutine(req, CompletionHOTAS, NULL);

	if(WdfRequestSend(req, GetDeviceExtension(Device)->TargetHIDHOTAS, NULL) == FALSE)
		IoTargetRemove(GetDeviceExtension(Device)->TargetHIDHOTAS);
}

WDFREQUEST IniciarRequestHID(IN WDFDEVICE Device, WDFIOTARGET IoTarget, size_t tamBuffer)
{
	UNREFERENCED_PARAMETER(Device);

	//PDEVICE_EXTENSION		devExt = GetDeviceExtension(Device);
	WDF_OBJECT_ATTRIBUTES	attributes;
	WDFREQUEST				newRequest = NULL;
	WDFMEMORY				writeBufferMemHandle;
	NTSTATUS				status;	
	PVOID					buffer;

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = IoTarget;

	status = WdfRequestCreate(&attributes, IoTarget, &newRequest);
	if(!NT_SUCCESS(status))
	{
		return NULL;
	}

	WDF_OBJECT_ATTRIBUTES_INIT(&attributes);
		attributes.ParentObject = newRequest;
	status = WdfMemoryCreate(&attributes, NonPagedPool, 0,  tamBuffer, &writeBufferMemHandle, &buffer);
	if(!NT_SUCCESS(status))
	{
		WdfObjectDelete(newRequest);
		return NULL;
	}

	status = WdfIoTargetFormatRequestForRead(IoTarget, newRequest, writeBufferMemHandle, NULL, NULL);
	if (!NT_SUCCESS(status))
	{
		WdfObjectDelete(writeBufferMemHandle);
		WdfObjectDelete(newRequest);
		return NULL;
	}

	return newRequest;
}

VOID CompletionPedales(
    IN WDFREQUEST  Request,
    IN WDFIOTARGET  Target,
    IN PWDF_REQUEST_COMPLETION_PARAMS  Params,
    IN WDFCONTEXT  Context
    )
{
	UNREFERENCED_PARAMETER(Context);

	PDEVICE_EXTENSION devExt = GetDeviceExtension(WdfIoTargetGetDevice(Target));
    NTSTATUS  status;
	PDPCTARGETS_CONTEXT ctx;

	status = Params->IoStatus.Status;
	if(NT_SUCCESS(status))
	{
		WDF_REQUEST_REUSE_PARAMS  rparams;
		WDFMEMORY mem = Params->Parameters.Read.Buffer;

		if(Params->Parameters.Read.Length == REPORTPEDALES_TAM)
		{
			ProcesarEntradaPedales(devExt, WdfMemoryGetBuffer(Params->Parameters.Read.Buffer, NULL));
		}
		
		WDF_REQUEST_REUSE_PARAMS_INIT(&rparams, WDF_REQUEST_REUSE_NO_FLAGS, STATUS_SUCCESS);
		status = WdfRequestReuse(Request, &rparams);
		if(NT_SUCCESS(status))
		{
			WdfRequestSetCompletionRoutine(Request, CompletionPedales, NULL);
			status = WdfIoTargetFormatRequestForRead(Target, Request, mem, NULL, NULL);
			if(NT_SUCCESS(status))
				if(WdfRequestSend(Request, Target, NULL) != FALSE)
					return;
		}
	
	}

	ctx = WdfObjectGet_DPCTARGETS_CONTEXT(devExt->DpcTargets[3]);
	ctx->target = Target;
	ctx->tipo = -1;
	WdfDpcEnqueue(devExt->DpcTargets[3]);
}

VOID CompletionHOTAS(
    IN WDFREQUEST  Request,
    IN WDFIOTARGET  Target,
    IN PWDF_REQUEST_COMPLETION_PARAMS  Params,
    IN WDFCONTEXT  Context
    )
{
	UNREFERENCED_PARAMETER(Context);

	PDEVICE_EXTENSION devExt = GetDeviceExtension(WdfIoTargetGetDevice(Target));
    NTSTATUS  status;
	PDPCTARGETS_CONTEXT ctx;

	status = Params->IoStatus.Status;
	if(NT_SUCCESS(status))
	{
		WDF_REQUEST_REUSE_PARAMS rparams;
		WDFMEMORY mem = Params->Parameters.Read.Buffer;

		if(Params->Parameters.Read.Length == REPORTX36X45_TAM)
			ProcesarEntradaHOTAS(devExt, WdfMemoryGetBuffer(Params->Parameters.Read.Buffer, NULL), FALSE);
		else if(Params->Parameters.Read.Length == REPORTX52_TAM)
			ProcesarEntradaHOTAS(devExt, WdfMemoryGetBuffer(Params->Parameters.Read.Buffer, NULL), TRUE);
		
		WDF_REQUEST_REUSE_PARAMS_INIT(&rparams, WDF_REQUEST_REUSE_NO_FLAGS, STATUS_SUCCESS);
		status = WdfRequestReuse(Request, &rparams);
		if(NT_SUCCESS(status))
		{
			WdfRequestSetCompletionRoutine(Request, CompletionHOTAS, NULL);
			status = WdfIoTargetFormatRequestForRead(Target, Request, mem, NULL, NULL);
			if(NT_SUCCESS(status))
				if(WdfRequestSend(Request, Target, NULL) != FALSE)
					return;
		}
	
	}

	ctx = WdfObjectGet_DPCTARGETS_CONTEXT(devExt->DpcTargets[4]);
	ctx->target = Target;
	ctx->tipo = -2;
	WdfDpcEnqueue(devExt->DpcTargets[4]);
}