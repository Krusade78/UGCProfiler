using System;


namespace ugcp_svc.X52
{
    class CMFDMenu : IDisposable
    {
        private enum Button : byte
        {
            Enter = 0,
		    Down,
		    Up
        };
        public class ConfigurationMenuMFD
        {
            public bool IsNXTActivated { get; set; }
            public class SHour
            {
                public short Minutes { get; set; } //horas + minutos (en minutos totales)
                public bool _24h { get; set; }
            }
            public SHour[] Hour { get; set; } = [new SHour(), new SHour(), new SHour()];

            public byte GlobalLight { get; set; }
            public byte MFDLight { get; set; }
            public ConfigurationMenuMFD() { }
        }
        private struct StatusMenuMFD    
        {
            //public bool TimerWaiting;
            public bool Activated;
            //public byte ButtonStatus;

            public bool IsHourActivated;
            public bool IsDateActivated;

            public byte CursorStatus;
            public byte PageStatus;
        }

        private static CMFDMenu? pInstance = null;

        private ConfigurationMenuMFD configurationMFD = new();
        private StatusMenuMFD menuMFD = new();
        private readonly System.Threading.Timer timerHour;
        //private readonly System.Threading.Timer timerMenu;

        public CMFDMenu()
        {
            pInstance = this;
            ReadConfiguration();
            timerHour = new(EvtTickHour, null, 4000, 4000);
            //timerMenu = new(EvtTickMenu);
        }

        void IDisposable.Dispose()
        {
            pInstance = null;
            timerHour.Dispose();
            //timerMenu.Dispose();
        }

        public static CMFDMenu? Get() => pInstance;

        public void SetWelcome()
        {
            byte[] row1 = System.Text.Encoding.ASCII.GetBytes("\x01  Saitek X-52");
            CX52Write.Get()?.Set_Text(row1);
            byte[] row2 = System.Text.Encoding.ASCII.GetBytes("\x02  Driver v11.0");
            CX52Write.Get()?.Set_Text(row2);
            byte[] row3 = System.Text.Encoding.ASCII.GetBytes("\x03 ");
            CX52Write.Get()?.Set_Text(row3);

            CX52Write.Get()?.Light_Global(configurationMFD.GlobalLight);
            CX52Write.Get()?.Light_MFD(configurationMFD.MFDLight);
        }

        public void SetHourActivated(bool onoff) { menuMFD.IsHourActivated = onoff; }
        public void SetDateActivated(bool onoff) { menuMFD.IsDateActivated = onoff; }

        #region Configuration
        private void ReadConfiguration()
        {
            menuMFD.IsHourActivated = true;
            menuMFD.IsDateActivated = true;

            try
            {
                string json = System.IO.File.ReadAllText("mfdconf.dat");
                var rconf = System.Text.Json.JsonSerializer.Deserialize<ConfigurationMenuMFD>(json);
                if (rconf != null)
                {
                    configurationMFD = rconf;
                }
            }
            catch { }
        }

        private void SaveConfiguration()
        { 
            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(configurationMFD);
                System.IO.File.WriteAllText("mfdconf.dat", json);
            }
            catch { }
        }
        #endregion

        private void EvtTickHour(object? state)
        {
            DateTime now = DateTime.Now;

            if (menuMFD.IsDateActivated)
            {
                byte[] bfd = [1, (byte)now.Day];
                CX52Write.Get()?.Set_Date(bfd);
                bfd[0] = 2; bfd[1] = (byte)now.Month;
                CX52Write.Get()?.Set_Date(bfd);
                bfd[0] = 3; bfd[1] = (byte)(now.Year % 100);
                CX52Write.Get()?.Set_Date(bfd);
            }

            if (menuMFD.IsHourActivated)
            {
                now = now.AddMinutes(configurationMFD.Hour[0].Minutes);

                byte[] bf = [1, (byte)now.Hour, (byte)now.Minute];
                if (configurationMFD.Hour[0]._24h)
                {
                    CX52Write.Get()?.Set_Hour24(bf);
                }
                else
                {
                    CX52Write.Get()?.Set_Hour(bf);
                }
            }
        }

