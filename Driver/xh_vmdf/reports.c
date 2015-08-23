#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "acciones.h"
#include "x52.h"

#define _REPORTS_
#include "reports.h"
#undef _REPORTS_

VOID ProcesarReport(IN WDFREQUEST Request)
{
	//NTSTATUS	status		= STATUS_SUCCESS;
	//PVOID		buffer		= NULL;
	//BOOLEAN		descancelar = TRUE;
	BOOLEAN		soloHolds	= FALSE;
	PDEVICE_EXTENSION devExt =  GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(Request)));

reintentar:
	WdfSpinLockAcquire(devExt->SpinLockAcciones);
		if(!ColaEstaVacia(&devExt->ColaAccionesComando))
		{
			PNODO siguiente = devExt->ColaAccionesComando.principio;
			soloHolds = TRUE;
			while(siguiente != NULL)
			{
				PCOLA cola = (PCOLA)siguiente->Datos;
				if( (((PEVENTO)cola->principio->Datos)->tipo & 0x1f) != 11)
				{
					soloHolds = FALSE;
					break;
				}
				siguiente = siguiente->link;
			}
		}
	if((ColaEstaVacia(&devExt->ColaAccionesComando) || soloHolds) && ColaEstaVacia(&devExt->ColaAccionesRaton) && ColaEstaVacia(&devExt->ColaAccionesHOTAS))
	{
		WdfSpinLockRelease(devExt->SpinLockAcciones);
		if(devExt->RequestEnEspera != NULL)
		{
			WdfRequestComplete(Request, STATUS_CANCELLED);
		}
		else
		{
			devExt->RequestEnEspera = Request;
			WdfRequestMarkCancelable(Request, EvtRequestCancel);
		}
		return;
	}
	else
	{
		BOOLEAN procesado = FALSE;
		do
		{
			switch(devExt->TurnoReport)
			{
			case 0:
				devExt->TurnoReport = 1;
				if(!ColaEstaVacia(&devExt->ColaAccionesHOTAS))
				{
					ProcesarHOTAS(Request);
					procesado = TRUE;
				}
				break;
			case 1:
				devExt->TurnoReport = 2;
				if(!ColaEstaVacia(&devExt->ColaAccionesRaton))
				{
					ProcesarRaton(Request);
					procesado = TRUE;
				}
				break;
			case 2:
				devExt->TurnoReport = 0;
				if(!ColaEstaVacia(&devExt->ColaAccionesComando))
				{
					if(!ProcesarComando(Request))
					{
						WdfSpinLockRelease(devExt->SpinLockAcciones);
						goto reintentar;
					}
					else
						procesado = TRUE;
				}
				break;
			}
		} while(!procesado);
		WdfSpinLockRelease(devExt->SpinLockAcciones);
	}
}

VOID EvtRequestCancel(IN WDFREQUEST  Request)
{
	PDEVICE_EXTENSION devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(Request)));
	devExt->RequestEnEspera = NULL;
	WdfRequestComplete(Request, STATUS_CANCELLED);
}

VOID EvDpc(IN WDFDPC  Dpc)
{
	NTSTATUS	status = STATUS_SUCCESS;
	WDFREQUEST	request = GetDeviceExtension(WdfDpcGetParentObject(Dpc))->RequestEnEspera;

	if(request == NULL)
		return;

	status = WdfRequestUnmarkCancelable(GetDeviceExtension(WdfDpcGetParentObject(Dpc))->RequestEnEspera);
	if(status == STATUS_CANCELLED)
		return;
	else
	{
		GetDeviceExtension(WdfDpcGetParentObject(Dpc))->RequestEnEspera = NULL;
		ProcesarReport(request);
	}
}

// *************************************************************************
// *************************************************************************

VOID ActualizarTimerRaton(PDEVICE_EXTENSION devExt)
{
	if((devExt->stRaton[1] == 0) && (devExt->stRaton[2] == 0))
		devExt->TimerRatonOn = FALSE;
	else
	{
		devExt->TimerRatonOn = TRUE;
		WdfTimerStart(devExt->TimerRaton, WDF_REL_TIMEOUT_IN_MS(devExt->TickRaton));
	}
}

