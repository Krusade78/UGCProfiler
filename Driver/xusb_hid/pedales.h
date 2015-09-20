#pragma once
#include <wdf.h>

NTSTATUS IniciarNotificacionPnP(_In_ WDFDEVICE device);

#ifdef _PRIVATE_

DRIVER_NOTIFICATION_CALLBACK_ROUTINE PnPCallback;

#endif