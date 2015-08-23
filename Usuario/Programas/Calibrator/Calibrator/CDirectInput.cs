using Microsoft.DirectX.DirectInput;
using System.Collections.Generic;

namespace Calibrator
{
    class CDirectInput
    {
        private Device g_pJoystick = null;

        public static string GetTipo()
        {
            string tipo = "";
            foreach (DeviceInstance di in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
            {
                switch (di.ProductName)
                {
                    case "Saitek X36+X35 gameport":
                    case "Saitek X36 gameport":
                    case "Saitek X36 USB":
                    case "Controlador virtual XHOTAS":
                    case "Saitek X45":
                    case "Saitek X52":
                        tipo = di.ProductName;
                        break;
                }
            }
            return tipo;
        }

        public CDirectInput()
        {
        }

        ~CDirectInput()
        {
            if (g_pJoystick != null) g_pJoystick.Dispose();
        }

        public bool AbrirDirectInput(System.Windows.Window hWnd, string deviceName)
        {
            foreach (DeviceInstance di in Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly))
            {
                if (di.ProductName == deviceName)
                {
                    g_pJoystick = new Device(di.InstanceGuid);
                    g_pJoystick.SetDataFormat(DeviceDataFormat.Joystick);
                    //g_pJoystick.Properties.SetApplicationData(ParameterHow.ByDevice, 0, 1);
                    g_pJoystick.SetCooperativeLevel(new System.Windows.Interop.WindowInteropHelper(hWnd).Handle, CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Background);
                    g_pJoystick.Properties.AxisModeAbsolute = true;

                    return true;
                }
            }

            return false;
        }

        public bool GetEstado(ref JoystickState joyData)
        {
            if (g_pJoystick == null) return false;

            try
            {
                g_pJoystick.Acquire();
                g_pJoystick.Poll();
                joyData = g_pJoystick.CurrentJoystickState;
                g_pJoystick.Unacquire();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
