using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Controls.Properties
{
    internal sealed partial class CtlMacroConf : UserControl
    {
        private readonly CtlProperties parent;

        public CtlMacroConf(CtlProperties parent)
        {
            this.parent = parent;
            this.InitializeComponent();

            ComboBoxAssigned.DataContext = parent.GetParent().GetData().Profile.Macros.OrderBy(x => x.Name);

            RadioButton1.IsChecked = true;
            RadioButton2.IsChecked = false;
            NumericUpDownPosition.Value = 1;

            spMacros.Translation += new System.Numerics.Vector3(0, 0, 16);
        }

        #region "Events"
        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
            {
                parent.Events = false;
                RadioButton1.IsChecked = false;
                parent.Events = true;
                parent.Refresh();
            }
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
            {
                parent.Events = false;
                RadioButton2.IsChecked = false;
                parent.Events = true;
                parent.Refresh();
            }
        }

        private void NumericUpDownPosition_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (parent.Events)
                LoadMacroIndex();
        }

        private void ComboBoxAssigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (parent.Events && (ComboBoxAssigned.SelectedValue != null))
                MacroAssignment((ushort)ComboBoxAssigned.SelectedValue);
        }
        #endregion

        public void Init(Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button)
        {
            if ((button != null) && (button.Type == 1))
            {
                NumericUpDownPosition.Maximum = button.Actions.Count;
                //macros
                panelPS.Visibility = Visibility.Collapsed;
                panelPos.Visibility = Visibility.Visible;
            }
            else
            {
                //macros
                panelPos.Visibility = Visibility.Collapsed;
                panelPS.Visibility = Visibility.Visible;
                RadioButton1.Content = Translate.Get("press");
                RadioButton2.Content = Translate.Get("release");
            }
        }

        public void Init(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis)
        {
            panelPos.Visibility = Visibility.Collapsed;
            panelPS.Visibility = Visibility.Visible;
            if (axis != null)
            {
                if ((axis.Type & 0b10000) == 0b10000) //incremental
                {
                    RadioButton1.Content = Translate.Get("increase");
                    RadioButton2.Content = Translate.Get("decrease");
                }
                if ((axis.Type & 0b100000) == 0b100000) //zones
                {
                    if (axis.Zones.Count > 0)
                    {
                        NumericUpDownPosition.Maximum = 1;
                        NumericUpDownPosition.Maximum = axis.Zones.Count + 1;
                    }

                    //'macros
                    panelPos.Visibility = Visibility.Visible;
                    panelPS.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void LoadMacroIndex()
        {
            parent.Events = false;

            if (CtlProperties.CurrentSel.Usage.Type == (byte)CEnums.ElementType.Hat)
            {
                Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = null;

                if (parent.GetParent().GetData().Profile.HatsMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    if (buttonMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                    {
                        mode.Buttons.TryGetValue((byte)((CtlProperties.CurrentSel.Usage.Id * 8) + CtlProperties.CurrentSel.HatPosition), out button);
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
                            ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros.Find(x => x.Id == (RadioButton1.IsChecked == true ? button.Actions[0] : button.Actions[1])).Id;
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
                            ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros.Find(x => x.Id == button.Actions[(byte)(NumericUpDownPosition.Value - 1)]).Id;
                        }
                    }
                }
                if (button == null)
                {
                    ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros[0].Id;
                }
            }
            else if (CtlProperties.CurrentSel.Usage.Type == (byte)CEnums.ElementType.Button)
            {
                Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button = null;

                if (parent.GetParent().GetData().Profile.ButtonsMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap))
                {
                    if (buttonMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode))
                    {
                        mode.Buttons.TryGetValue((byte)(CtlProperties.CurrentSel.Idx - CtlProperties.CurrentSel.Usage.ReportIdx + CtlProperties.CurrentSel.Usage.Id), out button);
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
                            ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros.Find(x => x.Id == (RadioButton1.IsChecked == true ? button.Actions[0] : button.Actions[1])).Id;
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
                            ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros.Find(x => x.Id == button.Actions[(byte)(NumericUpDownPosition.Value - 1)]).Id;
                        }
                    }
                }
                if (button == null)
                {
                    ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros[0].Id;
                }
            }
            else
            {
                Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis = null;

                if (parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap))
                {
                    if (axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode))
                    {
                        mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out axis);
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
                            ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros.Find(x => x.Id == (RadioButton1.IsChecked == true ? axis.Actions[0] : axis.Actions[1])).Id;
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
                            ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros.Find(x => x.Id == axis.Actions[(byte)(NumericUpDownPosition.Value - 1)]).Id;
                        }
                    }
                }
                if (axis == null)
                {
                    ComboBoxAssigned.SelectedValue = parent.GetParent().GetData().Profile.Macros[0].Id;
                }
            }

            parent.Events = true;
        }

        public void ResetMacroIndex()
        {
            NumericUpDownPosition.Value = 1;
        }

        #region "Edit"
        private void MacroAssignment(ushort idx)
        {
            switch ((CEnums.ElementType)CtlProperties.CurrentSel.Usage.Type)
            {
                case CEnums.ElementType.Hat:
                    {
                        //--- Ensure created
                        parent.EnsureCreatedButton();
                        //---
                        parent.GetParent().GetData().Profile.HatsMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap);
                        buttonMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode);
                        mode.Buttons.TryGetValue((byte)((CtlProperties.CurrentSel.Usage.Id * 8) + CtlProperties.CurrentSel.HatPosition), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button);
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
                        parent.EnsureCreatedButton();
                        //---
                        parent.GetParent().GetData().Profile.ButtonsMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.ButtonMapModel buttonMap);
                        buttonMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.ButtonMapModel.ModeModel mode);
                        mode.Buttons.TryGetValue((byte)(CtlProperties.CurrentSel.Idx - CtlProperties.CurrentSel.Usage.ReportIdx + CtlProperties.CurrentSel.Usage.Id), out Shared.ProfileModel.ButtonMapModel.ModeModel.ButtonModel button);
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
                        parent.EnsureCreatedAxis();
                        //---
                        parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
                        axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
                        mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
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

            parent.GetParent().GetData().Modified = true;
            parent.Refresh();
        }
        #endregion
    }
}
