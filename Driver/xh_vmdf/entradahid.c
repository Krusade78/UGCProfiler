#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "ejes.h"
#include "botones.h"

#define _DIRECTX_
#include "entradahid.h"
#undef _DIRECTX_

void ProcesarEntradaPedales(IN PDEVICE_EXTENSION devExt, IN PVOID buffer)
{
	UCHAR			izq = 0xff - ((PUCHAR)buffer)[6];
	UCHAR			der = 0xff - ((PUCHAR)buffer)[5];
	UCHAR			freno = 0xff - ((PUCHAR)buffer)[4];
	UINT16			eje = 0xff;
	HID_INPUT_DATA	nuevoHID;

	if((izq < 80) && (der < 80))
	{
		devExt->PedalSel = 0;
	}
	switch(devExt->PedalSel)
	{
		case 1:
			eje = 0xff - izq;
			break;
		case 2:
			eje = 0x0100 + der;
			break;
		default:
			if(izq > der) devExt->PedalSel = 1;
			else if(der >izq) devExt->PedalSel = 2;
	}

	if(devExt->PedalesActivados)
	{
		WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
			RtlCopyMemory(&nuevoHID, &devExt->DeltaHidData, sizeof(HID_INPUT_DATA));
		WdfSpinLockRelease(devExt->SpinLockDeltaHid);
		if((izq < 2) && (der < 2)) // zona nula
			eje = 512;
		else if ((izq < 80) && (der < 80)) // zona con los dos pedales
			eje = (der >= izq ) ? (0x100 + der - izq) * 2 : (0xff - izq + der) * 2;
		else // sólo un pedal
			eje *= 2;
		nuevoHID.Ejes[5] = eje >> 8;
		nuevoHID.Ejes[4] = (UCHAR)(eje & 0xff);
		nuevoHID.Ejes[14] = freno;
		ProcesarHID(devExt, &nuevoHID);
	}
}

void ProcesarEntradaHOTAS(IN PDEVICE_EXTENSION devExt, IN PVOID buffer, BOOLEAN x52)
{
	PUCHAR entrada = ((PUCHAR)buffer) + 1;
	if(!x52)
	{ //X36,X45 requieren manipular los modos a 3 botones
		BOOLEAN cambio = FALSE;
		UCHAR buf[0x0b];
		RtlCopyMemory(buf, entrada, 0x0b);
		if(devExt->stModos != (entrada[8] & 0x7)) { buf[8] &= 0x7c; cambio = TRUE; devExt->stModos = entrada[8] & 0x7;}
		if(devExt->stAux != ((entrada[8] >> 3) & 0x7)) { buf[8] &= 0xc7; cambio = TRUE; devExt->stAux = (entrada[8] >> 3) & 0x7;}
		if(cambio)
			PreProcesarHID(devExt, buf, FALSE);
				
		PreProcesarHID(devExt, entrada, FALSE);
	} 
	else
	{ //x52 asegurar que primero se suelta el modo y luego se pulsa
		BOOLEAN cambio = FALSE;
		UCHAR buf[0x0e];
		RtlCopyMemory(buf, entrada, 0x0e);
		if(devExt->stModos != (((entrada[11] & 3) << 1) | (entrada[10] >> 7)))
		{
			buf[10] &= 0x7f; buf[11] &= 0xfc;
			cambio = TRUE;
			devExt->stModos= (((entrada[11] & 3) << 1) | (entrada[10] >> 7));
		}
		if(cambio)
			PreProcesarHID(devExt, buf, TRUE);
		
		PreProcesarHID(devExt, entrada, TRUE);
	}
}

