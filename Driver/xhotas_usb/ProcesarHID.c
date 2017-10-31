#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#include "CalibradoHID.h"
#include "botones_setas.h"
#include "ejes_raton.h"
#include "directx.h"
#define _PRIVATE_
#include "ProcesarHID.h"
#undef _PRIVATE_

//#ifdef ALLOC_PRAGMA
//#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoInit)
//#endif

VOID PreProcesarModos(WDFDEVICE device, _In_ PUCHAR entrada)
{ //Obligar x52 asegurar que primero se suelta el modo y luego se pulsa (se usa para porder usarlos como botones normales)
	UCHAR				nuevoEstado = (((entrada[11] & 3) << 1) | (entrada[10] >> 7));
	UCHAR				antiguoEstado;
	HID_INPUT_DATA		viejohidData;

	WdfSpinLockAcquire(GetDeviceContext(device)->HID.SpinLockDeltaHid);
	{
		RtlCopyMemory(&viejohidData, &GetDeviceContext(device)->HID.DeltaHidData, sizeof(HID_INPUT_DATA));
	}
	WdfSpinLockRelease(GetDeviceContext(device)->HID.SpinLockDeltaHid);
	antiguoEstado = (viejohidData.Botones[1] & 0x7);

	if (antiguoEstado != nuevoEstado)
	{
		viejohidData.Botones[1] &= 0xf8;
		ProcesarHID(device, &viejohidData);
	}
}

