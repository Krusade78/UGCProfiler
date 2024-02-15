using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

namespace Launcher
{
    public class CMain
    {
        private NotifyIcon notifyIcon = null;
        private CService service = null;

        public CMain() { }

        public void Init()
        {
            CTranslate.Load();
            service = new CService(this);
            if (service.Init())
            {
                service.ExitEvt += Service_ExitEvt;
                System.Windows.Application.ResourceAssembly = typeof(CMain).Assembly;
                notifyIcon = new NotifyIcon
                {
                    Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("/res/Launcher.ico", UriKind.Relative)).Stream),
                    Visible = true
                };
                notifyIcon.Click += NotifyIcon_Click;
				System.Threading.Thread th = new(() =>
				{
                    //preload wpf libraries
					MenuLauncher popup = new(service);
					popup.Activate();
                    popup.Close();
				});
				th.SetApartmentState(System.Threading.ApartmentState.STA);
				th.Start();
			}
		}

        public void LoadDefault()
        {
			service.LoadProfile(null);
		}

        private void Service_ExitEvt(object sender, ResolveEventArgs e)
        {
            notifyIcon?.Dispose();
            service?.Dispose();
			//System.Windows.Application.Current.Shutdown();
        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            System.Threading.Thread th = new(MenuWnd);
            th.SetApartmentState(System.Threading.ApartmentState.STA);
			th.Start();
        }

        private void MenuWnd()
        {
			MenuLauncher popup = new(service);
			if (popup.ShowDialog() == true)
            {
                notifyIcon?.Dispose();
                service?.Dispose();
            }
        }

        public void MessageBox(string msj, string title, MessageBoxImage img)
        {
            if (notifyIcon == null)
            {
                System.Windows.MessageBox.Show(msj, title, MessageBoxButton.OK, img);
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
                notifyIcon.ShowBalloonTip(3000, title, msj, tti);
            }
        }
    }
}
