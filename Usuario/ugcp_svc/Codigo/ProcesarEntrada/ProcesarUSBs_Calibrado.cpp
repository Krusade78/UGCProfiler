#include "../framework.h"
#include "ProcesarUSBs_Calibrado.h"

void CCalibrado::Calibrar(CPerfil* pPerfil, TipoJoy tipoJ, PVHID_INPUT_DATA datosHID)
{
	//const BYTE mapaEjesUsados[4] = { 0b11001000, 0b00001011, 0b01011111, 0b11111111 }; //Pedales, X52_Joy, X52_Ace, NXT

	UCHAR tipo = static_cast<UCHAR>(tipoJ);
	CPerfil::ST_LIMITES	limites[8];
	CPerfil::ST_JITTER	jitter[8];

	pPerfil->InicioLecturaCal();
	{
		RtlCopyMemory(limites, pPerfil->GetCal()->Limites[tipo], sizeof(limites));
		RtlCopyMemory(jitter, pPerfil->GetCal()->Jitter[tipo], sizeof(jitter));
	}
	pPerfil->FinLecturaCal();

	// Filtrado de ejes
	for (char idx = 0; idx < 8; idx++)
	{
		//if (!((mapaEjesUsados[tipo] >> idx) & 1))
		//{
		//	continue;
		//}
		UINT16 pollEje = static_cast<UINT16>(datosHID->Ejes[idx]);

		if (jitter[idx].Antiv)
		{
			// Antivibraciones
			if ((pollEje < (jitter[idx].PosElegida - jitter[idx].Margen)) || (pollEje > (jitter[idx].PosElegida + jitter[idx].Margen)))
			{
				jitter[idx].PosRepetida = 0;
				jitter[idx].PosElegida = pollEje;
			}
			else
			{
				if (jitter[idx].PosRepetida < jitter[idx].Resistencia)
				{
					jitter[idx].PosRepetida++;
					pollEje = jitter[idx].PosElegida;
				}
				else
				{
					jitter[idx].PosRepetida = 0;
					jitter[idx].PosElegida = pollEje;
				}
			}
		}

		if (limites[idx].Cal)
		{
			// Calibrado
			UINT16 ancho1, ancho2;
			ancho1 = (limites[idx].Cen - limites[idx].Nulo) - limites[idx].Izq;
			ancho2 = limites[idx].Der - (limites[idx].Cen + limites[idx].Nulo);
			if (((pollEje >= (limites[idx].Cen - limites[idx].Nulo)) && (pollEje <= (limites[idx].Cen + limites[idx].Nulo))))
			{
				//Zona nula
				datosHID->Ejes[idx] = limites[idx].Cen;
				continue;
			}
			else
			{
				if (pollEje < limites[idx].Izq)
					pollEje = limites[idx].Izq;
				if (pollEje > limites[idx].Der)
					pollEje = limites[idx].Der;

				if (pollEje < limites[idx].Cen)
				{
					if (ancho1 != (limites[idx].Cen - 0))
					{
						if (pollEje >= ancho1) { pollEje = ancho1; }
						pollEje -= limites[idx].Izq;
						pollEje = ((pollEje * (limites[idx].Cen - 0)) / ancho1);
					}
				}
				else
				{
					if (ancho2 != (limites[idx].Rango - limites[idx].Cen))
					{
						if (pollEje >= limites[idx].Der) { pollEje = limites[idx].Der; }
						pollEje -= (limites[idx].Cen + limites[idx].Nulo);
						pollEje = limites[idx].Cen + ((pollEje * (limites[idx].Rango - limites[idx].Cen)) / ancho2);
					}
				}
			}
		}

		datosHID->Ejes[idx] = pollEje;
	}
}
