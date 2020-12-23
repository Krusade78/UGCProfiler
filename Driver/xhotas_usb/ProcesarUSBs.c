/*++
Copyright (c) 2020 Alfredo Costalago

Module Name:

ProcesarUSBs.c

Abstract:

Convertir datos originales del USB a HID modficado y después a eventos.

IRQL:

Todas la funciones PASSIVE_LEVEL

--*/
#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#define _PUBLIC_
#include "ProcesarUSBs_Calibrado.h"
#include "ProcesarUSBs_Botones-Setas.h"
#include "ProcesarUSBs_Ejes.h"
#include "EventosGenerar.h"
#define _PRIVATE_
#include "ProcesarUSBs.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, ProcesarEntradaUSB)
#pragma alloc_text (PAGE, PreProcesarModos)
#pragma alloc_text (PAGE, Switch4To8)
#pragma alloc_text (PAGE, ConvertirEjesA2048)
#pragma alloc_text (PAGE, ConvertirDatosUSBaHID)
#pragma alloc_text (PAGE, ProcesarHID)
#endif

VOID ProcesarEntradaUSB(WDFDEVICE device, PHIDX52_INPUT_DATA inputData, BOOLEAN pedales)
{
	PAGED_CODE();

	WdfWaitLockAcquire(GetDeviceContext(device)->USBaHID.WaitLockProcesar, NULL);
	{
		HIDX52_INPUT_DATA hidPedales;
		if (pedales)
		{
			inputData = &hidPedales;
			RtlCopyMemory(inputData, &GetDeviceContext(device)->USBaHID.UltimaPosicion, sizeof(HIDX52_INPUT_DATA));
		}
		else
		{
			PreProcesarModos(device, inputData);
			RtlCopyMemory(&GetDeviceContext(device)->USBaHID.UltimaPosicion, inputData, sizeof(HIDX52_INPUT_DATA));
		}

		ConvertirDatosUSBaHID(device, inputData);
	}
	WdfWaitLockRelease(GetDeviceContext(device)->USBaHID.WaitLockProcesar);
}

VOID PreProcesarModos(WDFDEVICE device, _In_ PHIDX52_INPUT_DATA entrada)
{ //Obligar x52 asegurar que primero se suelta el modo y luego se pulsa (se usa para porder usarlos como botones normales)
	PAGED_CODE();

	UCHAR				nuevoEstado = (((entrada->Botones[3] & 3) << 1) | (entrada->Botones[2] >> 7));
	UCHAR				antiguoEstado;
	HID_INPUT_DATA		viejohidData;

	//aqui no hace falta proteger con WaitLock
	RtlCopyMemory(&viejohidData, &GetDeviceContext(device)->USBaHID.UltimoEstado.DeltaHidData, sizeof(HID_INPUT_DATA));
	antiguoEstado = (viejohidData.Botones[1] & 0x7);

	if (antiguoEstado != nuevoEstado)
	{
		viejohidData.Botones[1] &= 0xf8;
		ProcesarHID(device, &viejohidData);
	}
}

UCHAR Switch4To8(UCHAR in)
{
	PAGED_CODE();

	switch (in)
	{
	case 0: return 0;
	case 1: return 1;
	case 2: return 3;
	case 3: return 2;
	case 4: return 5;
	case 6: return 4;
	case 8: return 7;
	case 9: return 8;
	case 12: return 6;
	default: return 0;
	}
}

