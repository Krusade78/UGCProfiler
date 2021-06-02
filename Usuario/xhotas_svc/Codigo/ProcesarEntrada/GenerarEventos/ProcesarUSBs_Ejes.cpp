#include "../../framework.h"
#include "CGenerarEventos.h"
#include "ProcesarUSBs_Ejes.h"

void CEjes::SensibilidadYMapeado(CPerfil* pPerfil, TipoJoy tipoJ, PVHID_INPUT_DATA viejo, PVHID_INPUT_DATA entrada)
{
	const INT16 stope = 32767;
	const UINT16 stopeSl = 65535;
	UCHAR tipo = static_cast<UCHAR>(tipoJ);
	UCHAR idx;
	LONG x;
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
		x = entrada->Ejes[idx];

		bool slider = false;
		pPerfil->InicioLecturaPr();
		{
			slider = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Slider;
		}
		pPerfil->FinLecturaPr();
		if (!slider && (x == 0))
		{
			continue;
		}
		bool izq = (x < 0);
		x = slider ? (x + stope) : ((izq) ? - x : x);
		UCHAR pos = slider ? (UCHAR)((x * 10) / stopeSl) : (UCHAR)((x * 10) / (stope + 1));
		pPerfil->InicioLecturaPr();
		{
			sy1 = (pos == 0) ? 0 : pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Sensibilidad[pos - 1];
			sy2 = pPerfil->GetPr()->MapaEjes[tipo][pinkie][modos][idx].Sensibilidad[pos];
		}
		pPerfil->FinLecturaPr();
		if (slider)
		{
			x = (x == 65534) ? ((sy2 * 65534) / 100) : ((((sy2 - sy1) * ((10 * x) - (pos * stopeSl))) + (sy1 * stopeSl))) / 100;
			x -= stope ;
		}
		else
		{
			x = (x == stope) ? ((sy2 * stope) / 100) : ((((sy2 - sy1) * ((10 * x) - (pos * stope))) + (sy1 * stope))) / 100;
			x = (izq) ? -x : x;
		}
		entrada->Ejes[idx] = static_cast<INT16>(x);
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

		if (tipoEje == 0)
		{
			continue;
		}
		else if ((tipoEje & 1) == 1) // eje normal
		{
			salida[joy].Ejes[nEje] = entrada->Ejes[idx];
			if ((tipoEje & 0x2) == 2) //invertido normal
			{
				salida[joy].Ejes[nEje] = -salida[joy].Ejes[nEje];
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
					INT16 stope = 32767;

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
UCHAR CEjes::TraducirGiratorio(CPerfil* pPerfil, UCHAR tipoJ, UCHAR eje, INT16 nueva, UCHAR pinkie, UCHAR modos)
{
	UCHAR idn = 255;
	bool incremental;
	bool bandas;

	incremental = (pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].TipoEje & 16) == 16;
	bandas = (pPerfil->GetPr()->MapaEjes[tipoJ][pinkie][modos][eje].TipoEje & 32) == 32;

	const UINT16 rango = 65535;
	UINT16	posNueva = (nueva < 0) ? static_cast<UINT16>(nueva + 32767) : static_cast<UINT16>(nueva) + 32768;

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