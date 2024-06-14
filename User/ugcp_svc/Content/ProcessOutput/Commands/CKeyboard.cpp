#include "../../framework.h"
#include "CKeyboard.h"

void CKeyboard::Processed(PEV_COMMAND pCommand, CVirtualHID* pVHid)
{
	bool release = ((pCommand->Type & CommandType::Release) == CommandType::Release);

    INPUT ip;
    RtlZeroMemory(&ip, sizeof(INPUT));
    UINT sc = GetExtended(pCommand->Basic.Data);
        ip.type = INPUT_KEYBOARD;
        //ip.ki.wVk = pCommand->Basic.Data;
        ip.ki.wScan = sc & 0xff;
        ip.ki.dwFlags = KEYEVENTF_SCANCODE |(release ? KEYEVENTF_KEYUP : 0) | ((sc & 0xff00) ? KEYEVENTF_EXTENDEDKEY : 0);
    SendInput(1, &ip, sizeof(INPUT));
}

UINT CKeyboard::GetExtended(UCHAR key)
{
    UINT sc = MapVirtualKey(key, MAPVK_VK_TO_VSC_EX) & 0xff;
    if (key == 7)
    {
        return 0xe01c;
    }
    else if ((key == VK_RCONTROL) || (key == VK_RMENU)
        || (key == VK_INSERT) || (key == VK_DELETE) || (key == VK_END) || (key == VK_HOME) || (key == VK_PRIOR) || (key == VK_NEXT)
        || (key == VK_LEFT) || (key == VK_UP) || (key == VK_RIGHT) || (key == VK_DOWN)
        || (key == VK_NUMLOCK)
        || (key == VK_SNAPSHOT) || (key == VK_CANCEL)
        || (key == VK_DIVIDE) || (key == 7))
    {
        sc |= 0xe000;
    }

    return sc;
}