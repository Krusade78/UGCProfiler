using Microsoft.UI.Xaml.Controls;
using System;

namespace Profiler.Dialogs
{
    internal partial class AssignDefault : Frame
    {
        private readonly Devices.DeviceInfo devInfo;

#if DEBUG
        public AssignDefault() { InitializeComponent(); devInfo = new(); }
#endif
        private AssignDefault(Devices.DeviceInfo devInfo, byte mode, byte subMode)
        {
            InitializeComponent();
            this.devInfo =devInfo;
            txtMode.Text = (mode + 1).ToString();
            txtSubMode.Text = (subMode + 1).ToString();
        }

        public static async System.Threading.Tasks.Task Show(Devices.DeviceInfo devInfo, byte mode, byte subMode)
        {
            AssignDefault content = new(devInfo, mode, subMode);
            ContentDialog dlg = new()
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = ((App)Microsoft.UI.Xaml.Application.Current).GetRoot(),
                Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Microsoft.UI.Xaml.Style,
                Title = Translate.Get("assign_vjoy_default_configuration"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = ContentDialogButton.Primary,
                Content = content
            };

            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                content.Save();
            }
        }

        private void Save()
        {
            byte selMode = (byte)((byte.Parse(txtMode.Text) - 1) | (byte.Parse(txtSubMode.Text) - 1) << 4);
            MainPage parent = ((App)Microsoft.UI.Xaml.Application.Current).GetMainPage();

            byte idxHat = 0;
            byte idxAxis = 0;
            System.Collections.Generic.List<byte> axesUsed = [];
            foreach (Shared.ProfileModel.DeviceInfo.CUsage usg in devInfo.Usages)
            {
                if (usg.Type == 254)
                {
                    for (byte idx = usg.Id; idx < (usg.Id + usg.Bits); idx++)
                    {
                        if (idx + (NBStartIndex.Value - 1) < 128)
                        {
                            AddButton(parent, selMode, idx);
                        }
                    }
                }
                else if (usg.Type == 253)
                {
                    AddHat(parent, selMode, idxHat++, usg.Range);
                }
                else
                {
                    AddAxis(parent, selMode, idxAxis++, usg.Type, axesUsed);
                }
            }

            parent.GetData().Modified = true;
        }

        private void AddButton(MainPage parent, byte selMode, byte idx)
        {
            if (!parent.GetData().Profile.ButtonsMap.TryGetValue(devInfo.Id, out Shared.ProfileModel.ButtonMapModel? buttonMap))
            {
                buttonMap = new();
                parent.GetData().Profile.ButtonsMap.Add(devInfo.Id, buttonMap);
            }
            if (!buttonMap.Modes.TryGetValue(selMode, out Shared.ProfileModel.ButtonMapModel.ModeModel? mode))
            {
                mode = new();
                buttonMap.Modes.Add(selMode, mode);
            }
            if (!mode.Buttons.TryGetValue(idx, out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel? button))
            {
                button = new() { Type = 0 };
                {
                    uint v = (((uint)idx + (byte)NBStartIndex.Value - 1) << 8) + (((uint)NumericUpDownJ.Value - 1) << 28);
                    uint[] block =
                    [
                        ((byte)Shared.CTypes.CommandType.DxButton + v),
                        (byte)Shared.CTypes.CommandType.Hold,
                        (((byte)Shared.CTypes.CommandType.DxButton | (byte)Shared.CTypes.CommandType.Release) + v),
                    ];
                    Shared.ProfileModel.MacroModel? btMacro = parent.GetData().Profile.Macros.Find(x => (x.Commands.Count == 3) && (x.Commands[0] == block[0]) && (x.Commands[1] == block[1]) && (x.Commands[2] == block[2]));
                    if (btMacro != null)
                    {
                        button.Actions.Add(btMacro.Id);
                    }
                    else
                    {
                        ushort newId = (ushort)(parent.GetData().Profile.Macros[^1].Id + 1);
                        parent.GetData().Profile.Macros.Add(new()
                        {
                            Id = newId,
                            Name = $"<{Translate.Get("button")} {NumericUpDownJ.Value} - {idx + (byte)NBStartIndex.Value - 1 + 1}>",
                            Commands = [.. block]
                        });
                        button.Actions.Add(newId);
                    }
                }
                mode.Buttons.Add(idx, button);
            }
        }

