using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Controls.Properties
{
    internal sealed partial class CtlButtonConf : UserControl
    {
        private readonly CtlProperties parent;
        public CtlButtonConf(CtlProperties parent)
        {
            this.parent = parent;
            this.InitializeComponent();

            spButton.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        #region "Events"
        private void RadioButtonUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
                SetButtonMode();
        }

        private void RadioButtonToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
                SetButtonMode(1);
        }

        private void NumericUpDownPositions_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (parent.Events)
                SetButtonMode((byte)NumericUpDownPositions.Value);
        }

        private void ButtonAssign_Click(object sender, RoutedEventArgs e)
        {
            SetButtonMode(0, true);
        }
        #endregion

        public void Init(Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button)
        {
            if ((button != null) && (button.Type == 1))
            {
                RadioButtonUpDown.IsChecked = false;
                RadioButtonToggle.IsChecked = true;
                NumericUpDownPositions.Value = 1;
            }
            else
            {
                RadioButtonUpDown.IsChecked = true;
                RadioButtonToggle.IsChecked = false;
            }
        }

        #region "Edit"
        private void SetButtonMode(byte numPositions = 0, bool dxDefault = false)
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

            button.Type = (byte)(dxDefault ? 0 : (RadioButtonToggle.IsChecked == true ? 1 : 0));
            if (button.Type == 0)
            {
                numPositions = 2;
            }
            while (button.Actions.Count > numPositions)
            {
                button.Actions.RemoveAt(button.Actions.Count - 1);
            }
            while (button.Actions.Count < numPositions)
            {
                button.Actions.Add(0);
            }

            if (dxDefault)
            {
                uint v = (((uint)NumericUpDown1.Value - 1) << 8) + (((uint)NumericUpDownJ.Value - 1) << 16);
                uint[] block =
                [
                    ((byte)Shared.CTypes.CommandType.DxButton + v),
                    (byte)Shared.CTypes.CommandType.Hold,
                    (((byte)Shared.CTypes.CommandType.DxButton | (byte)Shared.CTypes.CommandType.Release) + v),
                ];
                Shared.ProfileModel.MacroModel btMacro = parent.GetParent().GetData().Profile.Macros.Find(x => (x.Commands.Count == 3) && (x.Commands[0] == block[0]) && (x.Commands[1] == block[1]) && (x.Commands[2] == block[2]));
                if (btMacro != null)
                {
                    button.Actions[0] = btMacro.Id;
                }
                else
                {
                    parent.GetParent().GetData().Profile.Macros.Add(new() {
                        Id = (ushort)(parent.GetParent().GetData().Profile.Macros[^1].Id + 1),
                        Name = $"<{Translate.Get("button")} {NumericUpDownJ.Value} - {NumericUpDown1.Value}>",
                        Commands = [.. block] });
                }
            }

            parent.GetParent().GetData().Modified = true;
            parent.Refresh();
        }
        #endregion

        public void EnsureCreated()
        {
            if (RadioButtonToggle.IsChecked == true)
            {
                SetButtonMode((byte)NumericUpDownPositions.Value);
            }
            else
            {
                SetButtonMode();
            }
        }
    }
}
