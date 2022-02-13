#include "../../framework.h"
#include "CTeclado.h"

void CTeclado::Procesar(PEV_COMANDO pComando, CVirtualHID* pVHid)
{
	bool soltar = ((pComando->Tipo & TipoComando::Soltar) == TipoComando::Soltar);
	if (!soltar)
		pVHid->Estado.Teclado[pComando->Dato / 8] |= 1 << (pComando->Dato % 8);
	else
		pVHid->Estado.Teclado[pComando->Dato / 8] &= ~(1 << (pComando->Dato % 8));

	pVHid->EnviarRequestTeclado();
}