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
            tabPedales.Content = new CtlPedales(this);
            tabX52Joy.Content = new CtlX52Joystick(this);
            tabAcel.Content = new CtlAcelerador(this);
            tabNXTJoy.Content = new CtlNXTJoystick(this);
        }

        public Comunes.CTipos.TipoJoy GetTipoJoy()
        {
            if (tabPedales.IsSelected)
                return Comunes.CTipos.TipoJoy.Pedales;
            else if (tabX52Joy.IsSelected)
                return Comunes.CTipos.TipoJoy.X52_Joy;
            else if (tabAcel.IsSelected)
                return Comunes.CTipos.TipoJoy.X52_Ace;
            else
                return Comunes.CTipos.TipoJoy.NXT;
        }
    }
}