VOID TimerTickRaton(IN  WDFTIMER Timer)
{
	//NTSTATUS			status	= STATUS_SUCCESS;
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfTimerGetParentObject(Timer));

	WdfSpinLockAcquire(devExt->SpinLockAcciones);
	if(ColaEstaVacia(&devExt->ColaAccionesRaton))
	{
		UCHAR accion[2];
		
		WdfSpinLockRelease(devExt->SpinLockAcciones);
		if(devExt->stRaton[1] != 0)
		{
			if(devExt->stRaton[1] < 0)
				accion[0] = 4 | 64;
			else
				accion[0] = 5 | 64;
			accion[1] = devExt->stRaton[1];
			AccionarRaton(devExt, accion);
		}
		if(devExt->stRaton[2] != 0)
		{
			if(devExt->stRaton[2] < 0)
				accion[0] = 6 | 64;
			else
				accion[0] = 7 | 64;
			accion[1] = devExt->stRaton[2];
			AccionarRaton(devExt, accion);
		}
	}
	else
		WdfSpinLockRelease(devExt->SpinLockAcciones);

	if(devExt->TimerRatonOn)
		WdfTimerStart(devExt->TimerRaton, WDF_REL_TIMEOUT_IN_MS(devExt->TickRaton));
}

// *************************************************************************
// *************************************************************************

VOID ProcesarHOTAS(IN WDFREQUEST request)
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(request)));
	NTSTATUS			status;
	PNODO				nodo = devExt->ColaAccionesHOTAS.principio;
	PVOID				buffer;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(HID_INPUT_DATA) + 1, &buffer, NULL);
	if(NT_SUCCESS(status))
	{
		PHID_INPUT_DATA pdatos = (PHID_INPUT_DATA)((PUCHAR)buffer + 1);

		*((PUCHAR)buffer) = 1;
		RtlCopyMemory((PUCHAR)buffer + 1, nodo->Datos, sizeof(HID_INPUT_DATA));
		if(!devExt->itfExt.descalibrar)
		{
			RtlCopyMemory(pdatos->Botones, devExt->stHOTAS.Botones, sizeof(pdatos->Botones)); 
			RtlCopyMemory(pdatos->Setas, devExt->stHOTAS.Setas, sizeof(pdatos->Setas));
		}

		RtlCopyMemory(&devExt->stHOTAS, pdatos, sizeof(HID_INPUT_DATA));
		ColaBorrarNodo(&devExt->ColaAccionesHOTAS, nodo);
		WdfRequestSetInformation(request, sizeof(HID_INPUT_DATA) + 1);
	}
	WdfRequestComplete(request, status);
}

VOID ProcesarRaton(IN WDFREQUEST request)
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(request)));
	NTSTATUS			status;
	PNODO				nodo = devExt->ColaAccionesRaton.principio;
	PVOID				buffer;
	BOOLEAN				soltar;
	struct {
		UCHAR tipo;
		UCHAR dato;
	} evento;

	RtlCopyMemory(&evento, (PUCHAR)nodo->Datos, sizeof(UCHAR) * 2);
	soltar = ((evento.tipo & 32) == 32) ? TRUE : FALSE;

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->stRaton) + 1, &buffer, NULL);
	if(NT_SUCCESS(status))
	{
		switch(evento.tipo & 0x1f) 
		{
		case 4: //Eje -x
			if(!soltar)
				devExt->stRaton[1] = -evento.dato;
			else
				devExt->stRaton[1] = 0; 
			break;
		case 5: //Eje x
			if(!soltar)
				devExt->stRaton[1] = evento.dato;
			else
				devExt->stRaton[1] = 0; 
			break;
		case 6: //Eje -y
			if(!soltar)
				devExt->stRaton[2] = -evento.dato;
			else
				devExt->stRaton[2] = 0; 
			break;
		case 7: //Eje y
			if(!soltar)
				devExt->stRaton[2] = evento.dato;
			else
				devExt->stRaton[2] = 0; 
			break;
		}

		*((PUCHAR)buffer) = 2;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt->stRaton, sizeof(devExt->stRaton));

		ColaBorrarNodo(&devExt->ColaAccionesRaton, nodo);
		if(!(evento.tipo >> 6))
			ActualizarTimerRaton(devExt);
		WdfRequestSetInformation(request, sizeof(devExt->stRaton) + 1);
	}
	WdfRequestComplete(request, status);
}

