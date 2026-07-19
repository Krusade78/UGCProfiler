using System;

namespace ugcp_svc.ProcessOutput
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    struct VHID_INPUT_DATA
    {
        [System.Runtime.CompilerServices.InlineArray(8)]
        public struct AxesArray { private ushort _axis; }

        [System.Runtime.CompilerServices.InlineArray(4)]
        public struct HatsArray { private byte _hat; }

        [System.Runtime.CompilerServices.InlineArray(16)]
        public struct ButtonsArray { private byte _button; }

        public AxesArray Axes;
        public HatsArray Hats;
        public ButtonsArray Buttons;
    };

    sealed class CVirtualHID : IDisposable
    {
        public struct ST_MOUSE
        {
            public byte Buttons;
            public sbyte X;
            public sbyte Y;
            public sbyte Wheel;
        }
        public class ST_STATUS
        {
            public ST_MOUSE Mouse = new();
            public byte[] Keyboard = new byte[29];
            public VHID_INPUT_DATA[] DirectX = new VHID_INPUT_DATA[16];

            public void Reset()
            {
                Mouse = new();
                Keyboard = new byte[29];
                DirectX = new VHID_INPUT_DATA[16];
            }
        }

        private bool initialized = false;
        private readonly bool[] available = new bool[16];
        private readonly ST_STATUS status = new();

        public CVirtualHID()
        {
            for (byte i = 0; i < 16; i++)
            {
                available[i] = API.Vjoy.isVJDExists((uint)(i + 1));
            }
        }

        void IDisposable.Dispose()
        {
            for (byte i = 0; i < 16; i++)
            {
                if (available[i] && (API.Vjoy.GetVJDStatus((uint)(i + 1)) == API.Vjoy.VjdStat.VJD_STAT_OWN)) { API.Vjoy.RelinquishVJD((uint)(i + 1)); }
            }
        }

        public ST_STATUS GetStatus() => status;

        public bool Init()
        {
            if (!initialized)
            {
                if (!API.Vjoy.vJoyEnabled())
                {
                    return false;
                }
                initialized = true;
            }

            bool ok = true;
            for (byte i = 0; i < 16; i++)
            {
                if (available[i] && (API.Vjoy.GetVJDStatus((uint)(i + 1)) != API.Vjoy.VjdStat.VJD_STAT_OWN))
                {
                    ok = false;
                    break;
                }
            }
            if (!ok)
            {
                for (byte i = 0; i < 16; i++)
                {
                    if (available[i] && (API.Vjoy.GetVJDStatus((uint)(i + 1)) == API.Vjoy.VjdStat.VJD_STAT_OWN)) { API.Vjoy.RelinquishVJD((uint)(i + 1)); }
                    if (available[i] && (API.Vjoy.GetVJDStatus((uint)(i + 1)) != API.Vjoy.VjdStat.VJD_STAT_FREE))
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
            for (byte i = 0; i < 16; i++)
            {
                if (available[i] && !API.Vjoy.AcquireVJD((uint)(i + 1))) { ok = false; break; }
            }
            if (!ok)
            {
                for (byte i = 0; i >= 0; i--)
                {
                    if (available[i]) { API.Vjoy.RelinquishVJD((uint)(i + 1)); }
                }
                return false;
            }

            return true;
        }

        public void SendRequestToJoystick(byte joyId)
        {
            if (Init())
            {
                API.Vjoy.JOYSTICK_POSITION_V2 input = new()
                {
                    wAxisX = status.DirectX[joyId].Axes[0],
                    wAxisY = status.DirectX[joyId].Axes[1],
                    wAxisZ = status.DirectX[joyId].Axes[2],
                    wAxisXRot = status.DirectX[joyId].Axes[3],
                    wAxisYRot = status.DirectX[joyId].Axes[4],
                    wAxisZRot = status.DirectX[joyId].Axes[5],
                    wSlider = status.DirectX[joyId].Axes[6],
                    wDial = status.DirectX[joyId].Axes[7],
                    lButtons = (uint)(status.DirectX[joyId].Buttons[0] | (status.DirectX[joyId].Buttons[1] << 8) | (status.DirectX[joyId].Buttons[2] << 16) | (status.DirectX[joyId].Buttons[3] << 24)),
                    bHats = Hat2Switch(status.DirectX[joyId].Hats[0]),
                    bHatsEx1 = Hat2Switch(status.DirectX[joyId].Hats[1]),
                    bHatsEx2 = Hat2Switch(status.DirectX[joyId].Hats[2]),
                    bHatsEx3 = Hat2Switch(status.DirectX[joyId].Hats[3]),
                    lButtonsEx1 = status.DirectX[joyId].Buttons[4] | (status.DirectX[joyId].Buttons[5] << 8) | (status.DirectX[joyId].Buttons[6] << 16) | (status.DirectX[joyId].Buttons[7] << 24),
                    lButtonsEx2 = status.DirectX[joyId].Buttons[8] | (status.DirectX[joyId].Buttons[9] << 8) | (status.DirectX[joyId].Buttons[10] << 16) | (status.DirectX[joyId].Buttons[11] << 24),// Buttons 65-96
                    lButtonsEx3 = status.DirectX[joyId].Buttons[12] | (status.DirectX[joyId].Buttons[13] << 8) | (status.DirectX[joyId].Buttons[14] << 16) | (status.DirectX[joyId].Buttons[15] << 24)// Buttons 97-128
                };
                byte vjId = 0;
                for (byte i = 0; i < 16; i++)
                {
                    if (available[i])
                    {
                        if (joyId == vjId)
                        {
                            API.Vjoy.UpdateVJD((uint)(i + 1), ref input);
                            break;
                        }
                        vjId++;
                    }
                }

            }
        }

        private static readonly ushort[] angles = [0, 4500, 9000, 13500, 18000, 22500, 27000, 31500];
        private static uint Hat2Switch(byte pos)
        {
            return pos > 8 ? 0xffffffff : angles[pos];
        }
    }
}
