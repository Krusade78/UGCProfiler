#include "../../framework.h"
#include "CRaton.h"

bool CRaton::EnviarSalida(CVirtualHID* pVHid, bool ejeX, bool ejeY)
{
	BYTE buffer[4];
	bool ratonOn = FALSE;

	pVHid->LockRaton();
	{
		RtlCopyMemory(buffer, &pVHid->Estado.Raton, 4);
		if (!ejeX) buffer[1] = 0;
		if (!ejeY) buffer[2] = 0;
		ratonOn = ((pVHid->Estado.Raton.X != 0) || (pVHid->Estado.Raton.Y != 0));
	}
	pVHid->UnlockRaton();
	pVHid->EnviarRequestRaton(buffer);
	if (ratonOn)
	{
		return true;
	}

	return false;
}

bool CRaton::Procesar(CVirtualHID* pVHid, PEV_COMANDO comando, bool* setTimer)
{
	bool soltar = ((comando->Tipo & TipoComando::Soltar) == TipoComando::Soltar);
	bool procesado = true;
	bool ejeX = false, ejeY = false;

	pVHid->LockRaton();
	{
		if ((comando->Tipo & 0x7f) == TipoComando::RatonBt1)
		{
			if (!soltar)
				pVHid->Estado.Raton.Botones |= 1;
			else
				pVHid->Estado.Raton.Botones &= 254;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonBt2)
		{
			if (!soltar)
				pVHid->Estado.Raton.Botones |= 2;
			else
				pVHid->Estado.Raton.Botones &= 253;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonBt3)
		{
			if (!soltar)
				pVHid->Estado.Raton.Botones |= 4;
			else
				pVHid->Estado.Raton.Botones &= 251;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonIzq) //Eje -x
		{
			ejeX = true;
			if (!soltar)
				pVHid->Estado.Raton.X = -comando->Dato;
			else
				pVHid->Estado.Raton.X = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonDer) //Eje x
		{
			ejeX = true;
			if (!soltar)
				pVHid->Estado.Raton.X = comando->Dato;
			else
				pVHid->Estado.Raton.X = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonArr) //Eje -y
		{
			ejeY = true;
			if (!soltar)
				pVHid->Estado.Raton.Y = -comando->Dato;
			else
				pVHid->Estado.Raton.Y = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonAba) //Eje y
		{
			ejeY = true;
			if (!soltar)
				pVHid->Estado.Raton.Y = comando->Dato;
			else
				pVHid->Estado.Raton.Y = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonWhArr) // Wheel up
		{
			if (!soltar)
				pVHid->Estado.Raton.Rueda = 127;
			else
				pVHid->Estado.Raton.Rueda = 0;
		}
		else if ((comando->Tipo & 0x7f) == TipoComando::RatonWhAba) // Wheel down
		{
			if (!soltar)
				pVHid->Estado.Raton.Rueda = -127;
			else
				pVHid->Estado.Raton.Rueda = 0;
		}
		else
		{
			procesado = false;
		}
	}
	pVHid->UnlockRaton();

	if (procesado)
	{
		*setTimer = EnviarSalida(pVHid, ejeX, ejeY);		
	}

	return procesado;
}
