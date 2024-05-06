using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;


namespace Profiler.Controls
{
    /// <summary>
    /// Lógica de interacción para CtlEdicionElemento.xaml
    /// </summary>
    internal sealed partial class CtlProperties : UserControl
    {
        public CtlProperties()
        {
            events = false;
            InitializeComponent();
            events = true;
            spModes.Translation += new System.Numerics.Vector3(0, 0, 48);
            bd1.Translation += new System.Numerics.Vector3(0, 0, 32);
            spAxis.Translation += new System.Numerics.Vector3(0, 0, 16);
            spButton.Translation += new System.Numerics.Vector3(0, 0, 16);
            spHat.Translation += new System.Numerics.Vector3(0, 0, 16);
            spMacros.Translation += new System.Numerics.Vector3(0, 0, 32);
        }

        private void FcbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            parent = ((App)Application.Current).GetMainWindow();
            //            ComboBoxMacro.DataContext = new System.Data.DataView(padre.GetData().Profile.ACCIONES);
            //            ((System.Data.DataView)ComboBoxMacro.DataContext).RowFilter = "idAccion <> 0";
            //            ((System.Data.DataView)ComboBoxMacro.DataContext).Sort = "Nombre";
            //            ComboBoxMacro.SelectedIndex = 0;
        }

        #region "Axes"
        private void ComboBoxAxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (events)
                SetAxis();
        }

        private void CheckBoxInverted_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
            {
                SetAxis();
            }
        }

        private async void ButtonSensibility_Click(object sender, RoutedEventArgs e)
        {
            SetAxis();
            parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            await Dialogs.SensibilityEditor.Show(axis);
        }

        private async void ButtonCopyFrom_Click(object sender, RoutedEventArgs e)
        {
            SetAxis();
            parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            await Dialogs.CopyFrom.Show(axis, CurrentSel.Joy, CurrentSel.Usage.Id);
        }

        private void NumericUpDownMSensibility_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (events)
                SetMouseSensibility();
        }

        private void RadioButtonIncremental_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
            {
                events = false;
                RadioButtonZones.IsOn = false;
                events = true;
                SetAxisMode();
            }
        }
        private void RadioButtonBands_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
            {
                events = false;
                RadioButtonIncremental.IsOn = false;
                events = true;
                SetAxisMode();
            }
        }

        private void NumericUpDownResistanceInc_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (events)
                SetResistance(true);
        }

        private void NumericUpDownResistanceDec_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (events)
                SetResistance(false);
        }

        private async void ButtonEditBands_Click(object sender, RoutedEventArgs e)
        {
            SetAxis();
            parent.GetData().Profile.AxesMap.TryGetValue(CurrentSel.Joy, out Shared.ProfileModel.AxisMapModel axisMap);
            axisMap.Modes.TryGetValue(GetMode(), out Shared.ProfileModel.AxisMapModel.ModeModel mode);
            mode.Axes.TryGetValue(CurrentSel.Usage.Id, out Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axis);
            if (await Dialogs.ZoneEditor.Show(CurrentSel.Usage.Range, axis) == ContentDialogResult.Primary)
            {
                NumericUpDownPosition.Value = 1;
            }
        }
        #endregion

        #region "Buttons"
        private void RadioButtonUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
                SetButtonMode();
        }

        private void RadioButtonToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
                SetButtonMode(1);
        }

        private void NumericUpDownPositions_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (events)
                SetButtonMode((byte)NumericUpDownPositions.Value);
        }

        private async void ButtonAssignPOV_Click(object sender, RoutedEventArgs e)
        {
            await Dialogs.HatEditor.Show(CurrentSel.Joy, GetMode(), CurrentSel.Usage);
            //            VEditorPOV dlg = new VEditorPOV(idActual)
            //            {
            //                Owner = App.Current.MainWindow
            //            };
            //            if (dlg.ShowDialog() == true)
            //            {
            //                padre.GetData().Modified = true;
            //                Ver(idActual, tipoActual, "");
            //            }
        }
        #endregion

        #region "Macros"
        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
            {
                events = false;
                RadioButton1.IsChecked = false;
                events = true;
                Refresh();
            }
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            if (events)
            {
                events = false;
                RadioButton2.IsChecked = false;
                events = true;
                Refresh();
            }
        }

        private void NumericUpDownPosition_TextChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (events)
                Refresh();
        }

        private void ComboBoxAssigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (events && (ComboBoxAssigned.SelectedValue != null))
                MacroAssignment((ushort)ComboBoxAssigned.SelectedValue);
        }

        //private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    //            VEditorMacros dlg = new VEditorMacros(-1)
        //    //            {
        //    //                Owner = App.Current.MainWindow
        //    //            };
        //    //            if (dlg.ShowDialog() == true)
        //    //                padre.GetData().Modified = true;
        //}

        //private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        //{
        //    //            if (ComboBoxMacro.SelectedIndex != -1)
        //    //            {
        //    //                VEditorMacros dlg = new VEditorMacros((int)(ushort)ComboBoxMacro.SelectedValue)
        //    //                {
        //    //                    Owner = App.Current.MainWindow
        //    //                };
        //    //                if (dlg.ShowDialog() == true)
        //    //                    padre.GetData().Modified = true;
        //    //            }
        //}

        //private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        //{
        //    //            if (ComboBoxMacro.SelectedIndex != -1)
        //    //                padre.GetData().Profile.ACCIONES.Rows.Remove(padre.GetData().Profile.ACCIONES.FindByidAccion((ushort)ComboBoxMacro.SelectedValue));
        //}
        #endregion
    }
}
