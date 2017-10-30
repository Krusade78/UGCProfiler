VOID EnviarOrdenX52
(
	PDEVICE_EXTENSION devExt,
	UCHAR tipo,
	PVOID params,
	UCHAR tam
	);

#ifdef _X52_

EVT_WDF_REQUEST_COMPLETION_ROUTINE  CompletionX52;

#endif



