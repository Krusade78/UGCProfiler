#include "../../framework.h"
#include "CX52.h"
#include "../../X52/EscribirUSBX52.h"

/// <summary>
/// Comandos del X52
/// </summary>
/// <returns><para>TRUE: procesado y continuar</para><para>FALSE: no procesado</para></returns>
bool CX52::Procesar(CPaqueteEvento* cola)
{
	bool procesado = true;
	PEV_COMANDO comando = cola->GetColaComandos()->front();

	switch (comando->Tipo.Get())
	{
		case TipoComando::MfdLuz: //mfd_luz
		{
			UCHAR params = comando->Dato;
			CX52Salida::Get()->Luz_MFD(&params);
			break;
		}
		case TipoComando::Luz: // luz
		{
			UCHAR params = comando->Dato;
			CX52Salida::Get()->Luz_Global(&params);
			break;
		}
		case TipoComando::InfoLuz: // info luz
		{
			UCHAR params = comando->Dato;
			CX52Salida::Get()->Luz_Info(&params);
			break;
		}
		case TipoComando::MfdPinkie: // pinkie;
		{
			UCHAR params = comando->Dato;
			CX52Salida::Get()->Set_Pinkie(&params);
			break;
		}
		case TipoComando::MfdTextoIni: // texto
		{
			UCHAR texto[17];
			UCHAR tam = 0;
			RtlZeroMemory(texto, 17);

			texto[0] = comando->Dato; //linea
			delete comando;
			cola->GetColaComandos()->pop_front();

			while (cola->GetColaComandos()->size() != 1)
			{
				std::deque<PEV_COMANDO>::iterator pos = cola->GetColaComandos()->begin();
				PEV_COMANDO comTxt = *++pos;
				if (comTxt->Tipo == TipoComando::MfdTextoFin) // fin texto
				{
					break;
				}
				texto[tam] = comTxt->Dato;
				delete comTxt;
				cola->GetColaComandos()->erase(pos);

				tam++;
				if (tam == 18)
				{
					throw new std::exception("Error buffer de texto");
				}
			}
			CX52Salida::Get()->Set_Texto(texto, tam);
			break;
		}
		case TipoComando::MfdHora: // hora
		{
			UCHAR params[3];
			params[0] = comando->Dato;

			std::deque<PEV_COMANDO>::iterator pos = cola->GetColaComandos()->begin();
			params[1] = (*++pos)->Dato;
			delete (*pos);
			cola->GetColaComandos()->erase(pos);

			pos = cola->GetColaComandos()->begin();
			params[2] = (*++pos)->Dato;
			delete (*pos);
			cola->GetColaComandos()->erase(pos);

			CX52Salida::Get()->Set_Hora(params);
			break;
		}
		case TipoComando::MfdHora24: // hora 24
		{
			UCHAR params[3];
			params[0] = comando->Dato;

			std::deque<PEV_COMANDO>::iterator pos = cola->GetColaComandos()->begin();
			params[1] = (*++pos)->Dato;
			delete (*pos);
			cola->GetColaComandos()->erase(pos);

			pos = cola->GetColaComandos()->begin();
			params[2] = (*++pos)->Dato;
			delete (*pos);
			cola->GetColaComandos()->erase(pos);

			CX52Salida::Get()->Set_Hora24(params);
			break;
		}
		case TipoComando::MfdFecha: // fecha
		{
			UCHAR params[2];
			params[0] = comando->Dato;

			std::deque<PEV_COMANDO>::iterator pos = cola->GetColaComandos()->begin();
			params[1] = (*++pos)->Dato;
			delete (*pos);
			cola->GetColaComandos()->erase(pos);

			CX52Salida::Get()->Set_Fecha(params);
			break;
		}
		default:
			procesado = false;
			break;
	}

	return procesado;
}
