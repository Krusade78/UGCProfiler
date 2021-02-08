#include "../../framework.h"
#include "CGenerarEventos.h"
#include "ProcesarUSBs_Ejes.h"

void CEjes::SensibilidadYMapeado(CPerfil* pPerfil, TipoJoy tipoJ, PVHID_INPUT_DATA viejo, PVHID_INPUT_DATA entrada)
{
	UCHAR tipo = static_cast<UCHAR>(tipoJ);
	UCHAR idx;
	LONG x;
	UCHAR pos;
	UCHAR sy1;
	UCHAR sy2;

	UCHAR pinkie;
	UCHAR modos;
	pPerfil->LockEstado();
	{
		pinkie = pPerfil->GetEstado()->Pinkie;
		modos = pPerfil->GetEstado()->Modos;
	}
	pPerfil->UnlockEstado();

	UCHAR mapa[3] = { 0, 0, 0 };
	VHID_INPUT_DATA salida[3];

	//Sensibilidad
	for (idx = 0; idx < 8; idx++)
	{
		INT16 stope = (idx < 6) ? 32767 : 127;

		x = entrada->Ejes[idx];
		if (x == 0)
		{
			continue;
		}
		else
		{
			bool izq = (x < 0);
			x = (izq) ? - x : x;
			pos = (UCHAR)((x * 10) / stope);
			pPerfil->InicioLecturaPr();
			{
				sy1 = (pos == 0) ? 0 : pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Sensibilidad[pos - 1];
				sy2 = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Sensibilidad[pos];
			}
			pPerfil->FinLecturaPr();
			x = ((((sy2 - sy1) * ((10 * x) - (pos * stope))) + (sy1 * stope))) / 100;
			x = (izq) ? - x : x;
			entrada->Ejes[idx] = static_cast<INT16>(x);
		}
	}

	//Mapeado
	for (idx = 0; idx < 8; idx++)
	{
		UCHAR joy;
		UCHAR tipoEje;
		UCHAR nEje;
		UCHAR sRaton; //t

		pPerfil->InicioLecturaPr();
		{
			joy = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].JoySalida;
			tipoEje = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].TipoEje;
			nEje = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Eje;
			sRaton = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].SensibilidadRaton;
		}
		pPerfil->FinLecturaPr();

		if ((tipoEje & 1) == 1) // eje normal
		{
			salida[joy].Ejes[nEje] = entrada->Ejes[idx];
			if ((tipoEje & 0x2) == 2) //invertido normal
			{
				salida[joy].Ejes[nEje] = -salida->Ejes[nEje];
			}
			mapa[joy] |= 1 << nEje;
		}
		else if (tipoEje & 0x8) //mapear al ratón
		{
			if (entrada->Ejes[idx] != viejo->Ejes[idx])
			{
				if (entrada->Ejes[idx] == 0)
				{
					EjeARaton(nEje, 0);
				}
				else
				{
					INT16 stope = (idx < 6) ? 32767 : 127;

					if ((tipoEje & 0x2) == 0x2) //invertido
					{
						EjeARaton(nEje, static_cast<CHAR>(((-entrada->Ejes[idx]) * sRaton) / stope));
					}
					else
					{
						EjeARaton(nEje, static_cast<CHAR>((entrada->Ejes[idx] * sRaton) / stope));
					}

				}
			}
		}
	}

	for (idx = 0; idx < 3; idx++)
	{
		if (mapa[idx] != 0)
		{
			CGenerarEventos::DirectX(idx, mapa[idx], &salida[idx]);
		}
	}
}