UCHAR Switch4To8(UCHAR in)
{
	//PAGED_CODE();

	switch(in)
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
VOID PreProcesarHID(PDEVICE_EXTENSION devExt, PVOID inputData, BOOLEAN esX52)
{
	HID_INPUT_DATA hidData;
	RtlZeroMemory(&hidData, sizeof(HID_INPUT_DATA));

	// Traducir a formato común
	if(!esX52) {
		HIDX36_INPUT_DATA hidGameData;
		RtlCopyMemory(&hidGameData, inputData, sizeof(HIDX36_INPUT_DATA));
		hidData.Ejes[0] = hidGameData.EjesXY[0];
		hidData.Ejes[1] = hidGameData.EjesXY[1] & 0x0f;
		hidData.Ejes[2] = (hidGameData.EjesXY[1] >> 4)|((hidGameData.EjesXY[2] & 0x0f) <<4 );
		hidData.Ejes[3] = hidGameData.EjesXY[2] >> 4;
		hidData.Ejes[4] = hidGameData.Ejes[0];
		hidData.Ejes[6] = hidGameData.Ejes[1];
		hidData.Ejes[8] = hidGameData.Ejes[2];
		hidData.Ejes[10] = hidGameData.Ejes[3];
		hidData.Botones[0] = hidGameData.Botones[0];
		hidData.Botones[1] = hidGameData.Botones[1] & 0x3f;
		hidData.Setas[0] = hidGameData.Setas[1] >> 4;
		hidData.Setas[1] = Switch4To8( (hidGameData.Botones[1] >> 6)|((hidGameData.Setas[0] & 0x3) << 2) );
		hidData.Setas[2] = Switch4To8( (hidGameData.Setas[0] & 0x3c) >> 2 );
		hidData.Setas[3] = Switch4To8( (hidGameData.Setas[0] >> 6)|((hidGameData.Setas[1] & 0x3) << 2) );
		switch(hidData.Setas[3])
		{
			case 0:
				hidData.MiniStick = 0x88; break;
			case 1:
				hidData.MiniStick = 0x08; break;
			case 2:
				hidData.MiniStick = 0x0f; break;
			case 3:
				hidData.MiniStick = 0x8f; break;
			case 4:
				hidData.MiniStick = 0xff; break;
			case 5:
				hidData.MiniStick = 0xf8; break;
			case 6:
				hidData.MiniStick = 0xf0; break;
			case 7:
				hidData.MiniStick = 0x80; break;
			case 8:
				hidData.MiniStick = 0; break;
		}
	}
	else
	{
		HIDX52_INPUT_DATA hidGameData;
		RtlCopyMemory(&hidGameData, inputData, sizeof(HIDX52_INPUT_DATA));
		hidData.Ejes[0] = hidGameData.EjesXYR[0];
		hidData.Ejes[1] = hidGameData.EjesXYR[1] & 0x7;
		hidData.Ejes[2] = (hidGameData.EjesXYR[1] >> 3) | ((hidGameData.EjesXYR[2] & 0x7) << 5);
		hidData.Ejes[3] = (hidGameData.EjesXYR[2] >> 3) & 0x7;
		hidData.Ejes[4] = (hidGameData.EjesXYR[2] >> 6) | ((hidGameData.EjesXYR[3] & 0x3f) << 2);
		hidData.Ejes[5] = hidGameData.EjesXYR[3] >> 6;
		hidData.Ejes[6] = hidGameData.Ejes[0]; //Z
		hidData.Ejes[8] = hidGameData.Ejes[2];
		hidData.Ejes[10] = hidGameData.Ejes[1];
		hidData.Ejes[12] = hidGameData.Ejes[3];
		hidData.Botones[0] = ((hidGameData.Botones[1] >> 6) & 1) | ((hidGameData.Botones[0] >> 1) & 6) | ((hidGameData.Botones[0] << 2) & 8) | ((hidGameData.Botones[0] >> 2) & 16) | ((hidGameData.Botones[3] >> 1) & 32) | ((hidGameData.Botones[0] << 1) & 64) | ((hidGameData.Botones[0] << 3) & 128);
		hidData.Botones[1] = ((hidGameData.Botones[2] & 0x80) >> 7) | ((hidGameData.Botones[3] << 1) & 126) | (hidGameData.Botones[0] & 128);
		hidData.Botones[2] = (hidGameData.Botones[1] & 0x3f) | ((hidGameData.Botones[0] & 1) << 6) | (hidGameData.Botones[3] & 0x80);
		hidData.Botones[3] = hidGameData.Seta & 0x3;
		hidData.Setas[0] = hidGameData.Seta >> 4;
		hidData.Setas[1] = Switch4To8( (hidGameData.Botones[1] >> 7) + ((hidGameData.Botones[2] << 1) & 0xf) );
		hidData.Setas[2] = Switch4To8( (hidGameData.Botones[2] >> 3) & 0xf );
		switch(hidGameData.Ministick & 0xf)
		{
			case 0:
				hidData.Setas[3] = 8;
				break;
			case 0xf:
				hidData.Setas[3] = 2;
				break;
			default: hidData.Setas[3] = 0;
		}
		switch(hidGameData.Ministick >> 4)
		{
			case 0:
				hidData.Setas[3] |= 1;
				break;
			case 0xf:
				hidData.Setas[3] |= 4;
				break;
		}
		hidData.Setas[3]=Switch4To8( hidData.Setas[3] );
		hidData.MiniStick=hidGameData.Ministick;
	}

	if(devExt->PedalesActivados)
	{
		HID_INPUT_DATA ultimoHID;
		WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
			RtlCopyMemory(&ultimoHID, &devExt->DeltaHidData, sizeof(HID_INPUT_DATA));
		WdfSpinLockRelease(devExt->SpinLockDeltaHid);
		hidData.Ejes[16] = hidData.Ejes[4];
		hidData.Ejes[17] = hidData.Ejes[5];
		hidData.Ejes[14] = ultimoHID.Ejes[14];
		hidData.Ejes[15] = ultimoHID.Ejes[15];
		hidData.Ejes[4] = ultimoHID.Ejes[4];
		hidData.Ejes[5] = ultimoHID.Ejes[5];
	}
	ProcesarHID(devExt, &hidData);
}

VOID 
ProcesarHID(   
    PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA hidData
    )
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	UCHAR idx;
	UCHAR cambios;

	HID_INPUT_DATA viejohidData;
	HID_INPUT_DATA outputData;

	// Calibrar
	WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
		RtlCopyMemory(&viejohidData, &devExt->DeltaHidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockRelease(devExt->SpinLockDeltaHid);

	if(RtlCompareMemory(&viejohidData, hidData, sizeof(HID_INPUT_DATA)) == sizeof(HID_INPUT_DATA))
		return;

	if(!idevExt->descalibrar)
		Calibrado(idevExt, hidData);

	WdfSpinLockAcquire(devExt->SpinLockDeltaHid);
		RtlCopyMemory(&devExt->DeltaHidData, hidData, sizeof(HID_INPUT_DATA));
	WdfSpinLockRelease(devExt->SpinLockDeltaHid);

	if(!idevExt->descalibrar) {
		// Botones

		for(idx = 0; idx < 4; idx++) {
			cambios = hidData->Botones[idx] ^ viejohidData.Botones[idx];
			if(cambios != 0)
			{
				UCHAR exp;
				for(exp = 0; exp < 8; exp++)
				{
					if((cambios >> exp) & 1)
					{ // Si ha cambiado
						if((hidData->Botones[idx] >> exp) & 1)
							GenerarPulsarBoton(devExt, (idx * 8) + exp);
						else
							GenerarSoltarBoton(devExt, (idx * 8) + exp);
					}
				}
			}
		}

		// Setas

		for(idx = 0; idx < 4; idx++)
		{
			if(hidData->Setas[idx] != viejohidData.Setas[idx])
			{
				if(viejohidData.Setas[idx] != 0)
					GenerarSoltarSeta(devExt, (idx * 8) + viejohidData.Setas[idx] - 1);
				if(hidData->Setas[idx] != 0)
					GenerarPulsarSeta(devExt, (idx * 8) + hidData->Setas[idx] - 1);
			}
		}

		// Ejes

		for(idx = 0; idx < 7; idx++)
		{
			if( *((USHORT*)&hidData->Ejes[idx * 2]) != *((USHORT*)&viejohidData.Ejes[idx * 2]) )
				GenerarAccionesEjes(devExt, idx, *((USHORT*)&hidData->Ejes[idx * 2]) );
		}

		// Sensibilidad y mapeado
		RtlZeroMemory(&outputData, sizeof(HID_INPUT_DATA));
		SensibilidadYMapeado(devExt, &viejohidData, hidData, &outputData);

		//Frenos y R movido
		outputData.Ejes[14] = hidData->Ejes[14];
		outputData.Ejes[15] = hidData->Ejes[15];
		outputData.Ejes[16] = hidData->Ejes[16];
		outputData.Ejes[17] = hidData->Ejes[17];
	}
	else
	{
		RtlCopyMemory(&outputData, hidData, sizeof(HID_INPUT_DATA));
	}

	GenerarHIDEjes(devExt, &outputData);
}


