#include <ntddk.h>
#include <wdf.h>
#include "hidfilter.h"
#include "acciones.h"

#define _EJES_
#include "ejes.h"
#undef _EJES


VOID Calibrado(PITFDEVICE_EXTENSION idevExt, PHID_INPUT_DATA datosHID)
{
	UCHAR		idx;
	LONG		pollEje[4];
	STLIMITES	limites[4];
	STJITTER	jitter[4];

	WdfSpinLockAcquire(idevExt->slCalibrado);
		RtlCopyMemory(limites, idevExt->limites, sizeof(STLIMITES) * 4);
		RtlCopyMemory(jitter, idevExt->jitter, sizeof(STJITTER) * 4);
	WdfSpinLockRelease(idevExt->slCalibrado);

	// Filtrado de ejes
	for(idx = 0; idx < 4; idx++)
	{
		pollEje[idx] = *((UINT16*)&datosHID->Ejes[idx * 2]);

		if(jitter[idx].antiv) {
			// Antivibraciones
			if( (pollEje[idx] < (jitter[idx].PosElegida - jitter[idx].Margen)) || (pollEje[idx] > (jitter[idx].PosElegida + jitter[idx].Margen)) )
			{
				jitter[idx].PosRepetida = 0;
				jitter[idx].PosElegida = (UINT16)pollEje[idx];
			}
			else
			{
				if(jitter[idx].PosRepetida < jitter[idx].Resistencia) {
					jitter[idx].PosRepetida++;
					pollEje[idx] = jitter[idx].PosElegida;
				}
				else
				{
					jitter[idx].PosRepetida = 0;
					jitter[idx].PosElegida = (UINT16)pollEje[idx];
				}
			}
		}

		if(limites[idx].cal)
		{
			// Calibrado
			UINT16 ancho1,ancho2;
			UINT16 topes[] = {2047, 2047, 1023, 255};
			ancho1 = (limites[idx].c - limites[idx].n) - limites[idx].i;
			ancho2 = limites[idx].d - (limites[idx].c + limites[idx].n);
			if( ((pollEje[idx] >= (limites[idx].c - limites[idx].n)) && (pollEje[idx] <= (limites[idx].c + limites[idx].n)) ))
			{
				if(idx == 0 || idx == 1) {
					pollEje[idx] = 1023;
					continue;
				} else {
					if(idx == 2)
						pollEje[2] = 511;
					else
						pollEje[idx] = 127;
					continue;
				}
			}
			else 
			{
		        if(pollEje[idx] < limites[idx].i)
					pollEje[idx] = limites[idx].i;
				else
				{
					if(pollEje[idx] > limites[idx].d)
						pollEje[idx] = limites[idx].d;
				}
				if(pollEje[idx] < limites[idx].c)
				{
					pollEje[idx] = ((limites[idx].c - limites[idx].n) - pollEje[idx]);
					if(ancho1 > ancho2)
					{
						pollEje[idx]= ((pollEje[idx] * ancho2) / ancho1);
					}
					pollEje[idx] = 0 - pollEje[idx];
				} 
				else
				{
					pollEje[idx] -= (limites[idx].c + limites[idx].n);
					if(ancho2 > ancho1)
						pollEje[idx] = ((pollEje[idx] * ancho1) / ancho2);
				}
			}
			if(ancho2 > ancho1)
				pollEje[idx] = ((pollEje[idx] + ancho1) * (topes[idx])) / (2 * ancho1);
			else
				pollEje[idx] = ((pollEje[idx] + ancho2) * (topes[idx])) / (2 * ancho2);
		}
	}

	for(idx = 0; idx < 4; idx++)
		*((UINT16*)&datosHID->Ejes[idx * 2]) = (UINT16)pollEje[idx];
}

