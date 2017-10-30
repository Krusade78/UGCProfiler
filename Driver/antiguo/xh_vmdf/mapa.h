NTSTATUS
HF_IoEscribirComandos(
    IN WDFREQUEST Request,
    IN PDEVICE_EXTENSION DeviceExtension
    );

NTSTATUS
HF_IoEscribirMapa(
    IN WDFREQUEST Request,
    IN PDEVICE_EXTENSION DeviceExtension
    );

VOID LimpiarMemoria(IN PDEVICE_EXTENSION devExt);