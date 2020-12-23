using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Launcher
{
    internal class CMain
    {
        private static NotifyIcon notifyIcon = null;
        private static CServicio servicio = null;

        public static void Iniciar()
        {
            servicio = new CServicio();
            if (servicio.Iniciar())
            {
                notifyIcon = new NotifyIcon
                {
                    Icon = new Icon(App.GetResourceStream(new Uri("/res/Launcher.ico", UriKind.Relative)).Stream),
                    Visible = true
                };
                notifyIcon.Click += NotifyIcon_Click;
            }
        }

        private static void NotifyIcon_Click(object sender, EventArgs e)
        {
            MenuLauncher pop = new MenuLauncher(servicio);
            if (pop.ShowDialog() == true)
            {
                notifyIcon?.Dispose();
                servicio?.Dispose();
                App.Current.Shutdown();
            }
        }

        public static void MessageBox(String msj, String titulo, MessageBoxButton bt, MessageBoxImage img)
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
    }
}
