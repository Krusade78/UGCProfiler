using System;
using System.Windows;

using System.Windows.Forms;
using System.Drawing;

namespace Launcher
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;
        private CServicio servicio;

        public MainWindow()
        {
            InitializeComponent();
            System.Windows.MessageBox.Show("");
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            MenuLauncher pop = new MenuLauncher(servicio);
            if (pop.ShowDialog() == true)
            {
                notifyIcon.Dispose(); notifyIcon = null;
                servicio.Dispose();
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            servicio = new CServicio(this);
            if (servicio.Iniciar())
            {
                this.Visibility = Visibility.Hidden;
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = new System.Drawing.Icon(App.GetResourceStream(new Uri("/res/Launcher.ico", UriKind.Relative)).Stream);
                notifyIcon.Visible = true;
                notifyIcon.Click += NotifyIcon_Click;
            }
            else
                this.Close();
        }
    }
}
