using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Profiler.Controls.Properties
{
    internal partial class CtlHatConf : UserControl
    {
        private readonly CtlProperties parent;

#if DEBUG
        public CtlHatConf() { this.InitializeComponent(); }
#endif
        public CtlHatConf(CtlProperties parent)
        {
            this.parent = parent;
            this.InitializeComponent();
        }

        public void Init(bool isButton)
        {
            if (isButton)
            {
                buttonSection.IsVisible = true;
            }
            else
            {
                hatSection.IsVisible = true;
                buttonSection.IsVisible = true;
            }
        }

        private async void ButtonAssignPOV_Click(object sender, RoutedEventArgs e)
        {
            await Dialogs.HatEditor.Show(CtlProperties.CurrentSel.Joy, parent.GetMode(), CtlProperties.CurrentSel.Usage);
        }

        private void ButtonAssign_Click(object sender, RoutedEventArgs e)
        {
            Assign();
        }

        private void Assign()
        {
            Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button;

            if (CtlProperties.CurrentSel.Usage.Type == (byte)CEnums.ElementType.Hat)
            {
                if (!parent.GetParent().GetData().Profile.HatsMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    buttonMap = new();
                    parent.GetParent().GetData().Profile.HatsMap.Add(CtlProperties.CurrentSel.Joy, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(parent.GetMode(), mode);
                }
                if (!mode.Buttons.TryGetValue((byte)((CtlProperties.CurrentSel.Usage.Id * 8) + CtlProperties.CurrentSel.HatPosition), out button))
                {
                    button = new();
                    mode.Buttons.Add((byte)((CtlProperties.CurrentSel.Usage.Id * 8) + CtlProperties.CurrentSel.HatPosition), button);
                }
            }
            else
            {
                if (!parent.GetParent().GetData().Profile.ButtonsMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    buttonMap = new();
                    parent.GetParent().GetData().Profile.ButtonsMap.Add(CtlProperties.CurrentSel.Joy, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(parent.GetMode(), mode);
                }
                if (!mode.Buttons.TryGetValue((byte)(CtlProperties.CurrentSel.Idx - CtlProperties.CurrentSel.Usage.ReportIdx + CtlProperties.CurrentSel.Usage.Id), out button))
                {
                    button = new();
                    mode.Buttons.Add((byte)(CtlProperties.CurrentSel.Idx - CtlProperties.CurrentSel.Usage.ReportIdx + CtlProperties.CurrentSel.Usage.Id), button);
                }
            }

            button.Type = 0;
            while (button.Actions.Count > 2)
            {
                button.Actions.RemoveAt(button.Actions.Count - 1);
            }
            while (button.Actions.Count < 2)
            {
                button.Actions.Add(0);
            }

            string[] st8 = [Translate.Get("dx_hat_n"), Translate.Get("dx_hat_ne"), Translate.Get("dx_hat_e"), Translate.Get("dx_hat_se"), Translate.Get("dx_hat_s"), Translate.Get("dx_hat_sw"), Translate.Get("dx_hat_w"), Translate.Get("dx_hat_nw")];
            for (byte i = 0; i < 8; i++)
            {
                st8[i] = st8[i].Replace("%", NumericUpDownJ.Value.ToString()).Replace("$", NumericUpDown1.Value.ToString());
            }

            uint v = (((uint)NumericUpDown1.Value - 1) << 16) + (uint)(cbPosition.SelectedIndex << 8) + (((uint)NumericUpDownJ.Value - 1) << 28);
            uint[] block = [
                (byte)Shared.CTypes.CommandType.DxHat | v,
                (byte)Shared.CTypes.CommandType.Hold,
                (byte)(Shared.CTypes.CommandType.DxHat | Shared.CTypes.CommandType.Release) | v];
            Shared.ProfileModel.MacroModel ar = parent.GetParent().GetData().Profile.Macros.Find(x => (x.Commands.Count == 3) && (x.Commands[0] == block[0]) && (x.Commands[1] == block[1]) && (x.Commands[2] == block[2]));
            if (ar == null)
            {
                parent.GetParent().GetData().Profile.Macros.Add(new()
                {
                    Id = (ushort)(parent.GetParent().GetData().Profile.Macros[^1].Id + 1),
                    Name = st8[cbPosition.SelectedIndex],
                    Commands = [.. block],
                });
            }
            else
            {
                button.Actions[0] = ar.Id;
            }

            parent.GetParent().GetData().Modified = true;
            parent.Refresh();
        }
    }
}
