using System;
using ugcp_svc.EventQueue;

namespace ugcp_svc.ProcessOutput.Commands
{
    static class CMouse
    {
        private static readonly System.Threading.Lock mutex = new();

        public static System.Threading.Lock GetLock() => mutex;

        public static bool Process(CVirtualHID pVHid, ref EV_COMMAND command, ref bool setTimer)
        {
            bool release = ((command.Type & CommandType.Release) == CommandType.Release);
            bool processed = true;
            bool axisX = false, axisY = false;

            lock (GetLock())
            {
                if ((command.Type & 0x7f) == CommandType.MouseBt1)
                {
                    if (!release)
                        pVHid.GetStatus().Mouse.Buttons |= 1;
                    else
                        pVHid.GetStatus().Mouse.Buttons &= 254;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseBt2)
                {
                    if (!release)
                        pVHid.GetStatus().Mouse.Buttons |= 2;
                    else
                        pVHid.GetStatus().Mouse.Buttons &= 253;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseBt3)
                {
                    if (!release)
                        pVHid.GetStatus().Mouse.Buttons |= 4;
                    else
                        pVHid.GetStatus().Mouse.Buttons &= 251;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseLeft) //Axis -x
                {
                    axisX = true;
                    if (!release)
                        pVHid.GetStatus().Mouse.X = (sbyte)-command.Data.Basic.Data1;
                    else
                        pVHid.GetStatus().Mouse.X = 0;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseRight) //Axis x
                {
                    axisX = true;
                    if (!release)
                        pVHid.GetStatus().Mouse.X = (sbyte)command.Data.Basic.Data1;
                    else
                        pVHid.GetStatus().Mouse.X = 0;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseUp) //Axis -y
                {
                    axisY = true;
                    if (!release)
                        pVHid.GetStatus().Mouse.Y = (sbyte)-command.Data.Basic.Data1;
                    else
                        pVHid.GetStatus().Mouse.Y = 0;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseDown) //Axis y
                {
                    axisY = true;
                    if (!release)
                        pVHid.GetStatus().Mouse.Y = (sbyte)command.Data.Basic.Data1;
                    else
                        pVHid.GetStatus().Mouse.Y = 0;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseWhUp) // Wheel up
                {
                    if (!release)
                        pVHid.GetStatus().Mouse.Wheel = 127;
                    else
                        pVHid.GetStatus().Mouse.Wheel = 0;
                }
                else if ((command.Type & 0x7f) == CommandType.MouseWhDown) // Wheel down
                {
                    if (!release)
                        pVHid.GetStatus().Mouse.Wheel = -127;
                    else
                        pVHid.GetStatus().Mouse.Wheel = 0;
                }
                else
                {
                    processed = false;
                }
            }

            if (processed)
            {
                CommandType cmd; cmd = command.Type & 0x7f;
                setTimer = SendOutput(pVHid, cmd, axisX, axisY);
            }

            return processed;
        }

        private static bool SendOutput(CVirtualHID pVHid, EventQueue.CommandType cmd, bool axisX, bool axisY)
        {
            Span<byte> buffer = [0, 0, 0, 0];
            bool mouseOn;

            lock (GetLock())
            {
                Span<byte> src = System.Runtime.InteropServices.MemoryMarshal.AsBytes(System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref pVHid.GetStatus().Mouse, 1));
                src.CopyTo(buffer);
                if (!axisX) buffer[1] = 0;
                if (!axisY) buffer[2] = 0;
                mouseOn = ((pVHid.GetStatus().Mouse.X != 0) || (pVHid.GetStatus().Mouse.Y != 0));
            }

            API.Win32.INPUT[] ip = new API.Win32.INPUT[1];
            ip[0].type = API.Win32.InputType.Mouse;
            if ((cmd == CommandType.MouseLeft) || (cmd == CommandType.MouseRight))
            {
                ip[0].U.mi.dx = (sbyte)buffer[1];
                ip[0].U.mi.dwFlags = API.Win32.MouseEventFlags.MOVE;
            }
            else if ((cmd == CommandType.MouseUp) || (cmd == CommandType.MouseDown))
            {
                ip[0].U.mi.dy = (sbyte)buffer[2];
                ip[0].U.mi.dwFlags = API.Win32.MouseEventFlags.MOVE;
            }
            else if ((cmd == CommandType.MouseWhUp) || (cmd == CommandType.MouseWhDown))
            {
                ip[0].U.mi.mouseData = buffer[3];
                ip[0].U.mi.dwFlags = API.Win32.MouseEventFlags.WHEEL;
            }
            else if (cmd == CommandType.MouseBt1)
            {
                ip[0].U.mi.dwFlags = (buffer[0] & 1) != 0 ? API.Win32.MouseEventFlags.LEFTDOWN : API.Win32.MouseEventFlags.LEFTUP;
            }
            else if (cmd == CommandType.MouseBt2)
            {
                ip[0].U.mi.dwFlags = (buffer[0] & 2) != 0 ? API.Win32.MouseEventFlags.RIGHTDOWN : API.Win32.MouseEventFlags.RIGHTUP;
            }
            else
            {
                ip[0].U.mi.dwFlags = (buffer[0] & 4) != 0 ? API.Win32.MouseEventFlags.MIDDLEDOWN : API.Win32.MouseEventFlags.MIDDLEUP;
            }
            API.Win32.SendInput(1, ip, System.Runtime.InteropServices.Marshal.SizeOf<API.Win32.INPUT>());

            return mouseOn;
        }
    }
}
