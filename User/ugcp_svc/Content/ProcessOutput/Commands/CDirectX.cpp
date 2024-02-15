#include "../../framework.h"
#include "CDirectX.h"

void CDirectX::Position(PEV_COMMAND pCommand, CVirtualHID* pVHid)
{
	if (pCommand->VHid.OutputJoyId < 100)
	{
		for (char i = 0; i < 15; i++)
		{
			if ((pCommand->VHid.Map >> i) & 1)
			{
#pragma warning(disable:6386)
				pVHid->GetStatus()->DirectX[pCommand->VHid.OutputJoyId].Axes[i] = pCommand->VHid.Data.Axes[i];
#pragma warning(default:6386)
			}
		}
	}
	else
	{
		pCommand->VHid.OutputJoyId -= 100;
		RtlCopyMemory(&pVHid->GetStatus()->DirectX[pCommand->VHid.OutputJoyId], &pCommand->VHid.Data, sizeof(VHID_INPUT_DATA));
	}
	pVHid->SendRequestToJoystick(pCommand->VHid.OutputJoyId);
}

void CDirectX::Buttons_Hats(PEV_COMMAND pCommand, CVirtualHID* pVHid)
{
	bool release = ((pCommand->Type & CommandType::Release) == CommandType::Release);

	if ((pCommand->Type & 0x7f) == CommandType::DxButton) // Button DX
	{
		if (!release)
			pVHid->GetStatus()->DirectX[pCommand->Basic.OutputJoy].Buttons[pCommand->Basic.Data / 8] |= 1 << (pCommand->Basic.Data % 8);
		else
			pVHid->GetStatus()->DirectX[pCommand->Basic.OutputJoy].Buttons[pCommand->Basic.Data / 8] &= ~(1 << (pCommand->Basic.Data % 8));
	}
	else if ((pCommand->Type & 0x7f) == CommandType::DxHat) // Hat DX
	{
		if (!release)
			pVHid->GetStatus()->DirectX[pCommand->Basic.OutputJoy].Hats[pCommand->Basic.Data / 8] = (pCommand->Basic.Data % 8) + 1;
		else
			pVHid->GetStatus()->DirectX[pCommand->Basic.OutputJoy].Hats[pCommand->Basic.Data / 8] = 0;
	}

	pVHid->SendRequestToJoystick(pCommand->Basic.OutputJoy);
}
