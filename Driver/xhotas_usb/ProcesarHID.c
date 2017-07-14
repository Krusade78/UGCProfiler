#define INITGUID

#include <ntddk.h>
#include <wdf.h>
#include "Context.h"
#include "CalibradoHID.h"
#include "botones_setas.h"
#include "ejes_raton.h"
#define _PRIVATE_
#include "ProcesarHID.h"
#undef _PRIVATE_

//#ifdef ALLOC_PRAGMA
//#pragma alloc_text (PAGE, EvtDeviceSelfManagedIoInit)
//#endif

VOID PreProcesarModos(WDFDEVICE device, _Inout_ PUCHAR entrada)
{ //x52 asegurar que primero se suelta el modo y luego se pulsa
	BOOLEAN cambio = FALSE;
	UCHAR buf[0x0e];
	RtlCopyMemory(buf, entrada, 0x0e);
	if (GetDeviceContext(device)->HID.EstadoModos != (((entrada[11] & 3) << 1) | (entrada[10] >> 7)))
	{
		buf[10] &= 0x7f; buf[11] &= 0xfc;
		cambio = TRUE;
		GetDeviceContext(device)->HID.EstadoModos = (((entrada[11] & 3) << 1) | (entrada[10] >> 7));
	}
	if (cambio)
		RtlCopyMemory(entrada, buf, 0x0e);
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

//DISPATCH
VOID ProcesarInputX52(WDFDEVICE device, PVOID inputData)
{
	HID_INPUT_DATA hidData;
	RtlZeroMemory(&hidData, sizeof(HID_INPUT_DATA));

	PreProcesarModos(device, (PUCHAR)inputData);

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

	if ((hidData.Botones[2] & 0x1)) //Combinación para des/activar los pedales
		GetDeviceContext(device)->Pedales.Activado = !GetDeviceContext(device)->Pedales.Activado;

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
		hidData.Ejes[4] = posPedales >> 8;
		hidData.Ejes[5] = posPedales & 0xff;
	}

	ProcesarHID(device, &hidData);

	RtlCopyMemory((PVOID)((PUCHAR)inputData + 1), &hidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockAcquire(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
	{
		RtlCopyMemory(&GetDeviceContext(device)->EntradaX52.UltimaPosicion, &hidData, sizeof(HID_INPUT_DATA));
	}
	WdfSpinLockRelease(GetDeviceContext(device)->EntradaX52.SpinLockPosicion);
}

//DISPATCH
VOID ProcesarHID(WDFDEVICE device, _Inout_ PHID_INPUT_DATA hidData)
{
	HID_CONTEXT devExt = GetDeviceContext(device)->HID;
	UCHAR idx;
	UCHAR cambios;

	HID_INPUT_DATA viejohidData;
	HID_INPUT_DATA outputData;

	if (RtlCompareMemory(&devExt.DeltaHidData, &hidData, sizeof(HID_INPUT_DATA)) == sizeof(HID_INPUT_DATA))
		return;

	WdfSpinLockAcquire(devExt.SpinLockDeltaHid);
	RtlCopyMemory(&viejohidData, &devExt.DeltaHidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockRelease(devExt.SpinLockDeltaHid);

	// Calibrar
	if (!devExt.ModoRaw)
		Calibrar(device, hidData);

	WdfSpinLockAcquire(devExt.SpinLockDeltaHid);
		RtlCopyMemory(&devExt.DeltaHidData, &hidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockRelease(devExt.SpinLockDeltaHid);

	if (!devExt.ModoRaw)
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

		for (idx = 0; idx < 7; idx++)
		{
			if (*((USHORT*)&hidData->Ejes[idx * 2]) != *((USHORT*)&viejohidData.Ejes[idx * 2]))
				GenerarAccionesEjes(device, idx, *((USHORT*)&hidData->Ejes[idx * 2]));
		}

		// Sensibilidad y mapeado
		RtlZeroMemory(&outputData, sizeof(HID_INPUT_DATA));
		SensibilidadYMapeado(device, &viejohidData, hidData, &outputData);

		//Frenos y R movido
		outputData.Ejes[14] = hidData->Ejes[14];
		outputData.Ejes[15] = hidData->Ejes[15];
		outputData.Ejes[16] = hidData->Ejes[16];
		outputData.Ejes[17] = hidData->Ejes[17];

		RtlCopyMemory(hidData->Botones, devExt.stDirectX.Botones, sizeof(hidData->Botones));
		RtlCopyMemory(hidData->Setas, devExt.stDirectX.Setas, sizeof(hidData->Setas));
		RtlCopyMemory(hidData, &outputData, sizeof(HID_INPUT_DATA));
	}
}