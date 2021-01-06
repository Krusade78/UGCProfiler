#include <ntddk.h>
#include <wdf.h>
#include "context.h"
#define _PUBLIC_
#include "EventosGenerar.h"
#include "EventosProcesar.h"
#define _PRIVATE_
#include "ProcesarUSBs_Ejes.h"
#undef _PRIVATE_
#undef _PUBLIC_

#ifdef ALLOC_PRAGMA
#pragma alloc_text (PAGE, SensibilidadYMapeado)
#pragma alloc_text (PAGE, EjeARaton)
#pragma alloc_text (PAGE, MoverEje)
#pragma alloc_text (PAGE, TraducirGiratorio)
#endif

VOID SensibilidadYMapeado(
	WDFDEVICE		device,
	PHID_INPUT_DATA viejo,
	PHID_INPUT_DATA entrada,
	PHID_INPUT_DATA salida
)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT* prCtx = &GetDeviceContext(device)->Perfil;
	UCHAR				idx;
	ULONG				x;
	UCHAR				pos;
	UCHAR				sy1;
	UCHAR				sy2;
	UINT16				stope = (2048 / 2);

	UCHAR pinkie;
	UCHAR modos;
	WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
	{
		pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
		modos = GetDeviceContext(device)->HID.Estado.Modos;
	}
	WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);

	//Sensibilidad
	for (idx = 0; idx < 4; idx++)
	{
		x = entrada->Ejes[idx];
		if (x == (ULONG)(2048 / 2))
		{
			continue;
		}
		else
		{
			BOOLEAN izq = ((UINT16)x < stope);
			x = (izq) ? stope - 1 - x : x - stope - 1;
			pos = (UCHAR)((x * 10) / stope);
			WdfWaitLockAcquire(prCtx->WaitLockMapas, NULL);
			{
				sy1 = (pos == 0) ? 0 : prCtx->MapaEjes[pinkie][modos][idx].Sensibilidad[pos - 1];
				sy2 = prCtx->MapaEjes[pinkie][modos][idx].Sensibilidad[pos];
			}
			WdfWaitLockRelease(prCtx->WaitLockMapas);
			x = ((((sy2 - sy1) * ((10 * x) - (pos * stope))) + (sy1 * stope))) / 100;
			x = (izq) ? stope - 1 - x : x + stope + 1;
			entrada->Ejes[idx] = (UINT16)x;
		}
	}

	//Mapeado
	for (idx = 0; idx < 9; idx++)
	{
		UCHAR	tipoEje;
		UCHAR	nEje;
		UCHAR	sRaton; //t

		WdfWaitLockAcquire(prCtx->WaitLockMapas, NULL);
		{
			if (idx < 4)
			{
				tipoEje = prCtx->MapaEjes[pinkie][modos][idx].TipoEje;
				nEje = prCtx->MapaEjes[pinkie][modos][idx].Eje;
				sRaton = prCtx->MapaEjes[pinkie][modos][idx].SensibilidadRaton;
			}
			else
			{
				tipoEje = prCtx->MapaEjesPeque[pinkie][modos][idx - 4].TipoEje;
				nEje = prCtx->MapaEjesPeque[pinkie][modos][idx - 4].Eje;
				sRaton = prCtx->MapaEjesPeque[pinkie][modos][idx - 4].SensibilidadRaton;
			}
		}
		WdfWaitLockRelease(prCtx->WaitLockMapas);

		if ((tipoEje & 1) == 1) // eje normal
		{
			salida->Ejes[nEje] = entrada->Ejes[idx];
			if ((tipoEje & 0x2) == 2) //invertido normal
			{
				salida->Ejes[nEje] = 2048 - salida->Ejes[nEje];
			}
		}
		else if (tipoEje & 0x8) //mapear al ratón
		{
			if (entrada->Ejes[idx] != viejo->Ejes[idx])
			{
				if (entrada->Ejes[idx] == (2048 / 2))
				{
					EjeARaton(device, nEje, 0);
				}
				else if (entrada->Ejes[idx] < (2048 / 2))
				{
					if ((tipoEje & 0x2) == 0x2) //invertido
					{
						EjeARaton(device, nEje, (UCHAR)((((2048 / 2) - entrada->Ejes[idx]) * sRaton) / (2048 / 2)));
					}
					else
					{
						EjeARaton(device, nEje, (UCHAR)(-((((2048 / 2) - entrada->Ejes[idx]) * sRaton) / (2048 / 2))));
					}

				}
				else
				{
					if ((tipoEje & 0x2) == 0x2) //invertido
					{
						EjeARaton(device, nEje, (UCHAR)(-(((entrada->Ejes[idx] - (2048 / 2)) * sRaton) / (2048 / 2))));
					}
					else
					{
						EjeARaton(device, nEje, (UCHAR)(((entrada->Ejes[idx] - (2048 / 2)) * sRaton) / (2048 / 2)));
					}
				}
			}
		}
	}

	for (idx = 0; idx < 2; idx++)
	{
		UCHAR tipoEje;
		UCHAR nEje;
		UCHAR sRaton;

		WdfWaitLockAcquire(prCtx->WaitLockMapas, NULL);
		{
			tipoEje = prCtx->MapaEjesMini[pinkie][modos][idx].TipoEje;
			nEje = prCtx->MapaEjesMini[pinkie][modos][idx].nEje;
			sRaton = prCtx->MapaEjesMini[pinkie][modos][idx].SensibilidadRaton;
		}
		WdfWaitLockRelease(prCtx->WaitLockMapas);

		if ((tipoEje & 0x4) == 0x4) //ministick
		{
			UCHAR nuevo = (idx == 0) ? (entrada->MiniStick & 0xf) : (entrada->MiniStick >> 4);
			if ((tipoEje & 0x2) == 0x2) //invertido
				nuevo = 0xf - nuevo;
			salida->MiniStick &= (nEje == 0) ? 0xf0 : 0x0f;  //resetea para que no coincidan 2 ejes iguales
			salida->MiniStick |= nuevo << (4 * nEje);
		}
		else if (tipoEje & 0x8) //mapear al ratón
		{
			if (entrada->MiniStick != viejo->MiniStick)
			{
				if (((entrada->MiniStick >> (4 * nEje)) & 0xf) == 8)
				{
					EjeARaton(device, nEje, 0);
				}
				else if (((entrada->MiniStick >> (4 * nEje)) & 0xf) < 8)
				{
					if ((tipoEje & 0x2) == 0x2) //invertido
					{
						EjeARaton(device, nEje, ((8 - ((entrada->MiniStick >> (4 * nEje)) & 0xf)) * sRaton) / 8);
					}
					else
					{
						EjeARaton(device, nEje, -(((8 - ((entrada->MiniStick >> (4 * nEje)) & 0xf)) * sRaton) / 8));
					}
				}
				else
				{
					if ((tipoEje & 0x2) == 0x2) //invertido
					{
						EjeARaton(device, nEje, -(((((entrada->MiniStick >> (4 * nEje)) & 0xf) - 8) * sRaton) / 7));
					}
					else
					{
						EjeARaton(device, nEje, ((((entrada->MiniStick >> (4 * nEje)) & 0xf) - 8) * sRaton) / 7);
					}
				}
			}
		}
	}
}

