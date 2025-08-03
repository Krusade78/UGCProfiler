#include "../framework.h"
#include "CVirtualHID.h"
//#include "../Perfil/CPerfil.h"
#include "../vJoy/vjoyinterface.h"

CVirtualHID::CVirtualHID()
{
   RtlZeroMemory(&status, sizeof(ST_STATUS));
   hMutextMouse = CreateSemaphore(NULL, 1, 1, NULL);
   for (char i = 0; i < 16; i++)
   {
       available[i] = isVJDExists(i + 1);
   }
}

CVirtualHID::~CVirtualHID()
{
    for (char i = 0; i < 16; i++)
    {
        if (available[i] && (GetVJDStatus(i + 1) == VJD_STAT_OWN)) { RelinquishVJD(i + 1); }
    }

    CloseHandle(hMutextMouse);
}

bool CVirtualHID::Init()
{
    if (!initialized)
    {
        if (!vJoyEnabled())
        {
            return false;
        }
        initialized = true;
    }

    bool ok = true;
    for (char i = 0; i < 16; i++)
    {
        if (available[i] && (GetVJDStatus(i + 1) != VJD_STAT_OWN))
        {
            ok = false;
            break;
        }
    }
    if (!ok)
    {
        for (char i = 0; i < 16; i++)
        {
            if (available[i] && (GetVJDStatus(i + 1) == VJD_STAT_OWN)) { RelinquishVJD(i + 1); }
            if (available[i] && (GetVJDStatus(i + 1) != VJD_STAT_FREE))
            {
                return false;
            }
        }
    }
    else
    {
        return true;
    }

    ok = true;
    char i = 0;
    for (; i < 16; i++)
    {
        if (available[i] && !AcquireVJD(i + 1)) { ok = false; break; }
    }
    if (!ok)
    {
        for (; i >= 0; i--)
        {
            if (available[i]) { RelinquishVJD(i + 1); }
        }
        return false;
    }

    return true;
}

void CVirtualHID::SendRequestToJoystick(UCHAR joyId)
{
    if (Init())
    {
        JOYSTICK_POSITION_V2 input;
        RtlZeroMemory(&input, sizeof(JOYSTICK_POSITION_V2));

        input.wAxisX = status.DirectX[joyId].Axes[0];
        input.wAxisY = status.DirectX[joyId].Axes[1];
        input.wAxisZ = status.DirectX[joyId].Axes[2];
        input.wAxisXRot = status.DirectX[joyId].Axes[3];
        input.wAxisYRot = status.DirectX[joyId].Axes[4];
        input.wAxisZRot = status.DirectX[joyId].Axes[5];
        input.wSlider = status.DirectX[joyId].Axes[6];
        input.wDial = status.DirectX[joyId].Axes[7];
        input.wWheel = status.DirectX[joyId].Axes[8];
        input.wAxisVX = status.DirectX[joyId].Axes[9];
        input.wAxisVY = status.DirectX[joyId].Axes[10];
        input.wAxisVZ = status.DirectX[joyId].Axes[11];
        input.wAxisVBRX = status.DirectX[joyId].Axes[12];
        input.wAxisVBRY = status.DirectX[joyId].Axes[13];
        input.wAxisVBRZ = status.DirectX[joyId].Axes[14];
        input.lButtons = status.DirectX[joyId].Buttons[0] | (status.DirectX[joyId].Buttons[1] << 8) | (status.DirectX[joyId].Buttons[2] << 16) | (status.DirectX[joyId].Buttons[3] << 24);
        input.bHats = Hat2Switch(status.DirectX[joyId].Hats[0]);
        input.bHatsEx1 = Hat2Switch(status.DirectX[joyId].Hats[1]);
        input.bHatsEx2 = Hat2Switch(status.DirectX[joyId].Hats[2]);
        input.bHatsEx3 = Hat2Switch(status.DirectX[joyId].Hats[3]);
        input.lButtonsEx1 = status.DirectX[joyId].Buttons[4] | (status.DirectX[joyId].Buttons[5] << 8) | (status.DirectX[joyId].Buttons[6] << 16) | (status.DirectX[joyId].Buttons[7] << 24);
        input.lButtonsEx2 = status.DirectX[joyId].Buttons[8] | (status.DirectX[joyId].Buttons[9] << 8) | (status.DirectX[joyId].Buttons[10] << 16) | (status.DirectX[joyId].Buttons[11] << 24);// Buttons 65-96
        input.lButtonsEx3 = status.DirectX[joyId].Buttons[12] | (status.DirectX[joyId].Buttons[13] << 8) | (status.DirectX[joyId].Buttons[14] << 16) | (status.DirectX[joyId].Buttons[15] << 24);// Buttons 97-128
        char vjId = 0;
        for (char i = 0; i < 16; i++)
        {
            if (available[i])
            {
                if (joyId == vjId)
                {
                    UpdateVJD(i + 1, (PVOID)&input);
                    break;
                }
                vjId++;
            }
        }

    }
}

DWORD CVirtualHID::Hat2Switch(UCHAR pos)
{
    switch (pos)
    {
    case 1:
        return 0;
    case 2:
        return 4500;
    case 3:
        return 9000;
    case 4:
        return 13500;
    case 5:
        return 18000;
    case 6:
        return 22500;
    case 7:
        return 27000;
    case 8:
        return 31500;

    default:
        return -1;
    }
}
