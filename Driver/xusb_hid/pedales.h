#pragma once
#include <ntddk.h>
#include <wdf.h>


NTSTATUS IniciarPedales(_In_ WDFDEVICE device);
void CerrarPedales(_In_ WDFDEVICE device);

#ifdef _PRIVATE_

DRIVER_NOTIFICATION_CALLBACK_ROUTINE PnPCallback;

NTSTATUS IniciarIoTarget(_In_ PUNICODE_STRING strId, _In_ WDFDEVICE device);

NTSTATUS CerrarIoTarget(_In_ WDFDEVICE ioTarget);
EVT_WDF_WORKITEM CerrarIoTargetPassive;

EVT_WDF_IO_TARGET_REMOVE_COMPLETE EvIoTargetRemoveComplete;
EVT_WDF_IO_TARGET_REMOVE_CANCELED EvIoTargetRemoveCanceled;

NTSTATUS IniciarReports(WDFIOTARGET ioTarget);

void CompletionPedales(_In_ WDFREQUEST request, _In_ WDFIOTARGET ioTarget, _In_ PWDF_REQUEST_COMPLETION_PARAMS params, _In_ WDFCONTEXT context);

void ProcesarEntradaPedales(_In_ WDFDEVICE device, _In_ PVOID buffer);

#endif