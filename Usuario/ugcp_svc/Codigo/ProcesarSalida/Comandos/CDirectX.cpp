#include "../../framework.h"
#include "CDirectX.h"

void CDirectX::Posicion(PEV_COMANDO pComando, CVirtualHID* pVHid)
{
	if (pComando->VHid.JoyId < 100)
	{
		for (char i = 0; i < 8; i++)
		{
			if ((pComando->VHid.Mapa >> i) & 1)
			{
				pVHid->Estado.DirectX[pComando->VHid.JoyId].Ejes[i] = pComando->VHid.Datos.Ejes[i];
			}
		}
	}
	else
	{
		pComando->VHid.JoyId -= 100;
		RtlCopyMemory(&pVHid->Estado.DirectX[pComando->VHid.JoyId], &pComando->VHid.Datos, sizeof(VHID_INPUT_DATA));
	}
	pVHid->EnviarRequestJoystick(pComando->VHid.JoyId, &pVHid->Estado.DirectX[pComando->VHid.JoyId]);
}

void CDirectX::Botones_Setas(PEV_COMANDO comando, CVirtualHID* pVHid)
{
	bool soltar = ((comando->Tipo & TipoComando::Soltar) == TipoComando::Soltar);

	if ((comando->Tipo & 0x7f) == TipoComando::DxBoton) // Botón DX
	{
		if (!soltar)
			pVHid->Estado.DirectX[comando->Dato & 7].Botones[(comando->Dato >> 3) / 8] |= 1 << ((comando->Dato >> 3) % 8);
		else
			pVHid->Estado.DirectX[comando->Dato & 7].Botones[(comando->Dato >> 3) / 8] &= ~(1 << ((comando->Dato >> 3) % 8));
	}
	else if ((comando->Tipo & 0x7f) == TipoComando::DxSeta) // Seta DX
	{
		if (!soltar)
			pVHid->Estado.DirectX[comando->Dato & 7].Setas[(comando->Dato >> 3) / 8] = ((comando->Dato >> 3) % 8) + 1;
		else
			pVHid->Estado.DirectX[comando->Dato & 7].Setas[(comando->Dato >> 3) / 8] = 0;
	}

	pVHid->EnviarRequestJoystick(comando->Dato & 7, &pVHid->Estado.DirectX[comando->Dato & 7]);
}