VOID ConvertirEjesA2048(PHID_INPUT_DATA hid, BOOLEAN pedales)
{
	PAGED_CODE();

	UCHAR i;

	for (i = 0; i < 9; i ++)
	{
		if (i < 2)
		{
			if (hid->Ejes[i] == 2047) hid->Ejes[i] = 2048;
		}
		else if (i == 2) //R
		{
			if (pedales)
			{
				if (hid->Ejes[i] == 511) hid->Ejes[i] = 512;
				hid->Ejes[i] *= 4;
			}
			else
			{
				if (hid->Ejes[i] == 1023) hid->Ejes[i] = 1024;
				hid->Ejes[i] *= 2;
			}
		}
		else if (i < 7)//Z, Rx, Ry Sl1
		{
			hid->Ejes[i] += 1;
			if (hid->Ejes[i] == 1) hid->Ejes[i] = 0;
			hid->Ejes[i] *= 8;
		}
		else
		{
			hid->Ejes[i] += 1;
			if (hid->Ejes[i] == 1) hid->Ejes[i] = 0;
			hid->Ejes[i] *= 16;

		}
	}
}

VOID ConvertirDatosUSBaHID(WDFDEVICE device, _In_ PHIDX52_INPUT_DATA hidGameData)
{
	PAGED_CODE();

	HID_INPUT_DATA hidData;
	RtlZeroMemory(&hidData, sizeof(HID_INPUT_DATA));

	((PUCHAR)hidData.Ejes)[0] = hidGameData->EjesXYR[0];
	((PUCHAR)hidData.Ejes)[1] = hidGameData->EjesXYR[1] & 0x7;
	((PUCHAR)hidData.Ejes)[2] = (hidGameData->EjesXYR[1] >> 3) | ((hidGameData->EjesXYR[2] & 0x7) << 5);
	((PUCHAR)hidData.Ejes)[3] = (hidGameData->EjesXYR[2] >> 3) & 0x7;
	((PUCHAR)hidData.Ejes)[4] = (hidGameData->EjesXYR[2] >> 6) | ((hidGameData->EjesXYR[3] & 0x3f) << 2);
	((PUCHAR)hidData.Ejes)[5] = hidGameData->EjesXYR[3] >> 6;
	((PUCHAR)hidData.Ejes)[6] = hidGameData->Ejes[0]; //Z
	hidData.Ejes[4] = hidGameData->Ejes[2];
	hidData.Ejes[5] = hidGameData->Ejes[1];
	hidData.Ejes[6] = hidGameData->Ejes[3];
	hidData.Botones[0] = ((hidGameData->Botones[1] >> 6) & 1) | ((hidGameData->Botones[0] >> 1) & 6) | ((hidGameData->Botones[0] << 2) & 8) | ((hidGameData->Botones[0] >> 2) & 16) | ((hidGameData->Botones[3] >> 1) & 32) | ((hidGameData->Botones[0] << 1) & 64) | ((hidGameData->Botones[0] << 3) & 128);
	hidData.Botones[1] = ((hidGameData->Botones[2] & 0x80) >> 7) | ((hidGameData->Botones[3] << 1) & 126) | (hidGameData->Botones[0] & 128);
	hidData.Botones[2] = (hidGameData->Botones[1] & 0x3f) | ((hidGameData->Botones[0] & 1) << 6) | (hidGameData->Botones[3] & 0x80);
	hidData.Botones[3] = hidGameData->Seta & 0x3;
	hidData.Setas[0] = hidGameData->Seta >> 4;
	hidData.Setas[1] = Switch4To8((hidGameData->Botones[1] >> 7) + ((hidGameData->Botones[2] << 1) & 0xf));
	hidData.Setas[2] = Switch4To8((hidGameData->Botones[2] >> 3) & 0xf);
	switch (hidGameData->Ministick & 0xf)
	{
	case 0:
		hidData.Setas[3] = 8;
		break;
	case 0xf:
		hidData.Setas[3] = 2;
		break;
	default: hidData.Setas[3] = 0;
	}
	switch (hidGameData->Ministick >> 4)
	{
	case 0:
		hidData.Setas[3] |= 1;
		break;
	case 0xf:
		hidData.Setas[3] |= 4;
		break;
	}
	hidData.Setas[3] = Switch4To8(hidData.Setas[3]);
	hidData.MiniStick = hidGameData->Ministick;

	if (GetDeviceContext(device)->Pedales.Activado)
	{
		INT16 posRz = GetDeviceContext(device)->Pedales.UltimaPosicionRz;

		hidData.Ejes[7] = GetDeviceContext(device)->Pedales.UltimaPosicionFrenoI;
		hidData.Ejes[8] = GetDeviceContext(device)->Pedales.UltimaPosicionFrenoD;
		((PUCHAR)hidData.Ejes)[4] = posRz & 0xff;
		((PUCHAR)hidData.Ejes)[5] = posRz >> 8;
	}

	ConvertirEjesA2048(&hidData, GetDeviceContext(device)->Pedales.Activado);

	ProcesarHID(device, &hidData);
}

