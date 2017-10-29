#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#include "AccionesGenerar.h"
#define _EJES_
#include "ejes_raton.h"
#undef _EJES

VOID SensibilidadYMapeado(
			WDFDEVICE		device,
			PHID_INPUT_DATA viejo,
			PHID_INPUT_DATA entrada,
			PHID_INPUT_DATA salida
			)
{
	PROGRAMADO_CONTEXT*	itfExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT*		hidCtx = &GetDeviceContext(device)->HID;
	UCHAR				idx;
	//UINT16				topes[] = {2047,2047,1023,255,255,255,255,15,15};
	ULONG				x;
	UCHAR				pos;
	UCHAR				sy1;
	UCHAR				sy2;
	UINT16				stope;

	//Sensibilidad
	for(idx = 0; idx < 4; idx++)
	{
		x = *((UINT16*)&entrada->Ejes[idx * 2]);
		if(x == (ULONG)(2048 / 2))
		{
			continue;
		}
		else
		{
			pos = (UCHAR) ((x * 20) / 2048);
			if(pos == 20) pos = 19;
			if( (UINT16)x < (2048 / 2) )
			{
				WdfSpinLockAcquire(itfExt->slMapas);
				{
					sy1 = 100 - itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Sensibilidad[9 - pos];
					sy2 = ((pos == 9) ? 100 : 100) - itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Sensibilidad[8 - pos];
				}
				WdfSpinLockRelease(itfExt->slMapas);
				stope = (2048 / 2) + 1;
				x = (  ( (sy2 - sy1) * ((20 * x)-(pos * 2048)) ) + (2 * sy1 * stope) ) /200;
			}
			else
			{
				WdfSpinLockAcquire(itfExt->slMapas);
				{
					sy1 = (pos == 10) ? 0 : itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Sensibilidad[pos - 11];
					sy2 = itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Sensibilidad[pos - 10];
				}
				WdfSpinLockRelease(itfExt->slMapas);
				stope = (2048 / 2) + 1;
				x = stope + ((  ( (sy2 - sy1) * ((20 * x) - (pos * 2048)) ) + (2 * sy1 * stope) ) /200);
			}
			*((UINT16*)&entrada->Ejes[idx * 2]) = (UINT16)x;
		}
	}

	//Mapeado
	for(idx = 0; idx < 8; idx++)
	{
		UCHAR	nEje;
		UCHAR	sRaton;
		BOOLEAN invertido = FALSE;

		WdfSpinLockAcquire(itfExt->slMapas);
		{
			if (idx < 4)
			{
				nEje = itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].nEje & 127;
				sRaton = itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Mouse;
			}
			else
			{
				nEje = itfExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx - 4].nEje & 127;
				sRaton = itfExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx - 4].Mouse;
			}
		}
		WdfSpinLockRelease(itfExt->slMapas);

		if(nEje > 19) { invertido = TRUE; nEje -= 20; }
		if(nEje != 0)
		{
			if(nEje < 9)
			{
				*((USHORT*)&salida->Ejes[(nEje - 1) * 2]) = *((USHORT*)&entrada->Ejes[idx * 2]);
			}
			else
			{
				if( *((USHORT*)&entrada->Ejes[idx * 2])!= *((USHORT*)&viejo->Ejes[idx * 2]) )
				{
					if( *((UINT16*)&entrada->Ejes[idx]) == (2048 / 2) )
					{
						GenerarAccionRaton(device, nEje - 9, 0);
					} 
					else if( *((UINT16*)&entrada->Ejes[idx]) < (2048 / 2) )
					{
						if(invertido)
							GenerarAccionRaton(device, nEje - 9, (UCHAR)(( ((2048 / 2) - (*((UINT16*)&entrada->Ejes[idx * 2]))) *sRaton) / (2048 / 2)) );
						else
							GenerarAccionRaton(device, nEje - 9, (UCHAR)(- (( ((2048 / 2) - (*((UINT16*)&entrada->Ejes[idx * 2]))) *sRaton) / (2048 / 2))) );

					}
					else 
					{
						if(invertido)
							GenerarAccionRaton(device, nEje - 9, (UCHAR)(- (((*((UINT16*)&entrada->Ejes[idx * 2])-(2048 / 2)) * sRaton) / (2048 / 2))) );
						else
							GenerarAccionRaton(device, nEje - 9, (UCHAR)(((*((UINT16*)&entrada->Ejes[idx * 2]) - (2048 / 2))* sRaton) / (2048 / 2)) );
					}
				}
			}
			if(invertido && (nEje < 9))
			{
				*((USHORT*)&salida->Ejes[(nEje - 1) * 2]) = 2048 - ( *((USHORT*)&salida->Ejes[(nEje - 1) * 2]) );
			}
		}
	}

	for(idx = 0; idx < 2; idx++)
	{
		UCHAR nEje;
		UCHAR sRaton;
		BOOLEAN invertido = FALSE;

		WdfSpinLockAcquire(itfExt->slMapas);
		{
			nEje = itfExt->MapaEjesMini[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].nEje & 127;
			sRaton = itfExt->MapaEjesMini[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Mouse;
		}
		WdfSpinLockRelease(itfExt->slMapas);

		if (nEje > 19) { invertido = TRUE; nEje -= 20; }
		if(nEje != 0)
		{
			if(nEje < 10)
			{
				UCHAR nuevo = (idx == 0) ? (entrada->MiniStick & 0xf) : (entrada->MiniStick >> 4);
				if (invertido)
					nuevo = 0xf - nuevo;
				salida->MiniStick &= (nEje == 8) ? 0xf0 : 0x0f; 
				salida->MiniStick |= nuevo << (4 * (nEje - 8));
			}
			else
			{
				if( entrada->MiniStick != viejo->MiniStick )
				{
					if( ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) == 8 )
					{
						GenerarAccionRaton(device, nEje - 9, 0);
					}
					else if( ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) < 8 )
					{
						if(invertido)
							GenerarAccionRaton(device, nEje - 9, ( (8 - ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf)) * sRaton) / 8 );
						else
							GenerarAccionRaton(device, nEje - 9, -( ( (8 - ((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf)) * sRaton) / 8) );
					}
					else 
					{
						if(invertido)
							GenerarAccionRaton(device, nEje - 9, - ( ( (((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) - 8) * sRaton ) / 7 ) );
						else
							GenerarAccionRaton(device, nEje - 9, ( (((entrada->MiniStick >> (4 * (nEje - 10))) & 0xf) - 8) * sRaton ) / 7 );
					}
				}
			}
		}
	}
}