BOOLEAN ProcesarComando(IN WDFREQUEST request)
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(request)));
	NTSTATUS			status = STATUS_SUCCESS;
	PNODO				posAccion = devExt->ColaAccionesComando.principio;
	BOOLEAN				requestVacia = TRUE;

	while(posAccion != NULL)
	{
		PNODO psiguiente = posAccion->link;
		PNODO nodoComando = ((PCOLA)posAccion->Datos)->principio;

		while(nodoComando != NULL)
		{
#pragma region "Bucle de comandos"
			PNODO canterior = nodoComando->linkp;
			EVENTO evento;
			RtlCopyMemory(&evento, (PUCHAR)nodoComando->Datos, sizeof(UCHAR) * 2);

			if(((evento.tipo & 0x1f) > 0) && ((evento.tipo & 0x1f) < 10))
			{
				status = ProcesarEventoRaton(request, &evento);
				ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
				requestVacia = FALSE;
				psiguiente = NULL; //salir
				break;
			}
			else if(((evento.tipo & 0x1f) == 0)	||  ((evento.tipo & 0x1f) == 18) || ((evento.tipo & 0x1f) == 19))
			{
				status = ProcesarEventoTeclado_HOTAS(request, &evento);
				ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
				requestVacia = FALSE;
				psiguiente = NULL; //salir
				break;
			}
			else if((((evento.tipo & 0x1f) > 13) && ((evento.tipo & 0x1f) < 17)) || ((evento.tipo & 0x1f) >= 20))
			{
				ProcesarEventoX52_Modos(devExt, (PCOLA)posAccion->Datos, nodoComando, &evento);
				ColaBorrarNodo((PCOLA)posAccion->Datos, nodoComando);
			}
			else if((((evento.tipo & 0x1f) >= 10) && ((evento.tipo & 0x1f) <= 13)) || ((evento.tipo & 0x1f) == 17))
			{
				if(!ProcesarEventoRepeticiones_Delay(request, (PCOLA)posAccion->Datos, nodoComando, &evento))
					break;
				else
				{
					if((evento.tipo & 0x1f) == 17)
						canterior = nodoComando;
				}
			}
			
			if(canterior == NULL)
				nodoComando = ((PCOLA)posAccion->Datos)->principio;
			else
				nodoComando = canterior->link;

#pragma endregion
		}

		if(ColaEstaVacia((PCOLA)posAccion->Datos))
		{ // Fin eventos
			ColaBorrar((PCOLA)posAccion->Datos); posAccion->Datos = NULL;
			ColaBorrarNodo(&devExt->ColaAccionesComando, posAccion);
		}

		posAccion = psiguiente;
	}

	if(!requestVacia)
			WdfRequestComplete(request, STATUS_SUCCESS);

	return !requestVacia;
}

// *************************************************************************
// *************************************************************************