//DISPATCH
UCHAR Switch4To8(UCHAR in)
{
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

VOID ConvertirEjesA2048(PUCHAR ejes)
{
	UCHAR i;

	for (i = 0; i < 16; i += 2)
	{
		UINT16 pos = *((PUINT16)&ejes[i]);
		if (i < 4)
		{
			if (pos == 2047) pos = 2048;
		}
		else if ((i == 4) || (i == 14)) //R
		{
			if (pos == 1023) pos = 1024;
			pos *= 2;
		}
		else //Z Rx, Ry Sl1 Sl2
		{
			pos += 1;
			if (pos == 1) pos = 0;
			pos *= 8;
		}
		RtlCopyMemory(&ejes[i], &pos, 2);
	}
}

//DISPATCH
VOID ProcesarInputX52(WDFDEVICE device, _In_ PVOID inputData, BOOLEAN repetirUltimo)
{
		if (repetirUltimo)
		{
			WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
			{
				RtlCopyMemory(inputData, &GetDeviceContext(device)->EntradaX52.UltimaPosicion, sizeof(HIDX52_INPUT_DATA));
			}
			WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
		}
		else
		{
			PreProcesarModos(device, inputData);
			WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
			{
				RtlCopyMemory(&GetDeviceContext(device)->EntradaX52.UltimaPosicion, inputData, sizeof(HIDX52_INPUT_DATA));
			}
			WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
		}


	ProcesarX52(device, inputData);
}

//DISPATCH
VOID ProcesarX52(WDFDEVICE device, _In_ PVOID inputData)
{
	HID_INPUT_DATA hidData;
	RtlZeroMemory(&hidData, sizeof(HID_INPUT_DATA));

	PHIDX52_INPUT_DATA hidGameData = (PHIDX52_INPUT_DATA)inputData;
	hidData.Ejes[0] = hidGameData->EjesXYR[0];
	hidData.Ejes[1] = hidGameData->EjesXYR[1] & 0x7;
	hidData.Ejes[2] = (hidGameData->EjesXYR[1] >> 3) | ((hidGameData->EjesXYR[2] & 0x7) << 5);
	hidData.Ejes[3] = (hidGameData->EjesXYR[2] >> 3) & 0x7;
	hidData.Ejes[4] = (hidGameData->EjesXYR[2] >> 6) | ((hidGameData->EjesXYR[3] & 0x3f) << 2);
	hidData.Ejes[5] = hidGameData->EjesXYR[3] >> 6;
	hidData.Ejes[6] = 255 - hidGameData->Ejes[0]; //Z
	hidData.Ejes[8] = hidGameData->Ejes[2];
	hidData.Ejes[10] = hidGameData->Ejes[1];
	hidData.Ejes[12] = hidGameData->Ejes[3];
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

	// Menu MFD
	if ((hidData.Botones[1] & 0x8) == 0x8)
	{
		if (!GetDeviceContext(device)->HID.MenuTimerEsperando && !GetDeviceContext(device)->HID.MenuActivado)
		{
			GetDeviceContext(device)->HID.MenuTimerEsperando = TRUE;
			WdfTimerStart(GetDeviceContext(device)->HID.MenuTimer, WDF_REL_TIMEOUT_IN_SEC(3));
		}
	}
	else
	{
		if (GetDeviceContext(device)->HID.MenuTimerEsperando && !GetDeviceContext(device)->HID.MenuActivado)
		{
			GetDeviceContext(device)->HID.MenuTimerEsperando = FALSE;
			WdfTimerStop(GetDeviceContext(device)->HID.MenuTimer, FALSE);
		}
	}

	if (GetDeviceContext(device)->Pedales.Activado)
	{
		INT16 posPedales;
		WdfSpinLockAcquire(GetDeviceContext(device)->Pedales.SpinLockPosicion);
		{
			posPedales = GetDeviceContext(device)->Pedales.UltimaPosicion;
		}
		WdfSpinLockRelease(GetDeviceContext(device)->Pedales.SpinLockPosicion);
		hidData.Ejes[14] = hidData.Ejes[4];
		hidData.Ejes[15] = hidData.Ejes[5];
		hidData.Ejes[4] = posPedales & 0xff;
		hidData.Ejes[5] = posPedales >> 8;
	}

	ConvertirEjesA2048(hidData.Ejes);

	ProcesarHID(device, &hidData);
}

//DISPATCH
VOID ProcesarHID(WDFDEVICE device, _In_ PHID_INPUT_DATA hidData)
{
	HID_CONTEXT* devExt = &GetDeviceContext(device)->HID;
	UCHAR idx;
	UCHAR cambios;

	HID_INPUT_DATA viejohidData;
	HID_INPUT_DATA outputData;

	WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
	{
		RtlCopyMemory(&viejohidData, &devExt->DeltaHidData, sizeof(HID_INPUT_DATA));
	}
	WdfSpinLockRelease(devExt->SpinLockDeltaHid);

	// Calibrar
	if (!devExt->ModoRaw)
		Calibrar(device, hidData);

	WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
	{
		RtlCopyMemory(&devExt->DeltaHidData, hidData, sizeof(HID_INPUT_DATA));
	}
	WdfSpinLockRelease(devExt->SpinLockDeltaHid);

	if (!devExt->ModoRaw)
	{
		if (!devExt->MenuActivado)
		{
			//Botones

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
								GenerarPulsarBoton(device, (idx * 8) + exp);
							else
								GenerarSoltarBoton(device, (idx * 8) + exp);
						}
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
					GenerarSoltarSeta(device, (idx * 8) + viejohidData.Setas[idx] - 1);
				if (hidData->Setas[idx] != 0)
					GenerarPulsarSeta(device, (idx * 8) + hidData->Setas[idx] - 1);
			}
		}

		// Ejes

		for (idx = 0; idx < 8; idx++)
		{
			if (*((USHORT*)&hidData->Ejes[idx * 2]) != *((USHORT*)&viejohidData.Ejes[idx * 2]))
				GenerarAccionesEjes(device, idx, *((USHORT*)&hidData->Ejes[idx * 2]));
		}

		// Sensibilidad y mapeado
		RtlZeroMemory(&outputData, sizeof(HID_INPUT_DATA));
		SensibilidadYMapeado(device, &viejohidData, hidData, &outputData);

		if (devExt->MenuActivado)
			RtlCopyMemory(outputData.Botones, hidData->Botones, sizeof(hidData->Botones));

		RtlCopyMemory(hidData, &outputData, sizeof(HID_INPUT_DATA));
	}

	GenerarDirectX(device, hidData);
}