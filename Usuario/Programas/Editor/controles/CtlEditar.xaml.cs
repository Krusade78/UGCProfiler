using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlEditar.xaml
    /// </summary>
    internal partial class CtlEditar : UserControl
    {
        public CtlEditar()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            tabAcel.Content = new CtlAcelerador(this);
            tabJoy.Content = new CtlJoystick(this);
        }
    }
}