        //private void EvtTickMenu(object? state)
        //{
        //    byte param = 1;

        //    menuMFD.Activated = true;
        //    menuMFD.TimerWaiting = false;
        //    CX52Write.Get()?.Light_Info(param);
        //    param = 2;
        //    CX52Write.Get()?.Light_MFD(param);
        //    menuMFD.ButtonStatus = 0;
        //    menuMFD.CursorStatus = 0;
        //    menuMFD.PageStatus = 0;
        //    ShowPage1();
        //}

        #region Menu
        //void CMFDMenu::MenuPressButton(std::uint8_t button)
        //{
        //	if (button == static_cast<std::uint8_t>(Button::Enter))
        //	{
        //		if (!menuMFD.TimerWaiting && !menuMFD.Activated)
        //		{
        //			menuMFD.TimerWaiting = true;
        //			LARGE_INTEGER t{};
        //			t.QuadPart = -30000000LL; //3 segundos
        //			FILETIME timeout{};
        //			timeout.dwHighDateTime = t.HighPart;
        //			timeout.dwLowDateTime = t.LowPart;
        //			SetThreadpoolTimer(timerMenu, &timeout, 0, 0);
        //		}
        //	}
        //	if (menuMFD.Activated)
        //	{
        //		menuMFD.ButtonStatus |= 1u << button;
        //	}
        //}
        //
        //void CMFDMenu::MenuReleaseButton(std::uint8_t button)
        //{
        //	if (button == static_cast<std::uint8_t>(Button::Enter))
        //	{
        //		if (menuMFD.TimerWaiting && !menuMFD.Activated)
        //		{
        //			menuMFD.TimerWaiting = false;
        //			SetThreadpoolTimer(timerMenu, nullptr, 0, 0);
        //		}
        //	}
        //	if (menuMFD.Activated)
        //	{
        //		if ((menuMFD.ButtonStatus >> button) & 1)
        //		{
        //			ChangeStatus(button);
        //		}
        //		menuMFD.ButtonStatus &= ~((std::uint8_t)(1 << button));
        //	}
        //}
        private void ChangeStatus(byte button)
        {
            #region Enter button
            if (button == (byte)Button.Enter)
            {
                switch (menuMFD.PageStatus)
                {
                    case 0: //main
                        switch (menuMFD.CursorStatus)
                        {
                            case 0:
                                menuMFD.CursorStatus = 0;
                                menuMFD.PageStatus = 1;
                                ShowPageOnOff();
                                break;
                            case 1:
                                menuMFD.CursorStatus = 0;
                                menuMFD.PageStatus = 2;
                                ShowPageLight(configurationMFD.GlobalLight);
                                break;
                            case 2:
                                menuMFD.CursorStatus = 0;
                                menuMFD.PageStatus = 3;
                                ShowPageLight(configurationMFD.MFDLight);
                                break;
                            case 3:
                                menuMFD.CursorStatus = 0;
                                menuMFD.PageStatus = 4;
                                ShowPageHour(false, 
                                    (sbyte)(configurationMFD.Hour[0].Minutes / 60),
                                    (byte)((((configurationMFD.Hour[0].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[0].Minutes) % 60),
                                    configurationMFD.Hour[0]._24h);
                                break;
                            case 4:
                                menuMFD.CursorStatus = 0;
                                menuMFD.PageStatus = 5;
                                ShowPageHour(false,
                                    (sbyte)(configurationMFD.Hour[1].Minutes / 60),
                                    (byte)((((configurationMFD.Hour[1].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[1].Minutes) % 60),
                                    configurationMFD.Hour[1]._24h);
                                break;
                            case 5:
                                menuMFD.CursorStatus = 0;
                                menuMFD.PageStatus = 6;
                                ShowPageHour(false,
                                    (sbyte)(configurationMFD.Hour[2].Minutes / 60),
                                    (byte)((((configurationMFD.Hour[2].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[2].Minutes) % 60),
                                    configurationMFD.Hour[2]._24h);
                                break;
                            case 6:
                                CloseMenu();
                                break;
                        }
                        break;
                    case 1: //pedals
                        configurationMFD.IsNXTActivated = (menuMFD.CursorStatus == 0);
                        menuMFD.CursorStatus = 0;
                        menuMFD.PageStatus = 0;
                        ShowPage1();
                        break;
                    case 2: // global light
                        configurationMFD.GlobalLight = menuMFD.CursorStatus;
                        CX52Write.Get()?.Light_Global(configurationMFD.GlobalLight);
                        menuMFD.CursorStatus = 1;
                        menuMFD.PageStatus = 0;
                        ShowPage1();
                        break;
                    case 3: //mfd light
                        configurationMFD.MFDLight = menuMFD.CursorStatus;
                        menuMFD.CursorStatus = 2;
                        menuMFD.PageStatus = 0;
                        ShowPage1();
                        break;
                    case 4: //hour 1
                    case 5: //hour 2
                    case 6: //hour 3
                        if (menuMFD.CursorStatus == 3)
                        {
                            menuMFD.CursorStatus = (byte)(menuMFD.PageStatus - 1);
                            menuMFD.PageStatus = 0;
                            ShowPage1();
                        }
                        else
                        {
                            menuMFD.PageStatus = (byte)(menuMFD.PageStatus * 10 + menuMFD.CursorStatus);
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[0].Minutes / 60),
                                (byte)((((configurationMFD.Hour[0].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[0].Minutes) % 60),
                                configurationMFD.Hour[0]._24h);
                        }
                        break;
                    case 40:
                    case 41:
                    case 42:
                        menuMFD.PageStatus = 4;
                        ShowPageHour(false,
                            (sbyte)(configurationMFD.Hour[0].Minutes / 60),
                            (byte)((((configurationMFD.Hour[0].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[0].Minutes) % 60),
                            configurationMFD.Hour[0]._24h);
                        break;
                    case 50:
                    case 51:
                    case 52:
                        {
                           byte[] buffer = [2, 0, 0];
                            if (configurationMFD.Hour[1].Minutes < 0)
                            {
                                buffer[1] = (byte)(((-configurationMFD.Hour[1].Minutes) >> 8) + 4);
                                buffer[2] = (byte)((-configurationMFD.Hour[1].Minutes) & 0xff);
                            }
                            else
                            {
                                buffer[1] = (byte)(configurationMFD.Hour[1].Minutes >> 8);
                                buffer[2] = (byte)(configurationMFD.Hour[1].Minutes & 0xff);
                            }
                            if (configurationMFD.Hour[1]._24h)
                            {
                                CX52Write.Get()?.Set_Hour24(buffer);
                            }
                            else
                            {
                                CX52Write.Get()?.Set_Hour24(buffer);
                            }

                            menuMFD.PageStatus = 5;
                            ShowPageHour(false,
                                (sbyte)(configurationMFD.Hour[1].Minutes / 60),
                                (byte)((((configurationMFD.Hour[1].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[1].Minutes) % 60),
                                configurationMFD.Hour[1]._24h);
                        }
                        break;
                    case 60:
                    case 61:
                    case 62:
                        {
                            byte[] buffer = [3, 0, 0];
                            if (configurationMFD.Hour[2].Minutes < 0)
                            {
                                buffer[1] = (byte)(((-configurationMFD.Hour[2].Minutes) >> 8) + 4);
                                buffer[2] = (byte)((-configurationMFD.Hour[2].Minutes) & 0xff);
                            }
                            else
                            {
                                buffer[1] = (byte)(configurationMFD.Hour[2].Minutes >> 8);
                                buffer[2] = (byte)(configurationMFD.Hour[2].Minutes & 0xff);
                            }
                            if (configurationMFD.Hour[2]._24h)
                            {
                                CX52Write.Get()?.Set_Hour24(buffer);
                            }
                            else
                            {
                                CX52Write.Get()?.Set_Hour24(buffer);
                            }
                            menuMFD.PageStatus = 6;
                            ShowPageHour(false,
                                (sbyte)(configurationMFD.Hour[2].Minutes / 60),
                                (byte)((((configurationMFD.Hour[2].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[2].Minutes) % 60),
                                configurationMFD.Hour[2]._24h);
                        }
                        break;
                }
            }
            #endregion

            #region "button up"
            else if (button == (byte)Button.Up)
            {
                switch (menuMFD.PageStatus)
                {
                    case 0:
                        if (menuMFD.CursorStatus != 0)
                        {
                            menuMFD.CursorStatus--;
                        }
                        ShowPage1();
                        break;
                    case 1:
                        if (menuMFD.CursorStatus != 0)
                        {
                            menuMFD.CursorStatus--;
                        }
                        ShowPageOnOff();
                        break;
                    case 2:
                    case 3:
                        if (menuMFD.CursorStatus != 0)
                        {
                            menuMFD.CursorStatus--;
                        }
                        ShowPageLight(menuMFD.CursorStatus);
                        break;
                    case 4:
                    case 5:
                    case 6:
                        if (menuMFD.CursorStatus != 0)
                        {
                            menuMFD.CursorStatus--;
                        }
                        ShowPageHour(false,
                            (sbyte)(configurationMFD.Hour[menuMFD.PageStatus - 4].Minutes / 60),
                            (byte)((((configurationMFD.Hour[menuMFD.PageStatus - 4].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[menuMFD.PageStatus - 4].Minutes) % 60),
                            configurationMFD.Hour[menuMFD.PageStatus - 4]._24h);
                        break;
                    case 40:
                    case 50:
                    case 60:
                        {
                            char idx = (char)((menuMFD.PageStatus / 10) - 4);
                            if ((configurationMFD.Hour[0].Minutes / 60) != 23)
                            {
                                configurationMFD.Hour[idx].Minutes += 60;
                            }
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[idx].Minutes / 60),
                                (byte)((((configurationMFD.Hour[idx].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[idx].Minutes) % 60),
                                configurationMFD.Hour[idx]._24h);
                        }
                        break;
                    case 41:
                    case 51:
                    case 61:
                        {
                            char idx = (char)((menuMFD.PageStatus / 10) - 4);
                            if ((configurationMFD.Hour[0].Minutes % 60) != 59)
                            {
                                configurationMFD.Hour[idx].Minutes++;
                            }
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[idx].Minutes / 60),
                                (byte)((((configurationMFD.Hour[idx].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[idx].Minutes) % 60),
                                configurationMFD.Hour[idx]._24h);
                        }
                        break;
                    case 42:
                    case 52:
                    case 62:
                        {
                            char idx = (char)((menuMFD.PageStatus / 10) - 4);
                            configurationMFD.Hour[idx]._24h = !configurationMFD.Hour[idx]._24h;
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[idx].Minutes / 60),
                                (byte)((((configurationMFD.Hour[idx].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[idx].Minutes) % 60),
                                configurationMFD.Hour[idx]._24h);
                        }
                        break;
                }
            }
            #endregion

            #region "button down"
            else if (button == (byte)Button.Down)
            {
                switch (menuMFD.PageStatus)
                {
                    case 0:
                        if (menuMFD.CursorStatus != 6)
                        {
                            menuMFD.CursorStatus++;
                        }
                        ShowPage1();
                        break;
                    case 1:
                        if (menuMFD.CursorStatus != 1)
                        {
                            menuMFD.CursorStatus++;
                        }
                        ShowPageOnOff();
                        break;
                    case 2:
                    case 3:
                        if (menuMFD.CursorStatus != 2)
                        {
                            menuMFD.CursorStatus++;
                        }
                        ShowPageLight((menuMFD.PageStatus == 2) ? configurationMFD.GlobalLight : configurationMFD.MFDLight);
                        break;
                    case 4:
                    case 5:
                    case 6:
                        if (menuMFD.CursorStatus != 3)
                        {
                            menuMFD.CursorStatus++;
                        }
                        ShowPageHour(false,
                            (sbyte)(configurationMFD.Hour[menuMFD.PageStatus - 4].Minutes / 60),
                            (byte)((((configurationMFD.Hour[menuMFD.PageStatus - 4].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[menuMFD.PageStatus - 4].Minutes) % 60),
                            configurationMFD.Hour[menuMFD.PageStatus - 4]._24h);
                        break;
                    case 40:
                    case 50:
                    case 60:
                        {
                            char idx = (char)((menuMFD.PageStatus / 10) - 4);
                            if ((configurationMFD.Hour[0].Minutes / 60) != -23)
                            {
                                configurationMFD.Hour[idx].Minutes -= 60;
                            }
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[idx].Minutes / 60),
                                (byte)((((configurationMFD.Hour[idx].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[idx].Minutes) % 60), 
                                configurationMFD.Hour[idx]._24h);
                        }
                        break;
                    case 41:
                    case 51:
                    case 61:
                        {
                            char idx = (char)((menuMFD.PageStatus / 10) - 4);
                            if ((configurationMFD.Hour[0].Minutes % 60) != 0)
                            {
                                configurationMFD.Hour[idx].Minutes--;
                            }
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[idx].Minutes / 60),
                                (byte)((((configurationMFD.Hour[idx].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[idx].Minutes) % 60),
                                configurationMFD.Hour[idx]._24h);
                        }
                        break;
                    case 42:
                    case 52:
                    case 62:
                        {
                            char idx = (char)((menuMFD.PageStatus / 10) - 4);
                            configurationMFD.Hour[idx]._24h = !configurationMFD.Hour[idx]._24h;
                            ShowPageHour(true,
                                (sbyte)(configurationMFD.Hour[idx].Minutes / 60),
                                (byte)((((configurationMFD.Hour[idx].Minutes < 0) ? -1 : 1) * configurationMFD.Hour[idx].Minutes) % 60),
                                configurationMFD.Hour[idx]._24h);
                        }
                        break;
                }
            }
            #endregion
        }

        private void CloseMenu()
        {
            //clear screen
            byte[] row1 = [1, 0];
            CX52Write.Get()?.Set_Text(row1);
            byte[] row2 = [ 2, 0 ];
            CX52Write.Get()?.Set_Text(row2);
            byte[] row3 = [3, 0];
            CX52Write.Get()?.Set_Text(row3);

            //turn of light
            CX52Write.Get()?.Light_Info(0);
            CX52Write.Get()?.Light_MFD(configurationMFD.MFDLight);

            SaveConfiguration();
            menuMFD.Activated = false;
        }
        #endregion

        #region Pages
        private void ShowPage1() 
        {
            byte cursor = (byte)(menuMFD.CursorStatus % 3);
            byte page = (byte)(menuMFD.CursorStatus / 3);
            byte[] f1 = System.Text.Encoding.ASCII.GetBytes(" Gladiator NXT  ");
            byte[] f2 = System.Text.Encoding.ASCII.GetBytes(" Buttons light  ");
            byte[] f3 = System.Text.Encoding.ASCII.GetBytes(" MFD light      ");
            byte[] f4 = System.Text.Encoding.ASCII.GetBytes(" Hour 1         ");
            byte[] f5 = System.Text.Encoding.ASCII.GetBytes(" Hour 2         ");
            byte[] f6 = System.Text.Encoding.ASCII.GetBytes(" Hour 3         "); //251
            byte[] f7 = System.Text.Encoding.ASCII.GetBytes(" Exit           "); //252
            byte[] f8 = System.Text.Encoding.ASCII.GetBytes("                ");
            System.Collections.Generic.IReadOnlyList<byte[]> rows = [f1, f2, f3, f4, f5, f6, f7, f8, f8];

            rows[cursor + (page * 3)][0] = System.Text.Encoding.ASCII.GetBytes(">")[0];

	        for (byte i = 0; i< 3; i++)
	        {
		        byte[] text = rows[i + (page * 3)];
                byte[] buffer = new byte[17];
                for (byte c = 0; c < 16; c++) // Must be 16, strings in buffer are NOT \0 terminated
                {
                    buffer[c + 1] = text[c];
                }

                buffer[0] = (byte)(i + 1);

                CX52Write.Get()?.Set_Text(buffer);
	        }
        }

        private void ShowPageOnOff()
        {
            byte cursor = menuMFD.CursorStatus;
            byte[] f1 = System.Text.Encoding.ASCII.GetBytes(" On     ");
            byte[] f2 = System.Text.Encoding.ASCII.GetBytes(" Off    ");
            System.Collections.Generic.IReadOnlyList<byte[]> rows = [f1, f2];

            rows[cursor][0] = System.Text.Encoding.ASCII.GetBytes(">")[0];

            rows[(configurationMFD.IsNXTActivated) ? 0 : 1][5] = System.Text.Encoding.ASCII.GetBytes("(")[0];
            rows[(configurationMFD.IsNXTActivated) ? 0 : 1][6] = System.Text.Encoding.ASCII.GetBytes("*")[0];
            rows[(configurationMFD.IsNXTActivated) ? 0 : 1][7] = System.Text.Encoding.ASCII.GetBytes(")")[0];

            for (byte i = 0; i < 2; i++)
            {
                byte[] text = rows[i];
                byte[] buffer = new byte[9];
                for (byte c = 0; c < 8; c++) // Must be 8, strings in buffer are NOT \0 terminated
                {
                    buffer[c + 1] = text[c];
                }
                buffer[0] = (byte)(i + 1);
                CX52Write.Get()?.Set_Text(buffer);
            }
            byte[] row3 = [3]; //row 3 empty
            CX52Write.Get()?.Set_Text(row3);
        }

        private void ShowPageLight(byte status)
        {
            byte cursor = menuMFD.CursorStatus;
            byte[] f1 = System.Text.Encoding.ASCII.GetBytes(" Low      ");
            byte[] f2 = System.Text.Encoding.ASCII.GetBytes(" Medium   ");
            byte[] f3 = System.Text.Encoding.ASCII.GetBytes(" High     ");
            System.Collections.Generic.IReadOnlyList<byte[]> rows = [f1, f2, f3];

            rows[cursor][0] = System.Text.Encoding.ASCII.GetBytes(">")[0];

            rows[status][7] = System.Text.Encoding.ASCII.GetBytes("(")[0];
            rows[status][8] = System.Text.Encoding.ASCII.GetBytes("*")[0];
            rows[status][9] = System.Text.Encoding.ASCII.GetBytes(")")[0];

            for (byte i = 0; i< 3; i++)
	        {
		        byte[] text = rows[i];
                byte[] buffer = new byte[11];
                for (byte c = 0; c < 10; c++) // Must be 10, strings in buffer are NOT \0 terminated
                {
                    buffer[c + 1] = text[c];
                }
                buffer[0] = (byte)(i + 1);

                CX52Write.Get()?.Set_Text(buffer);
	        }
        }

        private void ShowPageHour(bool sel, sbyte hour, byte minute, bool h24)
        {

            byte cursor = (byte)(menuMFD.CursorStatus % 3);
            byte page = (byte)(menuMFD.CursorStatus / 3);
            byte[] f1 = System.Text.Encoding.ASCII.GetBytes($" Hour: {hour:+00;-00;+00} ");
            byte[] f2 = System.Text.Encoding.ASCII.GetBytes($" Minute: {minute:00}");
            byte[] f3 = System.Text.Encoding.ASCII.GetBytes($" AM/PM: {(h24 ? "No " : "Yes")}");
            byte[] f4 = System.Text.Encoding.ASCII.GetBytes(" Back      ");
            byte[] f5 = System.Text.Encoding.ASCII.GetBytes("           ");
            System.Collections.Generic.IReadOnlyList<byte[]> rows = [f1, f2, f3, f4, f5];

            rows[cursor + (page * 3)][0] = (sel) 
                ? System.Text.Encoding.ASCII.GetBytes("*")[0]
                : System.Text.Encoding.ASCII.GetBytes(">")[0];

	        for (byte i = 0; i< 3; i++)
	        {
		        byte[] text = rows[i + (page * 3)];
                byte[] buffer = new byte[12];

                buffer[0] = (byte)(i + 1);
                for (byte c = 0; c < 11; c++) // Must be 11, strings in buffer are NOT \0 terminated
                {
                    buffer[c + 1] = text[c];
                }

                CX52Write.Get()?.Set_Text(buffer);
	        }
        }
        #endregion
    }
}
