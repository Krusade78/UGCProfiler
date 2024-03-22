using System.Windows.Controls;


namespace Editor
{
    /// <summary>
    /// Lógica de interacción para CtlEditar.xaml
    /// </summary>
    internal partial class CtlOther : UserControl
    {
        private readonly CtlPropiedades Vista;

        public CtlOther(CtlPropiedades vista)
        {
            InitializeComponent();
            Vista = vista;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
        }
    }
}
