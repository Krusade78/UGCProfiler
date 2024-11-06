using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Profiler.Controls.Properties
{
    internal sealed partial class CtlVAxisConf : UserControl
    {
        private readonly CtlProperties parent;

        public CtlVAxisConf(CtlProperties parent)
        {
            this.parent = parent;
            this.InitializeComponent();
        }

        #region "Events"
        private void ComboBoxAxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (parent.Events)
            {
                //SetAxis();
            }
        }

        private void CheckBoxInverted_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
            {
                //SetAxis();
            }
        }

        private async void ButtonSensibility_Click(object sender, RoutedEventArgs e)
        {
            //SetAxis();
            parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            await Dialogs.SensibilityEditor.Show(axis);
        }

        private async void ButtonCopyFrom_Click(object sender, RoutedEventArgs e)
        {
            //SetAxis();
            parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            await Dialogs.CopyFrom.Show(axis, CtlProperties.CurrentSel.Joy, CtlProperties.CurrentSel.Usage.Id);
        }

        private void NumericUpDownMSensibility_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (parent.Events)
            {
                //SetMouseSensibility();
            }
        }

        private void RadioButtonIncremental_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
            {
                parent.Events = false;
                RadioButtonZones.IsOn = false;
                parent.Events = true;
                //SetAxisMode();
            }
        }
        private void RadioButtonBands_Checked(object sender, RoutedEventArgs e)
        {
            if (parent.Events)
            {
                parent.Events = false;
                RadioButtonIncremental.IsOn = false;
                parent.Events = true;
                //SetAxisMode();
            }
        }

        private async void ButtonEditBands_Click(object sender, RoutedEventArgs e)
        {
            //SetAxis();
            parent.GetParent().GetData().Profile.AxesMap.TryGetValue(CtlProperties.CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(parent.GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CtlProperties.CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            if (await Dialogs.ZoneEditor.Show(CtlProperties.CurrentSel.Usage.Range, axis) == ContentDialogResult.Primary)
            {
                parent.ResetMacroIndex();
            }
        }
        #endregion
    }
}
