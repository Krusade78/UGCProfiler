#include "../../framework.h"
#include "CTeclado.h"

void CTeclado::Procesar(PEV_COMANDO pComando, CVirtualHID* pVHid)
{
	bool soltar = ((pComando->Tipo & TipoComando::Soltar) == TipoComando::Soltar);

    INPUT ip;
    RtlZeroMemory(&ip, sizeof(INPUT));
    UINT sc = GetExtendida(pComando->Dato);
        ip.type = INPUT_KEYBOARD;
        ip.ki.wVk = pComando->Dato;
        ip.ki.wScan = sc & 0xff;
        ip.ki.dwFlags = 8 |(soltar ? KEYEVENTF_KEYUP : 0) | ((sc & 0xff00) ? KEYEVENTF_EXTENDEDKEY : 0);
    SendInput(1, &ip, sizeof(INPUT));
}

UINT CTeclado::GetExtendida(UCHAR tecla)
{
    UINT sc = MapVirtualKey(tecla, MAPVK_VK_TO_VSC_EX) & 0xff;
    if (tecla == 7)
    {
        return 0xe01c;
    }
    else if ((tecla == VK_RCONTROL) || (tecla == VK_RMENU)
        || (tecla == VK_INSERT) || (tecla == VK_DELETE) || (tecla == VK_END) || (tecla == VK_HOME) || (tecla == VK_PRIOR) || (tecla == VK_NEXT)
        || (tecla == VK_LEFT) || (tecla == VK_UP) || (tecla == VK_RIGHT) || (tecla == VK_DOWN)
        || (tecla == VK_NUMLOCK)
        || (tecla == VK_SNAPSHOT) || (tecla == VK_CANCEL)
        || (tecla == VK_DIVIDE) || (tecla == 7))
    {
        sc |= 0xe000;
    }

    return sc;
}