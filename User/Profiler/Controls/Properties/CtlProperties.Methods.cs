using Microsoft.UI.Xaml.Controls;

namespace Profiler.Controls.Properties
{
    internal partial class CtlProperties : UserControl
    {
        public static class CurrentSel
        {
            public static uint Joy { get; set; }
            public static ushort Idx { get; set; }
            public static byte HatPosition { get; set; }
            public static Shared.ProfileModel.DeviceInfo.CUsage Usage { get; set; } = new();
        }

        public Devices.DeviceInfo CurrentDevInfo { get; set; }
        private MainPage parent;
        public  bool Events { get; set; } = true;

        private CtlAxisConf? spAxis;
        private CtlHatConf? spHat;
        private CtlButtonConf? spButton;
        private CtlMacroConf? spMacros;

        public void Refresh()
        {
            Show(CurrentSel.Joy, CurrentSel.Idx, null, CurrentSel.HatPosition, true);
        }

        public byte GetMode() => (byte)((cbSubmode.SelectedIndex << 4) | cbMode.SelectedIndex);

        public MainPage GetParent() => parent;

        #region "Load"
        public void Show(uint joy, ushort idx, string? name, byte hatPosition = 0, bool refresh = false)
        {
            if (CurrentDevInfo == null) { return; }            

            Events = false;

            CurrentSel.Idx = idx;
            CurrentSel.Joy = joy;
            CurrentSel.HatPosition = hatPosition;
            {
                Shared.ProfileModel.DeviceInfo.CUsage? usage = CurrentDevInfo.Usages.Find(x => x.ReportIdx == idx);
                usage ??= CurrentDevInfo.Usages.Find(x => (x.Type == (byte)CEnums.ElementType.Button) && (idx >= x.ReportIdx) && (idx <= x.Range)) ?? new();
                CurrentSel.Usage = usage;
            }

            if (!refresh)
            {
                Label2.Text = name;
                spConfs.Children.Clear();
                spAxis = null;
                spHat = null;
                spButton = null;
                spMacros = null;
            }
            if (CurrentSel.Usage != null)
            {
                if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Hat)
                    Hat();
                else if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Button)
                    JButton();
                else
                    Axis();
            }
            Events = true;
        }

        private void Hat()
        {
            Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel? button = null;

            if (parent.GetData().Profile.HatsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel? buttonMap))
            {
                if (buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel? mode))
                {
                    mode.Buttons.TryGetValue((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), out button);
                }
            }

            if (spButton == null)
            {
                spConfs.Children.Add(spButton = new(this));
                spConfs.Children.Add(spHat = new(this));
            }
            if (spMacros == null)
            {
                spConfs.Children.Add(spMacros = new(this));
            }

            spButton.Init(button);
            spHat?.Init(false);
            spMacros?.Init(button);

            spMacros?.LoadMacroIndex();
        }


        private void JButton()
        {
            Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel? button = null;

            if (parent.GetData().Profile.ButtonsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel? buttonMap))
            {
                if (buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel? mode))
                {
                    mode.Buttons.TryGetValue((byte)(CurrentSel.Idx - CurrentSel.Usage.ReportIdx + CurrentSel.Usage.Id), out button);
                }
            }

            if (spButton == null)
            {
                spConfs.Children.Add(spButton = new(this));
                spConfs.Children.Add(spHat = new(this));
            }
            if (spMacros == null)
            {
                spConfs.Children.Add(spMacros = new(this));
            }

            spButton.Init(button);
            spHat?.Init(true);
            spMacros?.Init(button);

            spMacros?.LoadMacroIndex();
        }

        private void Axis()
        {
            Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel? axis = null;

            if (parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel? axesMap))
            {
                if (axesMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel? mode))
                {
                    mode.Axes.TryGetValue(CurrentSel.Usage.Id, out axis);
                }
            }

            if (spAxis == null)
            {
                spConfs?.Children.Add(spAxis = new(this));
            }
            if (axis != null)
            {
                if ((axis.Type & 0b10000) == 0b10000) //incremental
                {
                    if (spMacros == null)
                    {
                        spMacros = new(this);
                        spConfs?.Children.Add(spMacros);
                    }
                }
                else if ((axis.Type & 0b100000) == 0b100000) //zones
                {
                    if (spMacros == null)
                    {
                        spMacros = new(this);
                        spConfs?.Children.Add(spMacros);
                    }

                }
                else if (spMacros != null)
                {
                    spConfs?.Children.Remove(spMacros);
                    spMacros = null;
                }
            }

            spAxis?.Init(axis);
            spMacros?.Init(axis);
            spMacros?.LoadMacroIndex();
        }
        #endregion

        public void ResetMacroIndex()
        {
            spMacros?.ResetMacroIndex();
        }

        public void EnsureCreatedButton()
        {
            spButton?.EnsureCreated();
        }

        public void EnsureCreatedAxis()
        {
            spAxis?.EnsureCreated();
        }

        public async void AssignDefaultvJoy()
        {
            await Dialogs.AssignDefault.Show(CurrentDevInfo, (byte)cbMode.SelectedIndex, (byte)cbSubmode.SelectedIndex);
            Refresh();
        }
    }
}