NTSTATUS ProcesarEventoRaton(WDFREQUEST request, PEVENTO evento)
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(request)));
	BOOLEAN soltar;
	NTSTATUS status;
	PVOID buffer;

	soltar = ((evento->tipo & 32) == 32) ? TRUE : FALSE;

	switch(evento->tipo & 0x1f)
	{
		case 1:
			if(!soltar)
				devExt->stRaton[0] |= 1;
			else
				devExt->stRaton[0] &= 254; 
			break;
		case 2:
			if(!soltar)
				devExt->stRaton[0] |= 2;
			else 
				devExt->stRaton[0] &= 253; 
			break;
		case 3:
			if(!soltar)
				devExt->stRaton[0] |= 4;
			else
				devExt->stRaton[0] &= 251; 
			break;
		case 4: //Eje -x
			if(!soltar)
				devExt->stRaton[1] = -evento->dato;
			else
				devExt->stRaton[1] = 0; 
			break;
		case 5: //Eje x
			if(!soltar)
				devExt->stRaton[1] = evento->dato;
			else
				devExt->stRaton[1] = 0; 
			break;
		case 6: //Eje -y
			if(!soltar)
				devExt->stRaton[2] = -evento->dato;
			else
				devExt->stRaton[2] = 0; 
			break;
		case 7: //Eje y
			if(!soltar)
				devExt->stRaton[2] = evento->dato;
			else
				devExt->stRaton[2] = 0; 
			break;
		case 8: // Wheel up
			if(!soltar)
				devExt->stRaton[3] = 127;
			else
				devExt->stRaton[3] = 0; 
			break;
		case 9: // Wheel down
			if(!soltar)
				devExt->stRaton[3] = (UCHAR)-127;
			else 
				devExt->stRaton[3] = 0; 
			break;
	}

	status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->stRaton) + 1, &buffer, NULL);
	if(NT_SUCCESS(status))
	{
		*((PUCHAR)buffer) = 2;
		RtlCopyMemory((PUCHAR)buffer + 1, devExt->stRaton, sizeof(devExt->stRaton));
		WdfRequestSetInformation(request, sizeof(devExt->stRaton) + 1);
	}

	ActualizarTimerRaton(devExt);

	return status;
}

NTSTATUS ProcesarEventoTeclado_HOTAS(WDFREQUEST request, PEVENTO evento)
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(request)));
	BOOLEAN soltar;
	NTSTATUS status;
	PVOID buffer;

	soltar = ((evento->tipo >> 5) == 1) ? TRUE : FALSE;
	evento->tipo &= 0x1f;

	switch(evento->tipo) {
		case 0:	//Tecla
			if(!soltar)
				devExt->stTeclado[evento->dato / 8] |= 1 << (evento->dato % 8);
			else
				devExt->stTeclado[evento->dato / 8] &= ~(1 << (evento->dato % 8));
			break;
		case 18: // Botón DX
			if(!soltar)
				devExt->stHOTAS.Botones[evento->dato/8] |= 1 << (evento->dato % 8);
			else
				devExt->stHOTAS.Botones[evento->dato/8] &= ~(1 << (evento->dato % 8));

			break;
		case 19: // Seta DX
			if(!soltar)
				devExt->stHOTAS.Setas[evento->dato / 8] = (evento->dato % 8) + 1;
			else
				devExt->stHOTAS.Setas[evento->dato / 8] = 0;
			break;
	}

	if(evento->tipo == 0)
	{
		status = WdfRequestRetrieveOutputBuffer(request, sizeof(devExt->stTeclado) + 1, &buffer, NULL);
		if(NT_SUCCESS(status))
		{
			*((PUCHAR)buffer) = 3;
			RtlCopyMemory((PUCHAR)buffer + 1, devExt->stTeclado, sizeof(devExt->stTeclado));
			WdfRequestSetInformation(request, sizeof(devExt->stTeclado) + 1);
		}
	}
	else
	{
		status = WdfRequestRetrieveOutputBuffer(request, sizeof(HID_INPUT_DATA) + 1, &buffer, NULL);
		if(NT_SUCCESS(status))
		{
			*((PUCHAR)buffer) = 1;
			RtlCopyMemory((PUCHAR)buffer + 1, &devExt->stHOTAS, sizeof(HID_INPUT_DATA));
			WdfRequestSetInformation(request, sizeof(HID_INPUT_DATA) + 1);
		}
	}

	return status;
}

