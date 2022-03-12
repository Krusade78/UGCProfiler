#include "../../framework.h"
#include "CTeclado.h"

void CTeclado::Procesar(PEV_COMANDO pComando, CVirtualHID* pVHid)
{
	bool soltar = ((pComando->Tipo & TipoComando::Soltar) == TipoComando::Soltar);

    INPUT ip;
    RtlZeroMemory(&ip, sizeof(INPUT));
    UINT sc = MapVirtualKey(pComando->Dato, MAPVK_VK_TO_VSC_EX);
        ip.type = INPUT_KEYBOARD;
        ip.ki.wVk = pComando->Dato;
        ip.ki.wScan = sc & 0xff;
        ip.ki.dwFlags = 8 |(soltar ? KEYEVENTF_KEYUP : 0) | ((sc && 0xff00) ? KEYEVENTF_EXTENDEDKEY : 0);
    SendInput(1, &ip, sizeof(INPUT));
}