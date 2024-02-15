#include "../../framework.h"
#include "CMouse.h"

bool CMouse::SendOutput(CVirtualHID* pVHid, CommandType cmd, bool axisX, bool axisY)
{
	BYTE buffer[4];
	bool mouseOn = FALSE;

	pVHid->LockMouse();
	{
		RtlCopyMemory(buffer, &pVHid->GetStatus()->Mouse, 4);
		if (!axisX) buffer[1] = 0;
		if (!axisY) buffer[2] = 0;
		mouseOn = ((pVHid->GetStatus()->Mouse.X != 0) || (pVHid->GetStatus()->Mouse.Y != 0));
	}
	pVHid->UnlockMouse();

	INPUT ip;
	RtlZeroMemory(&ip, sizeof(INPUT));
	ip.type = INPUT_MOUSE;
	if ((cmd == CommandType::MouseLeft) || (cmd == CommandType::MouseRight))
	{
		ip.mi.dx = static_cast<CHAR>(buffer[1]);
		ip.mi.dwFlags = MOUSEEVENTF_MOVE;
	}
	else if ((cmd == CommandType::MouseUp) || (cmd == CommandType::MouseDown))
	{
		ip.mi.dy = static_cast<CHAR>(buffer[2]);
		ip.mi.dwFlags = MOUSEEVENTF_MOVE;
	}
	else if ((cmd == CommandType::MouseWhUp) || (cmd == CommandType::MouseWhDown))
	{
		ip.mi.mouseData= buffer[3];
		ip.mi.dwFlags = MOUSEEVENTF_WHEEL;
	}
	else if (cmd == CommandType::MouseBt1)
	{
		ip.mi.dwFlags = (buffer[0] & 1) ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
	}
	else if (cmd == CommandType::MouseBt2)
	{
		ip.mi.dwFlags = (buffer[0] & 2) ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
	}
	else
	{
		ip.mi.dwFlags = (buffer[0] & 4) ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
	}
	SendInput(1, &ip, sizeof(INPUT));

	if (mouseOn)
	{
		return true;
	}

	return false;
}

bool CMouse::Process(CVirtualHID* pVHid, PEV_COMMAND command, bool* setTimer)
{
	bool release = ((command->Type & CommandType::Release) == CommandType::Release);
	bool processed = true;
	bool axisX = false, axisY = false;

	pVHid->LockMouse();
	{
		if ((command->Type & 0x7f) == CommandType::MouseBt1)
		{
			if (!release)
				pVHid->GetStatus()->Mouse.Buttons |= 1;
			else
				pVHid->GetStatus()->Mouse.Buttons &= 254;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseBt2)
		{
			if (!release)
				pVHid->GetStatus()->Mouse.Buttons |= 2;
			else
				pVHid->GetStatus()->Mouse.Buttons &= 253;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseBt3)
		{
			if (!release)
				pVHid->GetStatus()->Mouse.Buttons |= 4;
			else
				pVHid->GetStatus()->Mouse.Buttons &= 251;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseLeft) //Axis -x
		{
			axisX = true;
			if (!release)
				pVHid->GetStatus()->Mouse.X = -command->Basic.Data;
			else
				pVHid->GetStatus()->Mouse.X = 0;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseRight) //Axis x
		{
			axisX = true;
			if (!release)
				pVHid->GetStatus()->Mouse.X = command->Basic.Data;
			else
				pVHid->GetStatus()->Mouse.X = 0;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseUp) //Axis -y
		{
			axisY = true;
			if (!release)
				pVHid->GetStatus()->Mouse.Y = -command->Basic.Data;
			else
				pVHid->GetStatus()->Mouse.Y = 0;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseDown) //Axis y
		{
			axisY = true;
			if (!release)
				pVHid->GetStatus()->Mouse.Y = command->Basic.Data;
			else
				pVHid->GetStatus()->Mouse.Y = 0;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseWhUp) // Wheel up
		{
			if (!release)
				pVHid->GetStatus()->Mouse.Wheel = 127;
			else
				pVHid->GetStatus()->Mouse.Wheel = 0;
		}
		else if ((command->Type & 0x7f) == CommandType::MouseWhDown) // Wheel down
		{
			if (!release)
				pVHid->GetStatus()->Mouse.Wheel = -127;
			else
				pVHid->GetStatus()->Mouse.Wheel = 0;
		}
		else
		{
			processed = false;
		}
	}
	pVHid->UnlockMouse();

	if (processed)
	{
		CommandType cmd; cmd = command->Type & 0x7f;
		*setTimer = SendOutput(pVHid, cmd, axisX, axisY);
	}

	return processed;
}