VOID SensibilidadYMapeado(
			PDEVICE_EXTENSION devExt,
			PHID_INPUT_DATA viejo,
			PHID_INPUT_DATA entrada,
			PHID_INPUT_DATA salida
			)
{
	PITFDEVICE_EXTENSION itfExt = &devExt->itfExt;
	UCHAR	idx;
	UINT16	topes[] = {2047,2047,1023,255,255,255,255,15,15};
	ULONG	x;
	UCHAR	pos;
	UCHAR	sy1;
	UCHAR	sy2;
	UINT16	stope;


	HID_INPUT_DATA nuevoHID;
	RtlCopyMemory(&nuevoHID, entrada, sizeof(HID_INPUT_DATA));


	//Sensibilidad
	for(idx = 0; idx < 4; idx++)
	{
		x = *((UINT16*)&entrada->Ejes[idx * 2]);
		if(x == (ULONG)(topes[idx] / 2))
		{
			continue;
		}
		else
		{
			pos = (UCHAR) ((x * 20) / topes[idx]);
			if(pos == 20) pos = 19;
			if( (UINT16)x < (topes[idx] / 2) )
			{
				WdfSpinLockAcquire(itfExt->slMapas);
					sy1 = 100 - itfExt->MapaEjes[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].Sensibilidad[9 - pos];
					sy2 = ((pos ==9 ) ? 100 : 100) - itfExt->MapaEjes[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].Sensibilidad[8 - pos];
				WdfSpinLockRelease(itfExt->slMapas);
				stope = (topes[idx] / 2) + 1;
				x = (  ( (sy2 - sy1) * ((20 * x)-(pos * topes[idx])) ) + (2 * sy1 * stope) ) /200;
			}
			else
			{
				WdfSpinLockAcquire(itfExt->slMapas);
					sy1 = (pos == 10) ? 0 : itfExt->MapaEjes[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].Sensibilidad[pos - 11];
					sy2 = itfExt->MapaEjes[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].Sensibilidad[pos - 10];
				WdfSpinLockRelease(itfExt->slMapas);
				stope = (topes[idx] / 2) + 1;
				x = stope + ((  ( (sy2 - sy1) * ((20 * x) - (pos * topes[idx])) ) + (2 * sy1 * stope) ) /200);
			}
			*((UINT16*)&entrada->Ejes[idx * 2]) = (UINT16)x;
		}
	}

	//Mapeado
	for(idx = 0; idx < 7; idx++)
	{
		UCHAR	nEje;
		UCHAR	sRaton;
		BOOLEAN invertido = FALSE;

		WdfSpinLockAcquire(itfExt->slMapas);
			if(idx < 4)
			{
				nEje = itfExt->MapaEjes[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].nEje & 127;
				sRaton = itfExt->MapaEjes[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].Mouse;
			} 
			else
			{
				nEje = itfExt->MapaEjesPeque[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx-4].nEje & 127;
				sRaton=itfExt->MapaEjesPeque[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx-4].Mouse;
			}
		WdfSpinLockRelease(itfExt->slMapas);
		if(nEje > 19) { invertido = TRUE; nEje -= 20; }
		if(nEje != 0)
		{
			if(nEje < 8)
			{
				*((USHORT*)&salida->Ejes[(nEje - 1) * 2]) = ((*((USHORT*)&entrada->Ejes[idx * 2])) * topes[nEje - 1] ) / topes[idx];
			}
			else if(nEje < 10)
			{
				salida->MiniStick &= (nEje == 8)? 0xf0 : 0x0f; 
				salida->MiniStick |= ( ( ((*((USHORT*)&entrada->Ejes[idx * 2])) * topes[nEje - 1]) / topes[idx] ) << (4 * (nEje - 8)) );
			}
			else
			{
				if( *((USHORT*)&nuevoHID.Ejes[idx * 2])!= *((USHORT*)&viejo->Ejes[idx * 2]) )
				{
					if( *((UINT16*)&entrada->Ejes[idx]) == (topes[idx] / 2) )
					{
							GenerarAccionRaton(devExt, nEje - 9, 0);
					} 
					else if( *((UINT16*)&entrada->Ejes[idx]) < (topes[idx] / 2) )
					{
						if(invertido)
							GenerarAccionRaton(devExt, nEje - 9, (UCHAR)(( ((topes[idx] / 2) - (*((UINT16*)&entrada->Ejes[idx * 2]))) *sRaton) / (topes[idx] / 2)) );
						else
							GenerarAccionRaton(devExt, nEje - 9, (UCHAR)(- (( ((topes[idx] / 2) - (*((UINT16*)&entrada->Ejes[idx * 2]))) *sRaton) / (topes[idx] / 2))) );

					}
					else 
					{
						if(invertido)
							GenerarAccionRaton(devExt, nEje - 9, (UCHAR)(- (((*((UINT16*)&entrada->Ejes[idx * 2])-(topes[idx] / 2)) * sRaton) / (topes[idx] / 2))) );
						else
							GenerarAccionRaton(devExt, nEje - 9, (UCHAR)(((*((UINT16*)&entrada->Ejes[idx * 2]) - (topes[idx] / 2))* sRaton) / (topes[idx] / 2)) );
					}
				}
			}
			if(invertido)
			{
				if(nEje < 8)
					*((USHORT*)&salida->Ejes[(nEje - 1) * 2]) = topes[nEje - 1] - ( *((USHORT*)&salida->Ejes[(nEje - 1) * 2]) );
				else if(nEje < 10)
					salida->MiniStick = 15 - salida->MiniStick;
			}
		}
	}

	for(idx = 0; idx < 2; idx++)
	{
		UCHAR nEje;
		UCHAR sRaton;
		BOOLEAN invertido = FALSE;

		WdfSpinLockAcquire(itfExt->slMapas);
			nEje = itfExt->MapaEjesMini[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].nEje & 127;
			sRaton = itfExt->MapaEjesMini[itfExt->stPinkie][itfExt->stModo][itfExt->stAux][idx].Mouse;
		WdfSpinLockRelease(itfExt->slMapas);
		if(nEje > 19) { invertido = TRUE; nEje -= 20; }
		if(nEje != 0)
		{
			if(nEje < 8)
			{
				*((USHORT*)&salida->Ejes[(nEje - 1) * 2]) = ( ((idx == 0) ? (entrada->MiniStick & 0xf) : (entrada->MiniStick >> 4)) * topes[nEje - 1] ) / 15;
			}
			else if(nEje < 10)
			{
				salida->MiniStick &= (nEje == 8) ? 0xf0 : 0x0f; 
				salida->MiniStick |= ((idx == 0) ? (entrada->MiniStick & 0xf) : (entrada->MiniStick >> 4)) << (4 * (nEje - 8));
			}
			else
			{
				if( nuevoHID.MiniStick != viejo->MiniStick )
				{
					if( ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) == 8 )
					{
						GenerarAccionRaton(devExt, nEje - 9, 0);
					}
					else if( ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) < 8 )
					{
						if(invertido)
							GenerarAccionRaton(devExt, nEje - 9, ( (8 - ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf)) * sRaton) / 8 );
						else
							GenerarAccionRaton(devExt, nEje - 9, -( ( (8 - ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf)) * sRaton) / 8) );
					}
					else 
					{
						if(invertido)
							GenerarAccionRaton(devExt, nEje - 9, - ( ( (((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) - 8) * sRaton ) / 7 ) );
						else
							GenerarAccionRaton(devExt, nEje - 9, ( (((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) - 8) * sRaton ) / 7 );
					}
				}
			}
			if(invertido)
			{
				if(nEje < 8)
					*((USHORT*)&salida->Ejes[(nEje - 1) * 2]) = topes[nEje - 1] - ( *((USHORT*)&salida->Ejes[(nEje - 1) * 2]) );
				else if(nEje < 10)
				{
					salida->MiniStick = 15 - salida->MiniStick;
				}
			}
		}
	}
}

