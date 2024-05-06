using System.Linq;
using Microsoft.UI.Xaml.Controls;


namespace Profiler.Controls
{
    internal partial class CtlProperties : UserControl
    {
        private static class CurrentSel
        {
            public static uint Joy { get; set; }
            public static ushort Idx { get; set; }
            public static byte HatPosition { get; set; }
            public static Shared.ProfileModel.DeviceInfo.CUsage Usage { get; set; }
        }

        public Devices.DeviceInfo CurrentDevInfo { get; set; }
        private MainWindow parent;
        private bool events = true;

        public void Refresh()
        {
            Show(CurrentSel.Joy, CurrentSel.Idx, /*tipoActual,*/ "", CurrentSel.HatPosition);
        }

        private byte GetMode()
        {
            return (byte)((cbMode.SelectedIndex << 4) | cbSubmode.SelectedIndex);
        }

        #region "Load"
        public void Show(uint joy, ushort idx, string name, byte hatPosition = 0)
        {
            if (CurrentDevInfo == null) { return; }
            ComboBoxAssigned.DataContext = parent.GetData().Profile.Macros.OrderBy(x => x.Name);

            events = false;

            if (name != "")
            {
                Label2.Text = name;
                RadioButton1.IsChecked = true;
                RadioButton2.IsChecked = false;
                NumericUpDownPosition.Value = 1;
            }
            CurrentSel.Idx = idx;
            CurrentSel.Joy = joy;
            CurrentSel.HatPosition = hatPosition;
            CurrentSel.Usage = CurrentDevInfo.Usages.Find(x => x.ReportIdx == idx);
            CurrentSel.Usage ??= CurrentDevInfo.Usages.Find(x => (x.Type == (byte)CEnums.ElementType.Button) &&  (idx >= x.ReportIdx) && (idx <= x.Range));

            if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Hat)
                Hat();
            else if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Button)
                JButton();
            else
                Axis();