VOID EjeARaton(WDFDEVICE device, UCHAR eje, CHAR mov)
{
	PAGED_CODE();

	EV_COMANDO evento;

	if (eje == 0)
	{
		if (mov == 0)
		{
			evento.Tipo = TipoComando_Soltar | TipoComando_RatonIzq;
			evento.Dato = 0;
		}
		else
		{
			if (mov >= 0)
			{
				evento.Tipo = TipoComando_RatonDer;
				evento.Dato = mov;
			}
			else
			{
				evento.Tipo = TipoComando_RatonIzq;
				evento.Dato = -mov;
			}
		}
	}
	else {
		if (mov == 0)
		{
			evento.Tipo = TipoComando_Soltar | TipoComando_RatonArr;
			evento.Dato = 0;
		}
		else
		{
			if (mov >= 0)
			{
				evento.Tipo = TipoComando_RatonAba;
				evento.Dato = mov;
			}
			else
			{
				evento.Tipo = TipoComando_RatonArr;
				evento.Dato = -mov;
			}
		}
	}

	GenerarEventoRaton(device, &evento);
}

VOID MoverEje(WDFDEVICE device, UCHAR idx, UINT16 nuevo)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT* prCtx = &GetDeviceContext(device)->Perfil;
	UINT16 accionID = 0; //pirada de pinza del compilador warning false no es necesario "= 0"
	UCHAR cambio;
	EV_COMANDO_EXTENDIDO datosEje;

	WdfWaitLockAcquire(prCtx->WaitLockMapas, NULL);
	{
		UCHAR pinkie;
		UCHAR modos;
		WdfWaitLockAcquire(GetDeviceContext(device)->HID.Estado.WaitLockEstado, NULL);
		{
			pinkie = GetDeviceContext(device)->HID.Estado.Pinkie;
			modos = GetDeviceContext(device)->HID.Estado.Modos;
		}
		WdfWaitLockRelease(GetDeviceContext(device)->HID.Estado.WaitLockEstado);

		WdfWaitLockAcquire(GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado, NULL);
		{
			cambio = TraducirGiratorio(device, idx, nuevo, pinkie, modos);
			datosEje.Incremental = GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental[pinkie][modos][idx];
			datosEje.Banda = GetDeviceContext(device)->USBaHID.UltimoEstado.Banda[pinkie][modos][idx];
		}
		WdfWaitLockRelease(GetDeviceContext(device)->USBaHID.UltimoEstado.WaitLockUltimoEstado);
		if (cambio != 255)
		{
			if (idx < 4)
			{
				accionID = prCtx->MapaEjes[pinkie][modos][idx].Indices[cambio];
			}
			else
			{
				accionID = prCtx->MapaEjesPeque[pinkie][modos][idx - 4].Indices[cambio];
			}

			datosEje.Modo = modos;
			datosEje.Pinkie = pinkie;
		}

	}
	WdfWaitLockRelease(prCtx->WaitLockMapas);

	if (cambio != 255)
	{
		GenerarEventoComando(device, accionID, idx, Origen_Eje, &datosEje);
	}
}