VOID GenerarAccionRaton(PDEVICE_EXTENSION devExt, UCHAR eje, CHAR mov)
{
	struct {
		UCHAR tipo;
		UCHAR dato;
	} accion;

	if(eje == 1)
	{
		if(mov == 0)
		{
			accion.tipo = 32 + 4;
			accion.dato = 0;
		}
		else
		{
			if(mov >= 0)
			{
				accion.tipo = 5;
				accion.dato = mov;
			}
			else
			{
				accion.tipo = 4;
				accion.dato = -mov;
			}
		}
	} else {
		if(mov == 0)
		{
			accion.tipo = 32 + 6;
			accion.dato = 0;
		} 
		else
		{
			if(mov >= 0)
			{
				accion.tipo = 7;
				accion.dato = mov;
			}
			else
			{
				accion.tipo = 6;
				accion.dato = -mov;
			}
		}
	}

	AccionarRaton(devExt, (PUCHAR)&accion);
}

VOID GenerarAccionesEjes(
	PDEVICE_EXTENSION devExt,
	UCHAR idx,
	USHORT nuevo)
{
	PITFDEVICE_EXTENSION idevExt = &devExt->itfExt;
	UINT16 accionID = 0; //pirada de pinza del compilador warning false no es necesario "= 0"
	UCHAR cambio;

	WdfSpinLockAcquire(idevExt->slMapas);
		cambio = TraducirGiratorio(idevExt, idx, nuevo);
		if(cambio != 0)
		{
			if(idx < 4)
				accionID = idevExt->MapaEjes[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx].Indices[cambio - 1];
			else
				accionID = idevExt->MapaEjesPeque[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][idx - 4].Indices[cambio - 1];
		}
	WdfSpinLockRelease(idevExt->slMapas);

	if(cambio != 0) AccionarComando(devExt, accionID, 0);
}

