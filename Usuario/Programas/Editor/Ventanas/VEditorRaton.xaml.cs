using System.Windows;

namespace Editor
{
    /// <summary>
    /// Lógica de interacción para VEditorRaton.xaml
    /// </summary>
    internal partial class VEditorRaton : Window
    {
        public VEditorRaton()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NumericUpDown1.Value = ((MainWindow)this.Owner).GetDatos().Perfil.GENERAL[0].TickRaton;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)this.Owner).GetDatos().Perfil.GENERAL[0].TickRaton = (byte)NumericUpDown1.Value;
            this.DialogResult = true;
            this.Close();
        }
    }
}
