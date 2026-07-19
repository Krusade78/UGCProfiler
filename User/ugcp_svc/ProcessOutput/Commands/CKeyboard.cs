namespace ugcp_svc.ProcessOutput.Commands
{
    static class CKeyboard
    {
        public static void Process(ref EventQueue.EV_COMMAND pCommand)
        {
            bool release = ((pCommand.Type & EventQueue.CommandType.Release) == EventQueue.CommandType.Release);

            API.Win32.INPUT[] ip = new API.Win32.INPUT[1];
            uint sc = GetExtended(pCommand.Data.Basic.Data1);
            ip[0].type = API.Win32.InputType.Keyboard;
            //ip.ki.wVk = pCommand->Basic.Data;
            ip[0].U.ki.wScan = (ushort)(sc & 0xff);
            ip[0].U.ki.dwFlags = API.Win32.KeyEventFlags.SCANCODE | (release ? API.Win32.KeyEventFlags.KEYUP : 0) | ((sc & 0xff00) != 0 ? API.Win32.KeyEventFlags.EXTENDEDKEY : 0);
            API.Win32.SendInput(1, ip, System.Runtime.InteropServices.Marshal.SizeOf<API.Win32.INPUT>());
        }

        private static uint GetExtended(byte key)
        {
            uint sc = API.Win32.MapVirtualKeyW(key, API.Win32.MAPVK_VK_TO_VSC_EX) & 0xff;
            if (key == 7)
            {
                return 0xe01c;
            }
            else if ((key == (byte)API.Win32.VirtualKeys.VK_RCONTROL) || (key == (byte)API.Win32.VirtualKeys.VK_RMENU)
                || (key == (byte)API.Win32.VirtualKeys.VK_INSERT) || (key == (byte)API.Win32.VirtualKeys.VK_DELETE) || (key == (byte)API.Win32.VirtualKeys.VK_END) || (key == (byte)API.Win32.VirtualKeys.VK_HOME) || (key == (byte)API.Win32.VirtualKeys.VK_PRIOR) || (key == (byte)API.Win32.VirtualKeys.VK_NEXT)
                || (key == (byte)API.Win32.VirtualKeys.VK_LEFT) || (key == (byte)API.Win32.VirtualKeys.VK_UP) || (key == (byte)API.Win32.VirtualKeys.VK_RIGHT) || (key == (byte)API.Win32.VirtualKeys.VK_DOWN)
                || (key == (byte)API.Win32.VirtualKeys.VK_NUMLOCK)
                || (key == (byte)API.Win32.VirtualKeys.VK_SNAPSHOT) || (key == (byte)API.Win32.VirtualKeys.VK_CANCEL)
                || (key == (byte)API.Win32.VirtualKeys.VK_DIVIDE) || (key == 7))
            {
                sc |= 0xe000;
            }

            return sc;
        }
    }
}
