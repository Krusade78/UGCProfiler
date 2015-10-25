#pragma once
#include <ntddk.h>
#include <wdf.h>

NTSTATUS IniciarIOUsrControl(_In_ WDFDEVICE device);

#ifdef _PRIVATE_
typedef struct _IOUSR_CONTROL_EXTENSION
{
	WDFDEVICE Padre;
} IOUSR_CONTROL_EXTENSION, *PIOUSR_CONTROL_EXTENSION;
WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(IOUSR_CONTROL_EXTENSION, GetIOUsrControlExtension);

typedef void (*PENVIARORDEN_X52)(_In_ PINTERFACE InterfaceHeader, _In_ ULONG ctlCode, _In_ PUCHAR buffer, _In_ size_t tamBuffer);
typedef struct _CONTROL_INTERFACE
{
	INTERFACE			InterfaceHeader; // Standard interface header, must be present
	PENVIARORDEN_X52	EnviarOrdenX52;

} CONTROL_INTERFACE, *PCONTROL_INTERFACE;

NTSTATUS IniciarInterface(WDFDEVICE device, WDFDEVICE deviceControl);

EVT_WDF_IO_QUEUE_IO_INTERNAL_DEVICE_CONTROL IoControl;

void EnviarOrdenX52(_In_ PINTERFACE InterfaceHeader, _In_ ULONG ctlCode, _In_ PUCHAR buffer, _In_ size_t tamBuffer);
#endif
