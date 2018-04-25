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
        private static NotifyIcon notifyIcon = null;
        private CServicio servicio;

        public MainWindow()
        {
            InitializeComponent();
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

        internal static void MessageBox(String msj, String titulo, MessageBoxButton bt, MessageBoxImage img)
        {
            if (notifyIcon == null)
                System.Windows.MessageBox.Show(msj, titulo, MessageBoxButton.OK, img);
            else
            {
                ToolTipIcon tti;
                switch (img)
                {
                    case MessageBoxImage.Error:
                        tti = ToolTipIcon.Error;
                        break;
                    case MessageBoxImage.Warning:
                        tti = ToolTipIcon.Warning;
                        break;
                    case MessageBoxImage.Information:
                        tti = ToolTipIcon.Info;
                        break;
                    default:
                        tti = ToolTipIcon.None;
                        break;
                }
                notifyIcon.ShowBalloonTip(3000, titulo, msj, tti);
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