VOID ProcesarEventoX52_Modos(PDEVICE_EXTENSION devExt, PCOLA cola, PNODO nodo, PEVENTO evento)
{
	evento->tipo &= 0x1f;

	switch(evento->tipo)
	{
		case 14: //Cambio modo
			devExt->itfExt.stModo = evento->dato;
			break;
		case 15: //Cambio modo Aux
			devExt->itfExt.stAux = evento->dato;
			break;
		case 16: //Cambio modo Pinkie
			devExt->itfExt.stPinkie = evento->dato;
			break;
		case 20: //mfd_luz
			{
				UCHAR params;
				params = evento->dato;
				EnviarOrdenX52(devExt, 0, &params, 1);
			}
			break;
		case 21: // luz
			{
				UCHAR params;
				params = evento->dato;
				EnviarOrdenX52(devExt, 1, &params, 1);
			}
			break;
		case 22: // info luz
			{
				UCHAR params;
				params = evento->dato;
				EnviarOrdenX52(devExt, 2, &params, 1);
			}
			break;
		case 23: // pinkie;
			{
				UCHAR params;
				params = evento->dato;
				EnviarOrdenX52(devExt, 3, &params, 1);
			}
			break;
		case 24: // texto
			{
				UCHAR texto[17];
				{
					PNODO nodos = nodo->link;
					UCHAR idx = 1;
					RtlZeroMemory(texto, 17);
					texto[0] = evento->dato;
					while(*((PUCHAR)nodos->Datos) != 56) // fin texto
					{
						texto[idx] = *((PUCHAR)nodos->Datos + 1);
						idx++;
						ColaBorrarNodo(cola, nodos);
						nodos = nodos->link;
					}
					ColaBorrarNodo(cola, nodos);
					EnviarOrdenX52(devExt, 4, texto, idx);
				} 
			}
			break;
		case 25: // hora
			{
				UCHAR params[3];
				params[0] = evento->dato;
				params[1] = *((PUCHAR)nodo->link->Datos + 1);
				ColaBorrarNodo(cola, nodo->link);
				params[2]= *((PUCHAR)nodo->link->Datos + 1);
				ColaBorrarNodo(cola, nodo->link);
				EnviarOrdenX52(devExt, 5, params, 3);
			}
			break;
		case 26: // hora 24
			{
				UCHAR params[3];
				params[0] = evento->dato;
				params[1] = *((PUCHAR)nodo->link->Datos + 1);
				ColaBorrarNodo(cola, nodo->link);
				params[2] = *((PUCHAR)nodo->link->Datos + 1);
				ColaBorrarNodo(cola, nodo->link);
				EnviarOrdenX52(devExt, 6, params, 3);
			}
			break;
		case 27: // fecha
			{
				UCHAR params[2];
				params[0] = evento->dato;
				params[1] = *((PUCHAR)nodo->link->Datos + 1);
				ColaBorrarNodo(cola, nodo->link);
				EnviarOrdenX52(devExt, 7, params, 2);
			}
			break;
	}
}

BOOLEAN ProcesarEventoRepeticiones_Delay(WDFREQUEST request,
											PCOLA cola,
											PNODO nodo,
											PEVENTO evento)