/// <summary>
/// En USBaHID.UltimoEstado.WaitLockUltimoEstado
/// </summary>
UCHAR TraducirGiratorio(WDFDEVICE device, UCHAR eje, UINT16 nueva, UCHAR pinkie, UCHAR modos)
{
	PAGED_CODE();

	PROGRAMADO_CONTEXT* pCtx = &GetDeviceContext(device)->Perfil;
	UCHAR	idn = 255;
	BOOLEAN incremental;
	BOOLEAN bandas;

	if (eje < 4)
	{
		incremental = (pCtx->MapaEjes[pinkie][modos][eje].TipoEje & 16) == 16;
		bandas = (pCtx->MapaEjes[pinkie][modos][eje].TipoEje & 32) == 32;
	}
	else
	{
		incremental = (pCtx->MapaEjesPeque[pinkie][modos][eje - 4].TipoEje & 16) == 16;
		bandas = (pCtx->MapaEjesPeque[pinkie][modos][eje - 4].TipoEje & 32) == 32;
	}

	if (incremental)
	{
		UINT16 vieja = GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental[pinkie][modos][eje];
		if (nueva > vieja)
		{
			UCHAR posiciones;
			if (eje < 4)
				posiciones = pCtx->MapaEjes[pinkie][modos][eje].ResistenciaInc;
			else
				posiciones = pCtx->MapaEjesPeque[pinkie][modos][eje - 4].ResistenciaInc;

			if (vieja < (2048 - posiciones))
			{
				if (nueva > (vieja + posiciones))
				{
					GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental[pinkie][modos][eje] = nueva;
					idn = 0;
				}
			}
		}
		else
		{
			UCHAR posiciones;
			if (eje < 4)
				posiciones = pCtx->MapaEjes[pinkie][modos][eje].ResistenciaDec;
			else
				posiciones = pCtx->MapaEjesPeque[pinkie][modos][eje - 4].ResistenciaDec;

			if (vieja > posiciones)
			{
				if (nueva < (vieja - posiciones))
				{
					GetDeviceContext(device)->USBaHID.UltimoEstado.PosIncremental[pinkie][modos][eje] = nueva;
					idn = 1;
				}
			}
		}
	}
	else if (bandas)
	{
		UCHAR	bandaAntigua = GetDeviceContext(device)->USBaHID.UltimoEstado.Banda[pinkie][modos][eje];
		UCHAR	bandaActual = 255;
		UINT16	posAnterior = 0;
		UCHAR	idc;

		for (idc = 0; idc < 15; idc++)
		{
			UCHAR banda;
			BOOLEAN salir = FALSE;
			if (eje < 4)
				banda = pCtx->MapaEjes[pinkie][modos][eje].Bandas[idc];
			else
				banda = pCtx->MapaEjesPeque[pinkie][modos][eje - 4].Bandas[idc];

			if ((banda == 0) || (banda >= 100))
			{
				banda = 100;
				salir = TRUE;
			}

			if ((nueva >= posAnterior) && (nueva < ((banda * 2048) / 100)))
			{
				bandaActual = idc;
				break;
			}
			if (salir)
			{
				break;
			}
			posAnterior = (UCHAR)((banda * 2048) / 100);
		}
		if ((bandaActual != 255) && (bandaActual != bandaAntigua))
		{
			GetDeviceContext(device)->USBaHID.UltimoEstado.Banda[pinkie][modos][eje] = bandaActual;
			idn = (UCHAR)bandaActual;
		}
	}

	return idn;
}