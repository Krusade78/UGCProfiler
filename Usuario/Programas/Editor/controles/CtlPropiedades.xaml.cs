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
            ComboBoxMacro.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Nombre", System.ComponentModel.ListSortDirection.Ascending));
            ComboBoxMacro.SelectedIndex = 0;
        }

        #region "Ejes"
        private void ButtonSensibility_Click(object sender, RoutedEventArgs e)
        {
            VEditorSensibilidad dlg = new VEditorSensibilidad();
            dlg.Owner = App.Current.MainWindow;
            dlg.ShowDialog();
        }

        private void ComboBoxAxes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsLoaded && eventos)
                SetEje();
        }

        private void CheckBoxInverted_Checked(object sender, RoutedEventArgs e)
        {
            if (eventos)
                SetEje();
        }
        #endregion

        #region "Macros"
        private void ComboBoxAssigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxMacro.SelectedIndex != -1)
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
