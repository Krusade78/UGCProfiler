#include "../../framework.h"
#include "CNXT.h"
#include "../../NXT/EscribirHIDNXT.h"

/// <summary>
/// Comandos del Gladiator NXT
/// </summary>
/// <returns><para>TRUE: procesado y continuar</para><para>FALSE: no procesado</para></returns>
bool CNXT::Procesar(CPaqueteEvento* cola)
{
	bool procesado = true;
	PEV_COMANDO comando = cola->GetColaComandos()->front();

	if (comando->Tipo.Get() == TipoComando::NxtLeds)
	{
		UCHAR params[4];
		params[0] = comando->Dato;

		std::deque<PEV_COMANDO>::iterator pos = cola->GetColaComandos()->begin();
		params[1] = (*(++pos))->Dato;
		delete (*pos);
		cola->GetColaComandos()->erase(pos);

		pos = cola->GetColaComandos()->begin();
		params[2] = (*(++pos))->Dato;
		delete (*pos);
		cola->GetColaComandos()->erase(pos);

		pos = cola->GetColaComandos()->begin();
		params[3] = (*(++pos))->Dato;
		delete (*pos);
		cola->GetColaComandos()->erase(pos);

		if (CNXTSalida::Get() != nullptr) CNXTSalida::Get()->SetLed(params);
	}
	else
	{
		procesado = false;
	}

	return procesado;
}