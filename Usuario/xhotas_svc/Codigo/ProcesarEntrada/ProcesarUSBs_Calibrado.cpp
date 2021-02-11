#include "../framework.h"
#include "ProcesarUSBs_Calibrado.h"

void CCalibrado::Calibrar(CPerfil* pPerfil, TipoJoy tipoJ, PVHID_INPUT_DATA datosHID)
{
	const BYTE mapaEjesUsados[4] = { 0b11001000, 0b00001011, 0b01011111, 0b11111111 }; //Pedales, X52_Joy, X52_Ace, NXT
	const INT16 mapaRangos[4][8] = {
		{0,0,0,255,0,0,63,63},
		{1023,1023,0,511,0,0,0,0},
		{0,0,127,127,127,127,7,7},
		{32767,32767,32767,32767,32767,32767,127,127} };

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
		if (!((mapaEjesUsados[tipo] >> idx) & 1))
		{
			continue;
		}
		LONG pollEje = static_cast<LONG>(datosHID->Ejes[idx]);

		if (jitter[idx].Antiv)
		{
			// Antivibraciones
			if ((pollEje < (jitter[idx].PosElegida - jitter[idx].Margen)) || (pollEje > (jitter[idx].PosElegida + jitter[idx].Margen)))
			{
				jitter[idx].PosRepetida = 0;
				jitter[idx].PosElegida = static_cast<INT16>(pollEje);
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
					jitter[idx].PosElegida = static_cast<INT16>(pollEje);
				}
			}
		}

		if (limites[idx].Cal)
		{
			// Calibrado
			INT16 ancho1, ancho2;
			ancho1 = -limites[idx].Izq + (limites[idx].Cen - limites[idx].Nulo) ;
			ancho2 = limites[idx].Der - (limites[idx].Cen + limites[idx].Nulo);
			if (((pollEje >= (limites[idx].Cen - limites[idx].Nulo)) && (pollEje <= (limites[idx].Cen + limites[idx].Nulo))))
			{
				//Zona nula
				datosHID->Ejes[idx] = 0;
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
					pollEje -= (limites[idx].Cen - limites[idx].Nulo);
					if (ancho1 != mapaRangos[tipo][idx])
					{
						pollEje = ((pollEje * mapaRangos[tipo][idx]) / ancho1);
					}
				}
				else
				{
					pollEje -= (limites[idx].Cen + limites[idx].Nulo);
					if (ancho2 != mapaRangos[tipo][idx])
					{
						pollEje = ((pollEje * mapaRangos[tipo][idx]) / ancho2);
					}
				}
			}
		}

		datosHID->Ejes[idx] = (INT16)pollEje;
	}
}