VOID GenerarAccionRaton(WDFDEVICE device, UCHAR eje, CHAR mov)
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

	AccionarRaton(device, (PUCHAR)&accion);
}

VOID GenerarAccionesEjes(WDFDEVICE device, UCHAR idx, USHORT nuevo)
{
	PROGRAMADO_CONTEXT*	itfExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT*		hidCtx = &GetDeviceContext(device)->HID;
	UINT16 accionID = 0; //pirada de pinza del compilador warning false no es necesario "= 0"
	UCHAR cambio;

	WdfSpinLockAcquire(itfExt->slMapas);
	{
		cambio = TraducirGiratorio(device, idx, nuevo);
		if (cambio != 0)
		{
			if (idx < 4)
				accionID = itfExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx].Indices[cambio - 1];
			else
				accionID = itfExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][idx - 4].Indices[cambio - 1];
		}
	}
	WdfSpinLockRelease(itfExt->slMapas);

	if(cambio != 0) AccionarComando(device, accionID, 0);
}

UCHAR TraducirGiratorio(WDFDEVICE device, UCHAR eje, USHORT nueva)
{
	PROGRAMADO_CONTEXT*	idevExt = &GetDeviceContext(device)->Programacion;
	HID_CONTEXT*		hidCtx = &GetDeviceContext(device)->HID;

	UCHAR	idn		= 0;
	USHORT	vieja	= idevExt->posVieja[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje];
    BOOLEAN incremental;

	if(eje < 4)
		incremental = (idevExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje].nEje & 128) == 128;
	else
		incremental = (idevExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje - 4].nEje & 128) == 128;


	if (incremental)
	{
		if (nueva > vieja)
		{
			UCHAR posiciones;
			if(eje < 4)
				posiciones = (UCHAR)idevExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje].ResistenciaInc;
			else
				posiciones = (UCHAR)idevExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje - 4].ResistenciaInc;

			if(vieja < (2048 - posiciones))
			{
				if (nueva > (vieja + posiciones))
				{
					idevExt->posVieja[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje] = nueva;
					idn = 1;
				}
			}
		}
		else
		{
			UCHAR posiciones;
			if(eje < 4)
				posiciones = (UCHAR)idevExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje].ResistenciaDec;
			else
				posiciones = (UCHAR)idevExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje - 4].ResistenciaDec;

			if(vieja > posiciones)
			{
				if (nueva < (vieja - posiciones))
				{
					idevExt->posVieja[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje] = nueva;
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
				banda = idevExt->MapaEjes[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje].Bandas[idc];
			else
				banda = idevExt->MapaEjesPeque[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje - 4].Bandas[idc];

			if(banda == 0)
				break;
			else
			{
				if(nueva >= posant && nueva<((banda * 2048) / 100) )
				{
					posActual = idc;
					break;
				}
				posant = (UCHAR)((banda * 2048) / 100);
			}
		}
		if(posActual == 10000)
		{
			if(nueva >= posant && nueva < (2048 + 1) )
				posActual = idc;
		}
		if(posActual != 10000 && posActual != vieja)
		{
			idevExt->posVieja[hidCtx->EstadoPinkie][hidCtx->EstadoModos][eje] = posActual;
			idn = (UCHAR)posActual + 1;
		}
	}

	return idn;
}