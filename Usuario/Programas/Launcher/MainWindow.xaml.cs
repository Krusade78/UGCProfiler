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
            servicio = new CServicio();
            if (servicio.Iniciar())
            {
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = new System.Drawing.Icon(App.GetResourceStream(new Uri("/res/Launcher.ico", UriKind.Relative)).Stream);
                notifyIcon.Visible = true;
                notifyIcon.Click += NotifyIcon_Click;
            }
            else
                this.Close();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            MenuLauncher pop = new MenuLauncher(servicio);
            if (pop.ShowDialog() == true)
            {
                notifyIcon.Dispose(); notifyIcon = null;
                this.Close();
            }
        }
    }
}
