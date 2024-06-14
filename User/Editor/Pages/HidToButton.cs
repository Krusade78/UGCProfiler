namespace Profiler.Pages
{
    internal interface IHidToButton
    {
        void UpdateStatus(Devices.DeviceInfo di, byte[] rawData);
    }

    internal class CHidToButton(System.Collections.Generic.List<CHidToButton.Map> map)
    {
        public class Map
        {
            public ushort Idx {  get; set; }
            public Microsoft.UI.Xaml.Controls.Primitives.ToggleButton Button { get; set; }
            public System.Collections.Generic.List<Microsoft.UI.Xaml.Controls.Primitives.ToggleButton> HatButtons { get; set; }

            public Map(ushort idx, Microsoft.UI.Xaml.Controls.Primitives.ToggleButton button)
            {
                Idx = idx;
                Button = button;
            }

            public Map(ushort idx, System.Collections.Generic.List<Microsoft.UI.Xaml.Controls.Primitives.ToggleButton> hatButtons)
            {
                Idx = idx;
                HatButtons = hatButtons;
            }
        }

        private Devices.DeviceInfo.HID_INPUT_DATA oldHidData = new();
        private Devices.DeviceInfo.HID_INPUT_DATA oldAxisHidData = new();

        public void Update(Devices.DeviceInfo di, byte[] rawData)
        {
            Devices.DeviceInfo.HID_INPUT_DATA hidData = new();
            if (!di.ToHiddata(rawData, ref hidData))
            {
                return;
            }

            for (byte i = 0; i < hidData.Axis.Length; i++)
            {
                Shared.ProfileModel.DeviceInfo.CUsage u = di.Usages.Find(x => (x.Id == i) && (x.Type < 253));
                if (u == null) continue;
                if (
                    ((hidData.Axis[i] < oldAxisHidData.Axis[i]) && (hidData.Axis[i] > (u.Range / 2)))
                    || ((hidData.Axis[i] > oldAxisHidData.Axis[i]) && (hidData.Axis[i] < (u.Range / 2)))
                    )
                {
                    continue; //going to center
                }
                if (((hidData.Axis[i] > (u.Range / 2 * 1.10)) || (hidData.Axis[i] < (u.Range / 2 * 0.9)))
                    && (hidData.Axis[i] > (u.Range * 0.25)) && (hidData.Axis[i] < (u.Range * 0.75))
                    && ((hidData.Axis[i] > (oldAxisHidData.Axis[i] * 1.10)) || (hidData.Axis[i] < (oldAxisHidData.Axis[i] * 0.9))))
                {
                    map.Find(x => x.Idx == u.ReportIdx).Button.IsChecked = true;
                    oldAxisHidData = hidData;
                    oldHidData = hidData;
                    return;
                }
            }

            for (byte i = 0; i < 4; i++)
            {
                if (hidData.Hats[i] != oldHidData.Hats[i])
                {
                    byte nPos = hidData.Hats[i];
                    if (nPos != 8)
                    {
                        Shared.ProfileModel.DeviceInfo.CUsage u = di.Usages.Find(x => (x.Id == i) && (x.Type == 253));
                        map.Find(x => x.Idx == u.ReportIdx).HatButtons[nPos].IsChecked = true;
                        oldHidData = hidData;
                        return;
                    }
                }
            }
            for (byte i = 0; i < 2; i++)
            {
                if (hidData.Buttons[i] != oldHidData.Buttons[i])
                {
                    for (byte j = 0; j < 64; j++)
                    {
                        ulong bt = hidData.Buttons[i] & (1ul << j);
                        if (bt > 0 && bt != (oldHidData.Buttons[i] & (1ul << j)))
                        {
                            byte pos = (byte)(j + (i * 64));
                            Shared.ProfileModel.DeviceInfo.CUsage u = di.Usages.Find(x => (pos >= x.Id) && (pos <= (x.Id + x.Bits)) && (x.Type == 254));
                            map.Find(x => x.Idx == u.ReportIdx + pos - u.Id).Button.IsChecked = true;
                            oldHidData = hidData;
                            return;
                        }
                    }
                }
            }

            oldHidData = hidData;
        }
    }
}
