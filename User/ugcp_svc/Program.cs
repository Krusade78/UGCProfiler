using System;

namespace ugcp_svc
{
    class Program
    {
        static void Main()
        {
            if (System.Diagnostics.Process.GetProcessesByName("ugcp_svc").Length > 1)
            {
                return;
            }

            LauncherWrapper.Passthrough pExp = new();
            pExp.Init();
            using X52.CX52Write x52Drv = new();
            using NXT.CNXTWrite nxtDrv = new();
            using X52.CMFDMenu mfd = new();
            using Profile.CProfile profile = new();
            using CComs coms = new(profile);
            if (coms.Init())
            {
                using ProcessOutput.CVirtualHID vhid = new();
                if (vhid.Init())
                {
                    using ProcessOutput.CProcessOutput output = new(profile, vhid);
                    using HIDInput.CPnP input = new(profile);
                    if (input.Init())
                    {
                        coms.SetHwndClose(input.GetCloseHwndCallback());
                        pExp.LoadDefault();
                        input.LoopWnd();
                    }
                }
            }
        }
    }
}
