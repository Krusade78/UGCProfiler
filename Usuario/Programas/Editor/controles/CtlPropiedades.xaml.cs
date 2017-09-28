using System;
using System.Windows;
using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlEdicionElemento.xaml
    /// </summary>
    internal partial class CtlPropiedades : UserControl
    {
        public CtlPropiedades()
        {
            InitializeComponent();
            padre = (MainWindow)App.Current.MainWindow;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;
#endif
            ComboBoxAssigned.DataContext = padre.GetDatos().Perfil.ACCIONES;
            ComboBoxAssigned.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Nombre", System.ComponentModel.ListSortDirection.Ascending));
            ComboBoxAssigned.SelectedIndex = 0;
            ComboBoxMacro.DataContext = new System.Data.DataView( padre.GetDatos().Perfil.ACCIONES);
            ((System.Data.DataView)ComboBoxMacro.DataContext).RowFilter = "idAccion <> 0";
            ((System.Data.DataView)ComboBoxMacro.DataContext).Sort = "Nombre";
            ComboBoxMacro.SelectedIndex = 0;
        }

        #region "Ejes"
        private void ComboBoxAxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetEje();
        }

        private void ComboBoxAxesMini_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetEjeMini();
        }

        private void CheckBoxInverted_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                if (ComboBoxAxesMini.Visibility == Visibility.Visible)
                    SetEjeMini();
                else
                    SetEje();
            }
        }

        private void ButtonSensibility_Click(object sender, RoutedEventArgs e)
        {
            VEditorSensibilidad dlg = new VEditorSensibilidad(idActual);
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void NumericUpDownMSensibility_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetSensibilidadRaton();
        }

        private void RadioButtonIncremental_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                RadioButtonBands.IsChecked = false;
                SetModoEje();
            }
        }
        private void RadioButtonBands_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                RadioButtonIncremental.IsChecked = false;
                SetModoEje();
            }
        }

        private void NumericUpDownResistanceInc_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetResistencia(true);
        }

        private void NumericUpDownResistanceDec_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetResistencia(false);
        }

        private void ButtonEditBands_Click(object sender, RoutedEventArgs e)
        {
            VEditorBandas dlg = new VEditorBandas(idActual, tipoActual);
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }
        #endregion

        #region "botones"
        private void RadioButtonUpDown_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
                SetModoBoton(0);
        }

        private void RadioButtonToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
                SetModoBoton(1);        
        }

        private void NumericUpDownPositions_TextChanged(object sender, EventArgs e)
        {
            if (eventos)
                SetModoBoton((byte)NumericUpDownPositions.Value);
        }

        private void ButtonAssignModes_Click(object sender, RoutedEventArgs e)
        {
            VEditorPinkieModos dlg = new VEditorPinkieModos(true);
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void ButtonAssignPinkie_Click(object sender, RoutedEventArgs e)
        {
            VEditorPinkieModos dlg = new VEditorPinkieModos(false);
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void ButtonAssignPOV_Click(object sender, RoutedEventArgs e)
        {
            VEditorPOV dlg = new VEditorPOV(idActual);
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }
        #endregion

        #region "Macros"
        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton1.IsChecked = false;
            Refrescar();
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
            {
                RadioButton2.IsChecked = false;
                Refrescar();
            }
        }

        private void NumericUpDownPosition_TextChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded)
                Refrescar();
        }

        private void ComboBoxAssigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && ComboBoxMacro.SelectedIndex != -1 && eventos)
                AsignarMacro((ushort)ComboBoxAssigned.SelectedValue);
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            VEditorMacros dlg = new VEditorMacros(-1);
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxMacro.SelectedIndex != -1)
            {
                VEditorMacros dlg = new VEditorMacros((int)(ushort)ComboBoxMacro.SelectedValue);
                dlg.Owner = App.Current.MainWindow;
                dlg.ShowDialog();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxMacro.SelectedIndex != -1)
                padre.GetDatos().Perfil.ACCIONES.Rows.Remove(padre.GetDatos().Perfil.ACCIONES.FindByidAccion((ushort)ComboBoxMacro.SelectedValue));
        }
        #endregion
    }
}