void CEjes::EjeARaton(UCHAR eje, CHAR mov)
{
	EV_COMANDO evento;

	if (eje == 0)
	{
		if (mov == 0)
		{
			evento.Tipo = TipoComando::Soltar | TipoComando::RatonIzq;
			evento.Dato = 0;
		}
		else
		{
			if (mov >= 0)
			{
				evento.Tipo = TipoComando::RatonDer;
				evento.Dato = mov;
			}
			else
			{
				evento.Tipo = TipoComando::RatonIzq;
				evento.Dato = -mov;
			}
		}
	}
	else {
		if (mov == 0)
		{
			evento.Tipo = TipoComando::Soltar | TipoComando::RatonArr;
			evento.Dato = 0;
		}
		else
		{
			if (mov >= 0)
			{
				evento.Tipo = TipoComando::RatonAba;
				evento.Dato = mov;
			}
			else
			{
				evento.Tipo = TipoComando::RatonArr;
				evento.Dato = -mov;
			}
		}
	}

	CGenerarEventos::Raton(&evento);
}

void CEjes::MoverEje(CPerfil* pPerfil, TipoJoy tipoJ, UCHAR idx, INT16 nuevo)
{
	UCHAR tipo = static_cast<UCHAR>(tipoJ);
	UINT16 accionID;
	UCHAR cambio;
	EV_COMANDO datosEje;
	UCHAR pinkie;
	UCHAR modos;

	pPerfil->InicioLecturaPr();
	{
		pPerfil->LockEstado();
		{
			pinkie = pPerfil->GetEstado()->Pinkie;
			modos = pPerfil->GetEstado()->Modos;
			cambio = TraducirGiratorio(pPerfil, tipo, idx, nuevo, pinkie, modos);
			datosEje.Extendido.Incremental = pPerfil->GetEstado()->Ejes[tipo][pinkie][modos][idx].PosIncremental;
			datosEje.Extendido.Banda = pPerfil->GetEstado()->Ejes[tipo][pinkie][modos][idx].Banda;
		}
		pPerfil->UnlockEstado();
		if (cambio != 255)
		{
			accionID = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Indices[cambio];
			datosEje.Extendido.Modo = modos;
			datosEje.Extendido.Pinkie = pinkie;
		}
	}
	pPerfil->FinLecturaPr();

	if (cambio != 255)
	{
		CGenerarEventos::Comando(tipoJ, accionID, idx, CGenerarEventos::Origen::Eje, &datosEje);
	}
}

/// <summary>
/// En LockEstado
/// </summary>
UCHAR CEjes::TraducirGiratorio(CPerfil* pPerfil, UCHAR tipoJ, UCHAR eje, INT16 nueva, UCHAR pinkie, UCHAR modos)
{
	UCHAR	idn = 255;
	bool incremental;
	bool bandas;
	INT16 rango = (eje < 6) ? 32767 : 127;

	incremental = (pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].TipoEje & 16) == 16;
	bandas = (pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].TipoEje & 32) == 32;

	if (incremental)
	{
		UINT16 vieja = pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].PosIncremental;
		if (nueva > vieja)
		{
			UCHAR posiciones = pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].ResistenciaInc;
			if (vieja < (rango - posiciones))
			{
				if (nueva > (vieja + posiciones))
				{
					pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].PosIncremental = nueva;
					idn = 0;
				}
			}
		}
		else
		{
			UCHAR posiciones = pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].ResistenciaDec;
			if (vieja > posiciones)
			{
				if (nueva < (vieja - posiciones))
				{
					pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].PosIncremental = nueva;
					idn = 1;
				}
			}
		}
	}
	else if (bandas)
	{
		UCHAR	bandaAntigua = pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].Banda;
		UCHAR	bandaActual = 255;
		INT16	posAnterior = 0;
		UCHAR	idc;

		for (idc = 0; idc < 15; idc++)
		{
			bool salir = false;
			UCHAR banda = pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].Bandas[idc];

			if ((banda == 0) || (banda >= 100))
			{
				banda = 100;
				salir = true;
			}

			if ((nueva >= posAnterior) && (nueva < ((banda * rango) / 100)))
			{
				bandaActual = idc;
				break;
			}
			if (salir)
			{
				break;
			}
			posAnterior = static_cast<UCHAR>((banda * rango) / 100);
		}
		if ((bandaActual != 255) && (bandaActual != bandaAntigua))
		{
			pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].Banda = bandaActual;
			idn = bandaActual;
		}
	}

	return idn;
}