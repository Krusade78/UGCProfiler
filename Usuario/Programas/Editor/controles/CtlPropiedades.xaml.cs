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
            ComboBoxAssigned.DataContext = padre.GetDatos().Perfil.ACCIONES;
            ComboBoxAssigned.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Nombre", System.ComponentModel.ListSortDirection.Ascending));
            ComboBoxAssigned.SelectedIndex = 0;
            ComboBoxMacro.DataContext = padre.GetDatos().Perfil.ACCIONES;
            ComboBoxMacro.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Nombre", System.ComponentModel.ListSortDirection.Ascending));
            ComboBoxMacro.SelectedIndex = 0;
        }
    }
}