/**** return TRUE; continua en la misma cola
 **** return FALSE; salta a la siguiente accion */
{
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfIoQueueGetDevice(WdfRequestGetIoQueue(request)));

	switch(evento->tipo & 0x1f)
	{
		case 10: // Delay
			{
				WDF_TIMER_CONFIG		timerConfig;
				WDF_OBJECT_ATTRIBUTES	timerAttributes;
				PDELAY_CONTEXT			ctx;
				WDFTIMER				timerHandle;

				WDF_TIMER_CONFIG_INIT(&timerConfig, TimerDelay);
					timerConfig.AutomaticSerialization = TRUE;

				WDF_OBJECT_ATTRIBUTES_INIT(&timerAttributes);
					timerAttributes.ParentObject = WdfIoQueueGetDevice(WdfRequestGetIoQueue(request));
				WDF_OBJECT_ATTRIBUTES_SET_CONTEXT_TYPE(&timerAttributes, DELAY_CONTEXT);

				if(NT_SUCCESS(WdfTimerCreate(&timerConfig, &timerAttributes, &timerHandle)))
				{
					LONG timeout = (-10 * 1000 * 100 * evento->dato);

					ctx = WdfObjectGet_DELAY_CONTEXT(timerHandle);
					ctx->NodoFin = cola->fin;
					if(nodo->linkp != NULL) //Se supone que solo puede haber un 17 antes
					{
						ctx->NodoIni = nodo->linkp;
						ColaBorrarNodo(cola, nodo);
						cola->principio = NULL;
						cola->fin = NULL;
					}
					else
					{
						ctx->NodoIni = nodo->link;
						nodo->link = NULL;
						cola->fin = nodo;
						ColaBorrarNodo(cola, nodo);
					}

					WdfTimerStart(timerHandle, timeout);
				}

				return FALSE;
			}
			break;
		case 11: // Autorepeat hold
			if(!EstaHold(devExt, evento->dato))
				ColaBorrarNodo(cola, nodo);
			else
				return FALSE;

			break;
#pragma region "Autorepeat infinito"
		case 12:
			if(!EstaHold(devExt, evento->dato))
limpiar12:
			{
				PNODO nodos = nodo->link;
				while(*((PUCHAR)nodos->Datos) != 44) // fin autorepeat infinito
				{
					ColaBorrarNodo(cola, nodos);
					nodos = nodo->link;
				}
				ColaBorrarNodo(cola, nodos); // 44
				ColaBorrarNodo(cola, nodo); //12
			}
			else
			{
				COLA nodosNuevos;

				if(!ReservarMemoriaRepeticiones(&nodosNuevos, nodo, 44))
					goto limpiar12;
				else
				{
				//--------------------------------------------------------------------
					PNODO nodos = nodo->link;
					PNODO pos = nodo->linkp;

					nodosNuevos.fin->link = nodo;
					nodo->linkp = nodosNuevos.fin;
					if(pos == NULL)
						cola->principio = nodosNuevos.principio;
					else
						pos->link = nodosNuevos.principio;

					pos = nodosNuevos.principio->link;

					// primer nodo repetición "17"
					*((PUCHAR)nodosNuevos.principio->Datos) = 17;
					*((PUCHAR)nodosNuevos.principio->Datos + 1) = 0;

					// resto
					while(*((PUCHAR)nodos->Datos) != 44) // fin autorepeat infinito
					{
						RtlCopyMemory(pos->Datos, nodos->Datos, 2);
						pos = pos->link;
						nodos = nodos->link;
					}
				//--------------------------------------------------------------------
				}
		
				return FALSE;
			}
			break;
#pragma endregion
#pragma region "Autorepeat N"
		case 13:
			if(evento->dato == 0)
limpiar13:
			{
				ColaBorrarNodo(cola, nodo);
			}
			else
			{
				COLA nodosNuevos;

				*((PUCHAR)nodo->Datos + 1) -= 1; //Reduce la cuenta (evento.dato)

				if(!ReservarMemoriaRepeticiones(&nodosNuevos, nodo, 45))
					goto limpiar13;
				else
				{
				//--------------------------------------------------------------------
					PNODO nodos = nodo->link;
					PNODO pos = nodo->linkp;

					nodosNuevos.fin->link = nodo;
					nodo->linkp = nodosNuevos.fin;
					if(pos == NULL)
						cola->principio = nodosNuevos.principio;
					else
						pos->link = nodosNuevos.principio;

					pos = nodosNuevos.principio->link;

					// primer nodo repetición "17"
					*((PUCHAR)nodosNuevos.principio->Datos) = 17;
					*((PUCHAR)nodosNuevos.principio->Datos + 1) = 0;

					// resto
					while(*((PUCHAR)nodos->Datos) != 45) // fin autorepeat infinito
					{
						RtlCopyMemory(pos->Datos, nodos->Datos, 2);
						pos = pos->link;
						nodos = nodos->link;
					}
				//--------------------------------------------------------------------
				}

				return FALSE;
			}
			break;
#pragma endregion
		case 17: // estoy procesando repeat
			if(*((PUCHAR)(nodo->link->Datos)) == 12 || *((PUCHAR)(nodo->link->Datos)) == 13)
			{
				PNODO pos = devExt->ColaAccionesComando.principio;
				while(pos != NULL)
				{
					PNODO nodo1 = ((PCOLA)pos->Datos)->principio;
					PNODO nodo2 = nodo1->link;
					UCHAR tipo1 = ((PEVENTO)nodo1->Datos)->tipo & 0x1f;
					UCHAR tipo2 = 12;
					if(nodo2 != NULL)
						tipo2 = ((PEVENTO)nodo2->Datos)->tipo & 0x1f;

					if((tipo1 == 17) && ((tipo2 == 12) || (tipo2 == 13)))
						pos = pos->link;
					else
						return FALSE;
				}
				pos = devExt->ColaAccionesComando.principio;
				while(pos != NULL)
				{
					ColaBorrarNodo((PCOLA)pos->Datos, ((PCOLA)pos->Datos)->principio); // Borra la cabeza (17)
					pos = pos->link;
				}

				evento->tipo = 0xff; // hace que canterior funcione normal en vez hacer el saltar el 17
			}
			break;
	}

	return TRUE;
}

