using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Launcher
{
    public class CMain
    {
        private NotifyIcon notifyIcon = null;
        private CServicio servicio = null;

        public void Iniciar()
        {
            servicio = new CServicio(this);
            if (servicio.Iniciar())
            {
                servicio.EvtSalir += Servicio_EvtSalir;
                System.Windows.Application.ResourceAssembly = typeof(CMain).Assembly;
                notifyIcon = new NotifyIcon
                {
                    Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("/res/Launcher.ico", UriKind.Relative)).Stream),
                    Visible = true
                };
                notifyIcon.Click += NotifyIcon_Click;
            }
        }

        private void Servicio_EvtSalir(object sender, ResolveEventArgs e)
        {
            notifyIcon?.Dispose();
            servicio?.Dispose();
            //App.Current.Shutdown();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            System.Threading.Thread th = new(MenuWnd);
            th.SetApartmentState(System.Threading.ApartmentState.STA);
            th.Start();
        }

        private void MenuWnd()
        {
            //System.Threading.ApartmentState old = System.Threading.Thread.CurrentThread.GetApartmentState();
            //System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
            MenuLauncher pop = new(servicio);
            if (pop.ShowDialog() == true)
            {
                notifyIcon?.Dispose();
                servicio?.Dispose();
                //App.Current.Shutdown();
            }
            //System.Threading.Thread.CurrentThread.SetApartmentState(old);
        }

        public void MessageBox(string msj, string titulo, MessageBoxButton bt, MessageBoxImage img)
        {
            if (notifyIcon == null)
            {
                System.Windows.MessageBox.Show(msj, titulo, MessageBoxButton.OK, img);
            }
            else
            {
                var tti = img switch
                {
                    MessageBoxImage.Error => ToolTipIcon.Error,
                    MessageBoxImage.Warning => ToolTipIcon.Warning,
                    MessageBoxImage.Information => ToolTipIcon.Info,
                    _ => ToolTipIcon.None,
                };
                notifyIcon.ShowBalloonTip(3000, titulo, msj, tti);
            }
        }
    }
}