            events = true;
        }

        private void Hat()
        {
            Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = null;

            if (parent.GetData().Profile.HatsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
            {
                if (buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode.Buttons.TryGetValue((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), out button);
                }
            }

            if ((button != null) && (button.Type == 1))
            {
                RadioButtonUpDown.IsChecked = false;
                RadioButtonToggle.IsChecked = true;
                NumericUpDownPositions.Value = 1;
                NumericUpDownPosition.Maximum = button.Actions.Count;
                //macros
                panelPS.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                panelPos.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            else
            {
                RadioButtonUpDown.IsChecked = true;
                RadioButtonToggle.IsChecked = false;
                //macros
                panelPos.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                panelPS.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                RadioButton1.Content = Translate.Get("press");
                RadioButton2.Content = Translate.Get("release");
            }

            PanelAxisMap.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PanelButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PanelHat.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PanelMacro.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            LoadMacroIndex();
        }


        private void JButton()
        {
            Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = null;

            if (parent.GetData().Profile.ButtonsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
            {
                if (buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode.Buttons.TryGetValue((byte)(CurrentSel.Idx - CurrentSel.Usage.ReportIdx + CurrentSel.Usage.Id), out button);
                }
            }

            if ((button != null) && (button.Type == 1))
            {
                RadioButtonUpDown.IsChecked = false;
                RadioButtonToggle.IsChecked = true;
                NumericUpDownPositions.Value = 1;
                NumericUpDownPosition.Maximum = button.Actions.Count;
                //macros
                panelPS.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                panelPos.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
            else
            {
                RadioButtonUpDown.IsChecked = true;
                RadioButtonToggle.IsChecked = false;
                //macros
                panelPos.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                panelPS.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                RadioButton1.Content = Translate.Get("press");
                RadioButton2.Content = Translate.Get("release");
            }

            PanelAxisMap.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PanelButton.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PanelHat.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PanelMacro.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

            LoadMacroIndex();
        }

        private void Axis()
        {
            Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis = null;

            if (parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                if (axesMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
                {
                    mode.Axes.TryGetValue(CurrentSel.Usage.Id, out axis);
                }
            }

            if (axis != null)
            {
                CheckBoxInverted.IsOn = ((axis.Type & 0b10) == 0b10); //inverted

                NBOutJoy.Value = axis.IdJoyOutput;
                if (axis.OutputAxis == 6) { axis.OutputAxis = 7; }
                else if (axis.OutputAxis == 7) { axis.OutputAxis = 6; }
                ComboBoxAxes.SelectedIndex = ((axis.Type & 0b1) == 0) ? 0 : axis.OutputAxis + 1;
                ButtonSensibility.IsEnabled = true;

                if ((axis.Type & 0b1000) == 0b1000) //mouse
                {
                    ComboBoxAxes.SelectedIndex = axis.OutputAxis + 10;
                    NumericUpDownMSensibility.IsEnabled = true;
                    NumericUpDownMSensibility.Value = axis.Mouse;
                }
                else
                {
                    NumericUpDownMSensibility.IsEnabled = false;
                }
            }

            RadioButtonZones.IsOn = false;
            RadioButtonIncremental.IsOn = false;
            panelPos.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            panelPS.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PanelMacro.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            if (axis != null)
            {
                if ((axis.Type & 0b10000) == 0b10000) //incremental
                {
                    RadioButtonIncremental.IsOn = true;
                    NumericUpDownResistanceInc.Value = axis.Resistance.Item1;
                    NumericUpDownResistanceDec.Value = axis.Resistance.Item2;
                    //'macros
                    PanelMacro.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    RadioButton1.Content = Translate.Get("increase");
                    RadioButton2.Content = Translate.Get("decrease");
                }
                if ((axis.Type & 0b100000) == 0b100000) //zones
                {
                    RadioButtonZones.IsOn = true;
                    if (axis.Zones.Count > 0)
                    {
                        NumericUpDownPosition.Maximum = 1;
                        NumericUpDownPosition.Maximum = axis.Zones.Count;
                        PanelMacro.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    }

                    //'macros
                    panelPos.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    panelPS.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                }
            }


            PanelAxisMap.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            PanelButton.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            PanelHat.Visibility = Microsoft.UI.Xaml.Visibility.Visible;

            LoadMacroIndex();
        }

        private void LoadMacroIndex()
        {
            events = false;

            if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Hat)
            {
                Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = null;

                if (parent.GetData().Profile.HatsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    if (buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                    {
                        mode.Buttons.TryGetValue((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), out button);
                    }
                }
                if (button != null)
                {
                    if (button.Type == 0)
                    {
                        if ((RadioButton1.IsChecked == true) && (button.Actions.Count == 0))
                        {
                            button = null;
                        }
                        else if ((RadioButton1.IsChecked == false) && (button.Actions.Count < 2))
                        {
                            button = null;
                        }
                        else
                        {
                            ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros.Find(x => x.Id == (RadioButton1.IsChecked == true ? button.Actions[0] : button.Actions[1])).Id;
                        }
                    }
                    else
                    {
                        if (button.Actions.Count < NumericUpDownPosition.Value)
                        {
                            button = null;
                        }
                        else
                        {
                            ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros.Find(x => x.Id == button.Actions[(byte)(NumericUpDownPosition.Value - 1)]).Id;
                        }
                    }
                }
                if (button == null)
                {
                    ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros[0].Id;
                }
            }
            else if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Button)
            {
                Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = null;

                if (parent.GetData().Profile.ButtonsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    if (buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                    {
                        mode.Buttons.TryGetValue((byte)(CurrentSel.Idx - CurrentSel.Usage.ReportIdx + CurrentSel.Usage.Id), out button);
                    }
                }
                if (button != null)
                {
                    if (button.Type == 0)
                    {
                        if ((RadioButton1.IsChecked == true) && (button.Actions.Count == 0))
                        {
                            button = null;
                        }
                        else if ((RadioButton1.IsChecked == false) && (button.Actions.Count < 2))
                        {
                            button = null;
                        }
                        else
                        {
                            ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros.Find(x => x.Id == (RadioButton1.IsChecked == true ? button.Actions[0] : button.Actions[1])).Id;
                        }
                    }
                    else
                    {
                        if (button.Actions.Count < NumericUpDownPosition.Value)
                        {
                            button = null;
                        }
                        else
                        {
                            ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros.Find(x => x.Id == button.Actions[(byte)(NumericUpDownPosition.Value - 1)]).Id;
                        }
                    }
                }
                if (button == null)
                {
                    ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros[0].Id;
                }
            }
            else
            {
                Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis = null;

                if (parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap))
                {
                    if (axisMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
                    {
                        mode.Axes.TryGetValue(CurrentSel.Usage.Id, out axis);
                    }
                }
                if (axis != null)
                {
                    if ((axis.Type & 0b10000) == 0b10000)
                    {
                        if ((RadioButton1.IsChecked == true) && (axis.Actions.Count == 0))
                        {
                            axis = null;
                        }
                        else if ((RadioButton1.IsChecked == false) && (axis.Actions.Count < 2))
                        {
                            axis = null;
                        }
                        else
                        {
                            ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros.Find(x => x.Id == (RadioButton1.IsChecked == true ? axis.Actions[0] : axis.Actions[1])).Id;
                        }
                    }
                    else
                    {
                        if (axis.Actions.Count < NumericUpDownPosition.Value)
                        {
                            axis = null;
                        }
                        else
                        {
                            ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros.Find(x => x.Id == axis.Actions[(byte)(NumericUpDownPosition.Value - 1)]).Id;
                        }
                    }
                }
                if (axis == null)
                {
                    ComboBoxAssigned.SelectedValue = parent.GetData().Profile.Macros[0].Id;
                }
            }

            events = true;
        }
        #endregion

        #region "Edit"

        #region "Ejes"
        private void SetAxis()
        {
            if (!parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetData().Profile.AxesMap.Add(CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CurrentSel.Usage.Id, axis);
            }

            axis.Type &= 0b1111_0000; //types to 0
            axis.OutputAxis = 0; //X by default
            axis.IdJoyOutput = (byte)(NBOutJoy.Value - 1);
            if (ComboBoxAxes.SelectedIndex > 9) //mouse
            {
                axis.Type |= (byte)0b1000;
                axis.OutputAxis = (byte)(ComboBoxAxes.SelectedIndex - 10);
                if (CheckBoxInverted.IsOn)
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
                if (CheckBoxInverted.IsOn)
                    axis.Type |= (byte)0b10; //inverted
            }

            parent.GetData().Modified = true;
            Refresh();
        }

        private void SetMouseSensibility()
        {
            if (!parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetData().Profile.AxesMap.Add(CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CurrentSel.Usage.Id, axis);
            }

            axis.Mouse = (byte)NumericUpDownMSensibility.Value;
            parent.GetData().Modified = true;
        }

        private void SetAxisMode()
        {
            byte inc = 0b10000;
            byte zones = 0b100000;

            if (!parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetData().Profile.AxesMap.Add(CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CurrentSel.Usage.Id, axis);
            }

            axis.Type &= (byte)~inc;
            axis.Type &= (byte)~zones;
            if (RadioButtonZones.IsOn)
            {
                axis.Type |= zones;
                while (axis.Actions.Count > axis.Zones.Count)
                {
                    axis.Actions.RemoveAt(axis.Actions.Count - 1);
                }
                while (axis.Actions.Count < axis.Zones.Count)
                {
                    axis.Actions.Add(0);
                }
            }
            else if (RadioButtonIncremental.IsOn)
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

            parent.GetData().Modified = true;
            SetAxis();
        }

        private void SetResistance(bool inc)
        {
            if (!parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axesMap))
            {
                axesMap = new();
                parent.GetData().Profile.AxesMap.Add(CurrentSel.Joy, axesMap);
            }
            if (!axesMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
            {
                mode = new();
                axesMap.Modes.Add(GetMode(), mode);
            }
            if (!mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis))
            {
                axis = new();
                mode.Axes.Add(CurrentSel.Usage.Id, axis);
            }

            if (inc)
                axis.Resistance = ((byte)NumericUpDownResistanceInc.Value, axis.Resistance.Item2);
            else
                axis.Resistance = (axis.Resistance.Item1, (byte)NumericUpDownResistanceDec.Value);

            parent.GetData().Modified = true;
        }
        #endregion

        #region "Buttons"
        private void SetButtonMode(byte numPositions = 0)
        {
            Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button;

            if (CurrentSel.Usage.Type == (byte)CEnums.ElementType.Hat)
            {
                if (!parent.GetData().Profile.HatsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    buttonMap = new();
                    parent.GetData().Profile.HatsMap.Add(CurrentSel.Joy, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(GetMode(), mode);
                }
                if (!mode.Buttons.TryGetValue((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), out button))
                {
                    button = new();
                    mode.Buttons.Add((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), button);
                }
            }
            else
            {
                if (!parent.GetData().Profile.ButtonsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    buttonMap = new();
                    parent.GetData().Profile.ButtonsMap.Add(CurrentSel.Joy, buttonMap);
                }
                if (!buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                {
                    mode = new();
                    buttonMap.Modes.Add(GetMode(), mode);
                }
                if (!mode.Buttons.TryGetValue(CurrentSel.Usage.Id, out button))
                {
                    button = new();
                    mode.Buttons.Add((byte)(CurrentSel.Idx - CurrentSel.Usage.ReportIdx + CurrentSel.Usage.Id), button);
                }
            }

            button.Type = (byte)(RadioButtonToggle.IsChecked == true ? 1 : 0);
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

            parent.GetData().Modified = true;
            Refresh();
        }
        #endregion

        #region "Macros"
        private void MacroAssignment(ushort idx)
        {
            switch ((CEnums.ElementType)CurrentSel.Usage.Type)
            {
                case CEnums.ElementType.Hat:
                    {
                        //--- Ensure created
                        if (RadioButtonToggle.IsChecked == true)
                        {
                            SetButtonMode((byte)NumericUpDownPositions.Value);
                        }
                        else
                        {
                            SetButtonMode();
                        }
                        //---
                        parent.GetData().Profile.HatsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap);
                        buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode);
                        mode.Buttons.TryGetValue((byte)((CurrentSel.Usage.Id * 8) + CurrentSel.HatPosition), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button);
                        if (button.Type == 0)
                        {
                            if (RadioButton1.IsChecked == true)
                                button.Actions[0] = idx;
                            else
                                button.Actions[1] = idx;
                        }
                        else
                            button.Actions[(byte)(NumericUpDownPosition.Value - 1)] = idx;
                    }
                    break;
                case CEnums.ElementType.Button:
                    {
                        //--- Ensure created
                        if (RadioButtonToggle.IsChecked == true)
                        {
                            SetButtonMode((byte)NumericUpDownPositions.Value);
                        }
                        else
                        {
                            SetButtonMode();
                        }
                        //---
                        parent.GetData().Profile.ButtonsMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap);
                        buttonMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode);
                        mode.Buttons.TryGetValue((byte)(CurrentSel.Idx - CurrentSel.Usage.ReportIdx + CurrentSel.Usage.Id), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button);
                        if (button.Type == 0)
                        {
                            if (RadioButton1.IsChecked == true)
                                button.Actions[0] = idx;
                            else
                                button.Actions[1] = idx;
                        }
                        else
                            button.Actions[(byte)(NumericUpDownPosition.Value - 1)] = idx;
                    }
                    break;
                default:
                    {
                        //--- Ensure created
                        SetAxisMode();
                        //---
                        parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
                        axisMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
                        mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
                        if ((axis.Type & 0b10000) == 0b10000) // incremental
                        {
                            if (RadioButton1.IsChecked == true)
                                axis.Actions[0] = idx;
                            else
                                axis.Actions[1] = idx;
                        }
                        else if ((axis.Type & 0b100000) == 0b100000) // zones
                        {
                            axis.Actions[(byte)(NumericUpDownPosition.Value - 1)] = idx;
                        }
                    }
                    break;
            }

            parent.GetData().Modified = true;
        }
        #endregion

        #endregion
    }
}
