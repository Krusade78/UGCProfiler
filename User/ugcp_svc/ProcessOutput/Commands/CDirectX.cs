using ugcp_svc.EventQueue;

namespace ugcp_svc.ProcessOutput.Commands
{
    static class CDirectX
    {
        public static void Position(ref EV_COMMAND pCommand, CVirtualHID pVHid)
        {
            if (pCommand.Data.VHid.OutputJoyId < 100)
            {
                for (byte i = 0; i < 15; i++)
                {
                    if (((pCommand.Data.VHid.Map >> i) & 1) != 0)
                    {
                        pVHid.GetStatus().DirectX[pCommand.Data.VHid.OutputJoyId].Axes[i] = pCommand.Data.VHid.Data.Axes[i];
                    }
                }
            }
            else
            {
                pCommand.Data.VHid.OutputJoyId -= 100;
                pVHid.GetStatus().DirectX[pCommand.Data.VHid.OutputJoyId] = pCommand.Data.VHid.Data;
            }
            pVHid.SendRequestToJoystick(pCommand.Data.VHid.OutputJoyId);
        }

        public static void Buttons_Hats(ref EV_COMMAND pCommand, CVirtualHID pVHid)
        {
            bool release = ((pCommand.Type & CommandType.Release) == CommandType.Release);

            if ((pCommand.Type & 0x7f) == CommandType.DxButton) // Button DX
            {
                if (!release)
                    pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Buttons[pCommand.Data.Basic.Data1 / 8] |= (byte)(1 << (pCommand.Data.Basic.Data1 % 8));
                else
                    pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Buttons[pCommand.Data.Basic.Data1 / 8] &= (byte)~(1 << (pCommand.Data.Basic.Data1 % 8));
            }
            else if ((pCommand.Type & 0x7f) == CommandType.DxHat) // Hat DX
            {
                if (!release)
                    pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Hats[pCommand.Data.Basic.Data2] = (byte)(pCommand.Data.Basic.Data1 + 1);
                else
                    pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Hats[pCommand.Data.Basic.Data2] = 0;
            }

            pVHid.SendRequestToJoystick(pCommand.Data.Basic.OutputJoy);
        }

        public static void Axis(ref EV_COMMAND pCommand, CVirtualHID pVHid)
        {
            byte axis = (byte)(pCommand.Data.Basic.Extra >> 1);
            short value = (short)(pCommand.Data.Basic.Data1 | (pCommand.Data.Basic.Data2 << 8));
            short move = (pCommand.Data.Basic.Extra & 1) != 0 ? (short)-value : value;
            if ((pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Axes[axis] + move) > 32767)
            {
                pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Axes[axis] = 32767;
            }
            else if ((pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Axes[axis] + move) < 0)
            {
                pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Axes[axis] = 0;
            }
            else
            {
                pVHid.GetStatus().DirectX[pCommand.Data.Basic.OutputJoy].Axes[axis] += (ushort)move;
            }

            pVHid.SendRequestToJoystick(pCommand.Data.Basic.OutputJoy);
        }
    }
}
