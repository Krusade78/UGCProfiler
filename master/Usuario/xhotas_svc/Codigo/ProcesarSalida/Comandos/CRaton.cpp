#include "../../framework.h"
#include "CRaton.h"

bool CRaton::EnviarSalida(CVirtualHID* pVHid, TipoComando cmd, bool ejeX, bool ejeY)
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

	INPUT ip;
	RtlZeroMemory(&ip, sizeof(INPUT));
	ip.type = INPUT_MOUSE;
	if ((cmd == TipoComando::RatonIzq) || (cmd == TipoComando::RatonDer))
	{
		ip.mi.dx = static_cast<CHAR>(buffer[1]);
		ip.mi.dwFlags = MOUSEEVENTF_MOVE;
	}
	else if ((cmd == TipoComando::RatonArr) || (cmd == TipoComando::RatonAba))
	{
		ip.mi.dy = static_cast<CHAR>(buffer[2]);
		ip.mi.dwFlags = MOUSEEVENTF_MOVE;
	}
	else if ((cmd == TipoComando::RatonWhArr) || (cmd == TipoComando::RatonWhAba))
	{
		ip.mi.mouseData= buffer[3];
		ip.mi.dwFlags = MOUSEEVENTF_WHEEL;
	}
	else if (cmd == TipoComando::RatonBt1)
	{
		ip.mi.dwFlags = (buffer[0] & 1) ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
	}
	else if (cmd == TipoComando::RatonBt2)
	{
		ip.mi.dwFlags = (buffer[0] & 2) ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
	}
	else
	{
		ip.mi.dwFlags = (buffer[0] & 4) ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
	}
	SendInput(1, &ip, sizeof(INPUT));

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
		TipoComando cmd; cmd = comando->Tipo & 0x7f;
		*setTimer = EnviarSalida(pVHid, cmd, ejeX, ejeY);
	}

	return procesado;
}
