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
            ComboBoxMacro.DataContext = padre.GetDatos().Perfil.ACCIONES;
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
    }
}
