using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;


namespace Launcher
{
    /// <summary>
    /// Lógica de interacción para UserControl1.xaml
    /// </summary>
    internal partial class MenuLauncher : Window
    {
        private bool salir = false;
        private readonly CService svc;

        public MenuLauncher(CService svc)
        {
            this.svc = svc;
            InitializeComponent();
			Resources.MergedDictionaries.RemoveAt(0);
			Resources.MergedDictionaries.Add(CTranslate.GetDictionary());
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilesList();
            this.Activate();
            this.ContextMenu.IsOpen = true;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (!salir)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            salir = true;
            this.DialogResult = true;
            this.Close();
        }

        private void LoadFilesList()
        {
            foreach (string f in Directory.GetFiles(".", "*.xhp"))
            {
                MenuItem miL = new()
                {
                    Header = Path.GetFileName(f).Remove(Path.GetFileName(f).Length - 4, 4)
                };
                miL.Click += MenuItemLaunch_Click;
                mnLaunch.Items.Add(miL);
                MenuItem miE = new()
                {
                    Header = Path.GetFileName(f).Remove(Path.GetFileName(f).Length - 4, 4)
                };
                miE.Click += MenuItemEdit_Click;
                mnEdit.Items.Add(miE);
            }
        }

        private void MenuItemLaunch_Click(object sender, RoutedEventArgs e)
        {
            svc.LoadProfile(Directory.GetCurrentDirectory() + "\\" + (String)((MenuItem)sender).Header + ".xhp");
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Profiler.exe", "\"" + (String)((MenuItem)sender).Header + ".xhp\"");
            }
            catch { }
        }

        private void MenuProfiler_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("Profiler.exe");
            }
            catch { }
        }

        private void MenuReset_Click(object sender, RoutedEventArgs e)
        {
            svc.LoadProfile(null);
        }
    }
}
