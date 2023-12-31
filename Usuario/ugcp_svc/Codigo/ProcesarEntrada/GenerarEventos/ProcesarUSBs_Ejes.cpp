#include "../../framework.h"
#include "CGenerarEventos.h"
#include "ProcesarUSBs_Ejes.h"

void CEjes::SensibilidadYMapeado(CPerfil* pPerfil, TipoJoy tipoJ, PVHID_INPUT_DATA viejo, PVHID_INPUT_DATA entrada)
{
	UCHAR tipo = static_cast<UCHAR>(tipoJ);
	UCHAR idx;
	INT32 x;
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
		UCHAR sy1;
		UCHAR sy2;
		pPerfil->InicioLecturaCal();
		UINT16 stopeSl = pPerfil->GetCal()->Limites[static_cast<UCHAR>(tipoJ)][idx].Rango;
		INT16 stope = pPerfil->GetCal()->Limites[static_cast<UCHAR>(tipoJ)][idx].Cen;
		pPerfil->FinLecturaCal();
		if (stopeSl == 0) { stopeSl = 32767; }
		if (stope == 0) { stope = stopeSl / 2; }
		bool slider = false;
		pPerfil->InicioLecturaPr();
		{
			slider = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Slider;
		}
		pPerfil->FinLecturaPr();

		x = entrada->Ejes[idx];

		if (!slider && (x == stope))
		{
			continue;
		}
		bool izq = (x < stope);
		x = slider ? x : ((izq) ? stope - x : x - stope );
		UCHAR pos = slider ? (UCHAR)((x * 10) / stopeSl) : (UCHAR)((x * 10) / stope);
		if (pos == 10)
		{
			pos = 9;
		}
		pPerfil->InicioLecturaPr();
		{
			sy1 = (pos == 0) ? 0 : pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Sensibilidad[pos - 1];
			sy2 = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Sensibilidad[pos];
		}
		pPerfil->FinLecturaPr();
		if (slider)
		{
			x = (x == stopeSl) ? ((sy2 * stopeSl) / 100) : ((((sy2 - sy1) * ((10 * x) - (pos * stopeSl))) + (sy1 * stopeSl))) / 100;
		}
		else
		{
			x = (x == stope) ? ((sy2 * stope) / 100) : ((((sy2 - sy1) * ((10 * x) - (pos * stope))) + (sy1 * stope))) / 100;
			x = (izq) ? stope - x : x + stope;
		}
		entrada->Ejes[idx] = static_cast<UINT16>(x);
	}

	//Mapeado
	for (idx = 0; idx < 8; idx++)
	{
		UCHAR joy;
		UCHAR tipoEje;
		UCHAR nEje;
		UCHAR sRaton; //t
		UINT16 etope, stope, centro;

		pPerfil->InicioLecturaPr();
		{
			joy = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].JoySalida;
			tipoEje = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].TipoEje;
			nEje = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Eje;
			sRaton = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].SensibilidadRaton;
		}
		pPerfil->FinLecturaPr();
		pPerfil->InicioLecturaCal();
		{
			etope = pPerfil->GetCal()->Limites[static_cast<unsigned char>(tipoJ)][idx].Rango;
			centro = pPerfil->GetCal()->Limites[static_cast<unsigned char>(tipoJ)][idx].Cen;
			stope = etope;
		}
		pPerfil->FinLecturaCal();

		if (tipoEje == 0)
		{
			continue;
		}
		else if ((tipoEje & 1) == 1) // eje normal
		{
			salida[joy].Ejes[nEje] = entrada->Ejes[idx];
			if ((tipoEje & 0x2) == 2) //invertido normal
			{
				salida[joy].Ejes[nEje] = stope - salida[joy].Ejes[nEje];
			}
			mapa[joy] |= 1 << nEje;
		}
		else if (tipoEje & 0x8) //mapear al rat�n
		{
			if (entrada->Ejes[idx] != viejo->Ejes[idx])
			{
				if (entrada->Ejes[idx] == centro)
				{
					EjeARaton(nEje, 0);
				}
				else
				{
					INT32 ejeTransformado = entrada->Ejes[idx] - centro;
					if ((tipoEje & 0x2) == 0x2) //invertido
					{
						EjeARaton(nEje, static_cast<CHAR>(-ejeTransformado * sRaton));
					}
					else
					{
						EjeARaton(nEje, static_cast<CHAR>(ejeTransformado * sRaton));
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

void CEjes::MoverEje(CPerfil* pPerfil, TipoJoy tipoJ, UCHAR idx, UINT16 nuevo)
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
		if (accionID == 0)
		{
			CGenerarEventos::CheckHolds();
		}
		else
		{
			CGenerarEventos::Comando(tipoJ, accionID, idx, CGenerarEventos::Origen::Eje, &datosEje);
		}
	}
}

/// <summary>
/// En LockEstado
/// </summary>
UCHAR CEjes::TraducirGiratorio(CPerfil* pPerfil, UCHAR tipoJ, UCHAR eje, UINT16 nueva, UCHAR pinkie, UCHAR modos)
{
	UCHAR idn = 255;
	bool incremental;
	bool bandas;

	incremental = (pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].TipoEje & 16) == 16;
	bandas = (pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].TipoEje & 32) == 32;

	UINT16 rango = pPerfil->GetCal()->Limites[tipoJ][eje].Rango;
	UINT16	posNueva = nueva;

	if (incremental)
	{
		UINT16 vieja = pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].PosIncremental;
		if (nueva > vieja)
		{
			UCHAR posiciones = pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].ResistenciaInc;
			if (vieja < (rango - posiciones))
			{
				if (posNueva > (vieja + posiciones))
				{
					pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].PosIncremental = posNueva;
					idn = 0;
				}
			}
		}
		else
		{
			UCHAR posiciones = pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].ResistenciaDec;
			if (vieja > posiciones)
			{
				if (posNueva < (vieja - posiciones))
				{
					pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].PosIncremental = posNueva;
					idn = 1;
				}
			}
		}
	}
	else if (bandas)
	{

		UCHAR	bandaAntigua = pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].Banda;
		UCHAR	bandaActual = 255;
		UINT16	posAnterior = 0;
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

			if ((posNueva >= posAnterior) && (posNueva < ((banda * rango) / 100)))
			{
				bandaActual = idc;
				break;
			}
			if (salir)
			{
				break;
			}
			posAnterior = static_cast<UINT16>((banda * rango) / 100);
		}
		if ((bandaActual != 255) && (bandaActual != bandaAntigua))
		{
			pPerfil->GetEstado()->Ejes[tipoJ][pinkie][modos][eje].Banda = bandaActual;
			idn = bandaActual;
		}
	}

	return idn;
}