UCHAR TraducirGiratorio(PITFDEVICE_EXTENSION idevExt, UCHAR eje, USHORT nueva)
{
	USHORT	topes[]	= {2047, 2047, 1023, 255, 255, 255, 255, 15, 15};
	UCHAR	idn		= 0;
	USHORT	vieja	= idevExt->posVieja[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje];	
    BOOLEAN incremental;

	if(eje < 4)
		incremental = (idevExt->MapaEjes[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje].nEje & 128) == 128;
	else
		incremental = (idevExt->MapaEjesPeque[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje - 4].nEje & 128) == 128;


	if (incremental)
	{
		if (nueva > vieja)
		{
			UCHAR posiciones;
			if(eje < 4)
				posiciones = (UCHAR)idevExt->MapaEjes[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje].Indices[3];
			else
				posiciones = (UCHAR)idevExt->MapaEjesPeque[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje - 4].Indices[3];

			if(vieja < (topes[eje] - posiciones))
			{
				if (nueva > (vieja + posiciones))
				{
					idevExt->posVieja[(UCHAR)idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje] = nueva;
					idn = 1;
				}
			}
		}
		else
		{
			UCHAR posiciones;
			if(eje < 4)
				posiciones = (UCHAR)idevExt->MapaEjes[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje].Indices[2];
			else
				posiciones = (UCHAR)idevExt->MapaEjesPeque[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje - 4].Indices[2];

			if(vieja > posiciones)
			{
				if (nueva < (vieja - posiciones))
				{
					idevExt->posVieja[(UCHAR)idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje] = nueva;
					idn = 2;
				}
			}
		}
	} 
	else
	{
		USHORT	posActual	= 10000;
		UCHAR	posant		= 0;
		UCHAR	idc;

		for(idc = 0; idc < 15; idc++)
		{
			UCHAR banda;
			if(eje < 4)
				banda = idevExt->MapaEjes[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje].Bandas[idc];
			else
				banda = idevExt->MapaEjesPeque[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje - 4].Bandas[idc];

			if(banda == 0)
				break;
			else
			{
				if(nueva >= posant && nueva<((banda * topes[eje]) / 100) )
				{
					posActual = idc;
					break;
				}
				posant = (UCHAR)((banda * topes[eje]) / 100);
			}
		}
		if(posActual == 10000)
		{
			if(nueva >= posant && nueva < (topes[eje] + 1) )
				posActual = idc;
		}
		if(posActual != 10000 && posActual != vieja)
		{
			idevExt->posVieja[idevExt->stPinkie][idevExt->stModo][idevExt->stAux][eje] = posActual;
			idn = (UCHAR)posActual + 1;
		}
	}

	return idn;
}

VOID GenerarHIDEjes(
	PDEVICE_EXTENSION devExt,
	PHID_INPUT_DATA datosHid
	)
{	//No hace falta procesar los datos
	AccionarHOTAS(devExt, datosHid);
}