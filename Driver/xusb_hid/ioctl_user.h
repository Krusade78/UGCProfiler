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

EVT_WDF_IO_QUEUE_IO_INTERNAL_DEVICE_CONTROL HF_IoControl;
#endif