VOID ProcesarHID(WDFDEVICE device, _In_ PHID_INPUT_DATA hidData)
{
	PAGED_CODE();

	USB_HIDX52_CONTEXT* devExt = &GetDeviceContext(device)->USBaHID;
	HID_INPUT_DATA viejohidData;

	//aqui no hace falta proteger con WaitLock
	RtlCopyMemory(&viejohidData, &devExt->UltimoEstado.DeltaHidData, sizeof(HID_INPUT_DATA));

	// Calibrar
	if (!devExt->ModoRaw)
	{
		Calibrar(device, hidData);
	}

	WdfWaitLockAcquire(devExt->UltimoEstado.WaitLockUltimoEstado, NULL);
	RtlCopyMemory(&devExt->UltimoEstado.DeltaHidData, hidData, sizeof(HID_INPUT_DATA));
	WdfWaitLockRelease(devExt->UltimoEstado.WaitLockUltimoEstado);

	if (devExt->ModoRaw)
	{
		//	Botones menu
		UCHAR cambios = hidData->Botones[1] ^ viejohidData.Botones[1];
		if (cambios != 0)
		{
			UCHAR exp;
			for (exp = 3; exp < 6; exp++)
			{
				if ((cambios >> exp) & 1)
				{ // Si ha cambiado
					if ((hidData->Botones[1] >> exp) & 1)
						PulsarBoton(device, 8 + exp);
					else
						SoltarBoton(device, 8 + exp);
				}
			}
		}
	}
	else
	{
		UCHAR idx;
		UCHAR cambios;
		HID_INPUT_DATA outputData;

		//	Botones

		for (idx = 0; idx < 4; idx++) {
			cambios = hidData->Botones[idx] ^ viejohidData.Botones[idx];
			if (cambios != 0)
			{
				UCHAR exp;
				for (exp = 0; exp < 8; exp++)
				{
					if ((cambios >> exp) & 1)
					{ // Si ha cambiado
						if ((hidData->Botones[idx] >> exp) & 1)
							PulsarBoton(device, (idx * 8) + exp);
						else
							SoltarBoton(device, (idx * 8) + exp);
					}
				}
			}
		}

		// Setas

		for (idx = 0; idx < 4; idx++)
		{
			if (hidData->Setas[idx] != viejohidData.Setas[idx])
			{
				if (viejohidData.Setas[idx] != 0)
					SoltarSeta(device, (idx * 8) + viejohidData.Setas[idx] - 1);
				if (hidData->Setas[idx] != 0)
					PulsarSeta(device, (idx * 8) + hidData->Setas[idx] - 1);
			}
		}

		// Ejes

		for (idx = 0; idx < 9; idx++)
		{
			if (hidData->Ejes[idx] != viejohidData.Ejes[idx])
				MoverEje(device, idx, hidData->Ejes[idx]);
		}

		// Sensibilidad y mapeado
		RtlZeroMemory(&outputData, sizeof(HID_INPUT_DATA));
		SensibilidadYMapeado(device, &viejohidData, hidData, &outputData);

		RtlCopyMemory(hidData, &outputData, sizeof(HID_INPUT_DATA));
	}

	GenerarEventoDirectX(device, hidData);
}