using Avalonia.Controls;

namespace Profiler.Controls.Properties
{
    internal partial class CtlAxisConf : UserControl
    {
        private readonly CtlProperties parent;

#if DEBUG
        public CtlAxisConf() { this.InitializeComponent(); }
#endif
        public CtlAxisConf(CtlProperties parent)
        {
            this.parent = parent;
            this.InitializeComponent();
        }

        #region "Events"
        private void ComboBoxAxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
#if DEBUG
            if (Design.IsDesignMode) { return; }
#endif
            if (parent.Events)
                SetAxis();
        }

        private void CheckBoxInverted_Checked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (parent.Events)
            {
                SetAxis();
            }
        }

        private async void ButtonSensibility_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SetAxis();
            parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            await Dialogs.SensibilityEditor.Show(axis);
        }

        private async void ButtonCopyFrom_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SetAxis();
            parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            await Dialogs.CopyFrom.Show(axis, CtlProperties.CurrentSel.Joy, CtlProperties.CurrentSel.Usage.Id);
        }

        private void NumericUpDownMSensibility_TextChanged(FluentAvalonia.UI.Controls.NumberBox sender, FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (parent.Events)
            {
                SetMouseSensibility();
            }
        }

        private void RadioButtonIncremental_Checked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (parent.Events)
            {
                parent.Events = false;
                RadioButtonZones.IsChecked = false;
                parent.Events = true;
                SetAxisMode();
            }
        }
        private void RadioButtonBands_Checked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (parent.Events)
            {
                parent.Events = false;
                RadioButtonIncremental.IsChecked = false;
                parent.Events = true;
                SetAxisMode();
            }
        }

        private void NumericUpDownResistanceInc_TextChanged(FluentAvalonia.UI.Controls.NumberBox sender, FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (parent.Events)
            {
                SetResistance(true);
            }
        }

        private void NumericUpDownResistanceDec_TextChanged(FluentAvalonia.UI.Controls.NumberBox sender, FluentAvalonia.UI.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (parent.Events)
            {
                SetResistance(false);
            }
        }

        private async void ButtonEditBands_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SetAxis();
            parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            if (await Dialogs.ZoneEditor.Show(CtlProperties.CurrentSel.Usage.Range, axis) == FluentAvalonia.UI.Controls.ContentDialogResult.Primary)
            {
                SetAxisMode();
            }
        }
        #endregion

        public void Init(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis)
        {
            if (axis != null)
            {
                CheckBoxInverted.IsChecked = ((axis.Type & 0b10) == 0b10); //inverted

                NBOutJoy.Value = axis.IdJoyOutput + 1;
                byte outputAxis = axis.OutputAxis;
                if (outputAxis == 6) { outputAxis = 7; }
                else if (outputAxis == 7) { outputAxis = 6; }
                ComboBoxAxes.SelectedIndex = ((axis.Type & 0b1) == 0) ? 0 : outputAxis + 1;
                ButtonSensibility.IsEnabled = true;

                if ((axis.Type & 0b1000) == 0b1000) //mouse
                {
                    ComboBoxAxes.SelectedIndex = outputAxis + 10;
                    NumericUpDownMSensibility.IsEnabled = true;
                    NumericUpDownMSensibility.Value = axis.Mouse;
                }
                else
                {
                    NumericUpDownMSensibility.IsEnabled = false;
                }
            }

            RadioButtonZones.IsChecked = false;
            RadioButtonIncremental.IsChecked = false;

            if (axis != null)
            {
                if ((axis.Type & 0b10000) == 0b10000) //incremental
                {
                    RadioButtonIncremental.IsChecked = true;
                    NumericUpDownResistanceInc.Value = axis.Resistance.Item1;
                    NumericUpDownResistanceDec.Value = axis.Resistance.Item2;
                }
                if ((axis.Type & 0b100000) == 0b100000) //zones
                {
                    RadioButtonZones.IsChecked = true;
                }
            }
        }

        #region "Edit"
        private void SetAxis()
        {
            if (!parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetParent().GetData().Profile.AxesMap.Add(CtlProperties.CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(parent.GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CtlProperties.CurrentSel.Usage.Id, axis);
            }

            axis.Type &= 0b1111_0000; //types to 0
            axis.OutputAxis = 0; //X by default
            axis.IdJoyOutput = (byte)(NBOutJoy.Value - 1);
            if (ComboBoxAxes.SelectedIndex > 9) //mouse
            {
                axis.Type |= (byte)0b1000;
                axis.OutputAxis = (byte)(ComboBoxAxes.SelectedIndex - 10);
                if (CheckBoxInverted.IsChecked == true)
                    axis.Type |= (byte)0b10; //inverted
            }
            else if (ComboBoxAxes.SelectedIndex > 0)
            {
                axis.Type |= (byte)0b1;
                if ((ComboBoxAxes.SelectedIndex - 1) == 6)
                {
                    axis.OutputAxis = 7;
                }
                else if ((ComboBoxAxes.SelectedIndex - 1) == 7)
                {
                    axis.OutputAxis = 6;
                }
                else
                    axis.OutputAxis = (byte)(ComboBoxAxes.SelectedIndex - 1);
                if (CheckBoxInverted.IsChecked == true)
                    axis.Type |= (byte)0b10; //inverted
            }

            parent.GetParent().GetData().Modified = true;
            parent.Refresh();
        }

        private void SetMouseSensibility()
        {
            if (!parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetParent().GetData().Profile.AxesMap.Add(CtlProperties.CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(parent.GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CtlProperties.CurrentSel.Usage.Id, axis);
            }

            axis.Mouse = (byte)NumericUpDownMSensibility.Value;
            parent.GetParent().GetData().Modified = true;
        }

        private void SetAxisMode()
        {
            byte inc = 0b10000;
            byte zones = 0b100000;

            if (!parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetParent().GetData().Profile.AxesMap.Add(CtlProperties.CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(parent.GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CtlProperties.CurrentSel.Usage.Id, axis);
            }

            axis.Type &= (byte)~inc;
            axis.Type &= (byte)~zones;
            if (RadioButtonZones.IsChecked == true)
            {
                axis.Type |= zones;
                while (axis.Actions.Count > (axis.Zones.Count + 1))
                {
                    axis.Actions.RemoveAt(axis.Actions.Count - 1);
                }
                while (axis.Actions.Count < (axis.Zones.Count + 1))
                {
                    axis.Actions.Add(0);
                }
            }
            else if (RadioButtonIncremental.IsChecked == true)
            {
                axis.Type |= inc;
                while (axis.Actions.Count > 2)
                {
                    axis.Actions.RemoveAt(axis.Actions.Count - 1);
                }
                while (axis.Actions.Count < 2)
                {
                    axis.Actions.Add(0);
                }
            }

            parent.GetParent().GetData().Modified = true;
            SetAxis();
        }

        private void SetResistance(bool inc)
        {
            if (!parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetParent().GetData().Profile.AxesMap.Add(CtlProperties.CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(parent.GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CtlProperties.CurrentSel.Usage.Id, axis);
            }

            if (inc)
                axis.Resistance = ((byte)NumericUpDownResistanceInc.Value, axis.Resistance.Item2);
            else
                axis.Resistance = (axis.Resistance.Item1, (byte)NumericUpDownResistanceDec.Value);

            parent.GetParent().GetData().Modified = true;
        }
        #endregion

        public void EnsureCreated()
        {
            SetAxisMode();
        }
    }
}