// *************************************************************************
// *************************************************************************

BOOLEAN ReservarMemoriaRepeticiones(PCOLA cola, PNODO nodo, UCHAR eof)
{
	UINT16* datos = NULL;

	PNODO nodos = nodo->link;
	UCHAR idx = 0;

	cola->principio = NULL;
	cola->fin = NULL;

	datos = (UINT16*)ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'npHV');
	if(datos == NULL)
		return FALSE;
	else
	{
		if(!ColaPush(cola, datos))
		{
			ExFreePoolWithTag((PVOID)datos, (ULONG)'npHV'); datos = NULL;
			return FALSE;
		}
		else
		{
			while(*((PUCHAR)nodos->Datos) != eof) // fin autorepeat infinito
			{
				datos = (UINT16*)ExAllocatePoolWithTag(NonPagedPool, sizeof(UCHAR) * 2, (ULONG)'npHV');
				if(datos == NULL)
				{
					while(!ColaBorrarNodo(cola, cola->principio));
					return FALSE;
				}
				else
				{
					if(!ColaPush(cola, datos))
					{
						ExFreePoolWithTag((PVOID)datos, (ULONG)'npHV'); datos = NULL;
						while(!ColaBorrarNodo(cola, cola->principio));
						return FALSE;;
					}
				}

				nodos = nodos->link;
				idx++;
			}
			if(idx == 0)
			{
				while(!ColaBorrarNodo(cola, cola->principio));
				return FALSE;
			}
		}
	}

	return TRUE;
}

BOOLEAN EstaHold(PDEVICE_EXTENSION devExt, UCHAR boton)
{
	if(boton < 100)
		return ( ((devExt->DeltaHidData.Botones[boton/8] >> (boton % 8)) & 1) == 1);
	else 
	{ //Seta
		boton -= 100;
		return (devExt->DeltaHidData.Setas[boton / 8] == (boton % 8 ));
	}
}

VOID TimerDelay(IN  WDFTIMER Timer)
{
	PDELAY_CONTEXT		ctx = WdfObjectGet_DELAY_CONTEXT(Timer);
	PDEVICE_EXTENSION	devExt = GetDeviceExtension(WdfTimerGetParentObject(Timer));
	PCOLA				eventos;

	eventos = ColaCrear();
	if(eventos != NULL)
	{
		eventos->principio = ctx->NodoIni;
		eventos->fin = ctx->NodoFin;
		WdfSpinLockAcquire(devExt->SpinLockAcciones);

			if(!ColaPush(&devExt->ColaAccionesComando, eventos))
			{
				ColaBorrar(eventos); eventos = NULL;
			}
			else
				WdfDpcEnqueue(devExt->DpcRequest);

		WdfSpinLockRelease(devExt->SpinLockAcciones);
	}
	WdfObjectDelete(Timer);
}
