using FluentAvalonia.UI.Controls;
using static Shared.CTypes;

namespace Profiler.Dialogs
{
    internal partial class HatEditor : Frame
    {
        //private byte pov;
        private readonly byte currentModes;
        private readonly uint currentJoy;
        private readonly Shared.ProfileModel.DeviceInfo.CUsage usage;

#if DEBUG
        public HatEditor() { InitializeComponent(); }
#endif
        public HatEditor(uint joyId, byte mode, Shared.ProfileModel.DeviceInfo.CUsage usage)
        {
            InitializeComponent();
            currentModes = mode;
            currentJoy = joyId;
            this.usage = usage;
            //pov = idSeta;
        }

        public static async System.Threading.Tasks.Task Show(uint joyId, byte mode, Shared.ProfileModel.DeviceInfo.CUsage usage)
        {
            HatEditor content = new(joyId, mode, usage);
            ContentDialog dlg = new()
            {
                Title = Translate.Get("hat_editor"),
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
            MainWindow parent = (MainWindow)((Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow;
            string[] st8 = [ Translate.Get("dx_hat_n"), Translate.Get("dx_hat_ne"), Translate.Get("dx_hat_e"), Translate.Get("dx_hat_se"), Translate.Get("dx_hat_s"), Translate.Get("dx_hat_sw"), Translate.Get("dx_hat_w"), Translate.Get("dx_hat_nw")];
            for (byte i = 0; i < 8; i++)
            {
                st8[i] = st8[i].Replace("%", NumericUpDownJ.Value.ToString()).Replace("$", NumericUpDown1.Value.ToString());
            }

            string[] st4 = [st8[0], st8[2], st8[4], st8[6]];


            for (byte i = 0; i <= usage.Range; i++)
            {
                uint v = ((4 - (uint)NumericUpDown1.Value) << 12) + (uint)(i << 8) + (((uint)NumericUpDownJ.Value - 1) << 16);
                uint[] block = [
                    (byte)CommandType.DxHat | v,
                    (byte)CommandType.Hold,
                    (byte)(CommandType.DxHat | CommandType.Release) | v];
                Shared.ProfileModel.MacroModel ar = parent.GetData().Profile.Macros.Find(x => (x.Commands.Count == 3) && (x.Commands[0] == block[0]) && (x.Commands[1] == block[1]) && (x.Commands[2] == block[2]));
                ushort nId = 0;
                if (ar == null)
                {
                    nId = (ushort)(parent.GetData().Profile.Macros[^1].Id + 1);
                    parent.GetData().Profile.Macros.Add(new()
                    {
                        Id = nId,
                        Name = usage.Range == 3 ? st4[i] : st8[i],
                        Commands = [.. block],
                    });
                }
                else
                {
                    nId = ar.Id;
                }

                if (!parent.GetData().Profile.HatsMap.TryGetValue(currentJoy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    buttonMap = new();
                    parent.GetData().Profile.HatsMap.Add(currentJoy, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(currentModes, out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(currentModes, mode);
                }
                if (!mode.Buttons.TryGetValue((byte)((usage.Id * 8) + i), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button))
                {
                    button = new();
                    mode.Buttons.Add((byte)((usage.Id * 8) + i), button);
                }
                button.Type = 0;
                button.Actions.Clear();
                button.Actions.Add(nId);
                button.Actions.Add(0);
            }

            parent.GetData().Modified = true;
        }

        private void NumberBox_GotFocus(object sender, Avalonia.Input.GotFocusEventArgs args)
        {
            if (!IsLoaded)
            {
                args.Handled = true;
            }
        }
    }
}