        private void AddHat(MainPage parent, byte selMode, byte idx, ushort range)
        {
            string[] st8 = [Translate.Get("dx_hat_n"), Translate.Get("dx_hat_ne"), Translate.Get("dx_hat_e"), Translate.Get("dx_hat_se"), Translate.Get("dx_hat_s"), Translate.Get("dx_hat_sw"), Translate.Get("dx_hat_w"), Translate.Get("dx_hat_nw")];
            for (byte i = 0; i < 8; i++)
            {
                st8[i] = st8[i].Replace("%", NumericUpDownJ.Value.ToString()).Replace("$", idx.ToString());
            }

            range = (ushort)((range >> 4) - (range & 0xf) + 1);
            for (int pos = 0; pos < range; pos++)
            {
                if (!parent.GetData().Profile.HatsMap.TryGetValue(devInfo.Id, out Shared.ProfileModel.ButtonMapModel? buttonMap))
                {
                    buttonMap = new();
                    parent.GetData().Profile.HatsMap.Add(devInfo.Id, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(selMode, out Shared.ProfileModel.ButtonMapModel.ModeModel? mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(selMode, mode);
                }
                if (!mode.Buttons.TryGetValue((byte)((idx * 8) + pos), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel? button))
                {
                    button = new() { Type = 0 };
                    {
                        uint v = (((uint)idx) << 16) + (uint)(pos << 8) + (((uint)NumericUpDownJ.Value - 1) << 28);
                        uint[] block = [
                            (byte)Shared.CTypes.CommandType.DxHat | v,
                            (byte)Shared.CTypes.CommandType.Hold,
                            (byte)(Shared.CTypes.CommandType.DxHat | Shared.CTypes.CommandType.Release) | v];
                        Shared.ProfileModel.MacroModel? ar = parent.GetData().Profile.Macros.Find(x => (x.Commands.Count == 3) && (x.Commands[0] == block[0]) && (x.Commands[1] == block[1]) && (x.Commands[2] == block[2]));
                        if (ar == null)
                        {
                            ushort newId = (ushort)(parent.GetData().Profile.Macros[^1].Id + 1);
                            parent.GetData().Profile.Macros.Add(new()
                            {
                                Id = newId,
                                Name = st8[pos],
                                Commands = [.. block],
                            });
                            button.Actions.Add(newId);
                        }
                        else
                        {
                            button.Actions.Add(ar.Id);
                        }
                    }
                    mode.Buttons.Add((byte)((idx * 8) + pos), button);
                }
            }
        }

        private void AddAxis(MainPage parent, byte selMode, byte idx, byte type, System.Collections.Generic.List<byte> axesUsed)
        {
            if (!parent.GetData().Profile.AxesMap.TryGetValue(devInfo.Id, out Shared.ProfileModel.AxisMapModel? axisMap))
            {
                axisMap = new();
                parent.GetData().Profile.AxesMap.Add(devInfo.Id, axisMap);
            }
            if (!axisMap.Modes.TryGetValue(selMode, out Shared.ProfileModel.AxisMapModel.ModeModel? mode))
            {
                mode = new();
                axisMap.Modes.Add(selMode, mode);
            }
            if (!mode.Axes.TryGetValue(idx, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel? _))
            {
                if (!axesUsed.Contains(type))
                {
                    Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel newAxis = new()
                    {
                        IdJoyOutput = (byte)(NumericUpDownJ.Value - 1),
                        Type = 1,
                        OutputAxis = type
                    };
                    if (type == 6)
                    {
                        newAxis.IsSensibilityForSlider = true;
                    }
                    mode.Axes.Add(idx, newAxis);
                    axesUsed.Add(type);
                }
                else if (type == 6)
                {
                    for (byte na = 7; na < 9; na++)
                    {
                        if (!axesUsed.Contains(na))
                        {
                            Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel newSlider = new()
                            {
                                IdJoyOutput = (byte)(NumericUpDownJ.Value - 1),
                                Type = 1,
                                OutputAxis = na,
                                IsSensibilityForSlider = true
                            };
                            mode.Axes.Add(idx, newSlider);
                            axesUsed.Add(na);
                            break;
                        }
                    }
                }
            }
        }


        private void NumericUpDownJ_GettingFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs args)
        {
            //if (!IsLoaded)
            //{
            //    args.Handled = true;
            //}
        }
    }
}
