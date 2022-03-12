#include "../framework.h"
#include "CVirtualHID.h"
//#include "../Perfil/CPerfil.h"
#include "../vJoy/vjoyinterface.h"

CVirtualHID::CVirtualHID()
{
   RtlZeroMemory(&Estado, sizeof(Estado));
   hMutextRaton = CreateSemaphore(NULL, 1, 1, NULL);
}

CVirtualHID::~CVirtualHID()
{
    if (reportOk)
    {
        reportOk = false;
        RelinquishVJD(3);
        RelinquishVJD(2);
        RelinquishVJD(1);
    }
    CloseHandle(hMutextRaton);
}

bool CVirtualHID::Iniciar()
{
    if (!vJoyEnabled())
    {
        return false;
    }
    if ((GetVJDStatus(1) != VJD_STAT_FREE) || (GetVJDStatus(2) != VJD_STAT_FREE) || (GetVJDStatus(3) != VJD_STAT_FREE))
    {
        return false;
    }
    if (AcquireVJD(1))
    {
        if (AcquireVJD(2))
        {
            if (!AcquireVJD(3))
            {
                RelinquishVJD(2);
                RelinquishVJD(1);
                return false;
            }
        }
        else
        {
            RelinquishVJD(1);
            return false;
        }
    }
    else
    {
        return false;
    }

    reportOk = true;
    return true;
}

void CVirtualHID::EnviarRequestJoystick(UCHAR joyId, PVHID_INPUT_DATA inputData)
{
    if (reportOk)
    {
        JOYSTICK_POSITION input;
        RtlZeroMemory(&input, sizeof(JOYSTICK_POSITION));
        input.wAxisX = inputData->Ejes[0];
        input.wAxisY = inputData->Ejes[1];
        input.wAxisZ = inputData->Ejes[2];
        input.wAxisXRot = inputData->Ejes[3];
        input.wAxisYRot = inputData->Ejes[4];
        input.wAxisZRot = inputData->Ejes[5];
        input.wSlider = inputData->Ejes[6];
        input.wDial = inputData->Ejes[7];
        input.lButtons = inputData->Botones[0] | (inputData->Botones[1] << 8) | (inputData->Botones[2] << 16) | (inputData->Botones[3] << 24);
        input.bHats = Hat2Switch(inputData->Setas[0]);
        input.bHatsEx1 = Hat2Switch(inputData->Setas[1]);
        input.bHatsEx2 = Hat2Switch(inputData->Setas[2]);
        input.bHatsEx3 = Hat2Switch(inputData->Setas[3]);
        UpdateVJD(joyId + 1, &input);
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
