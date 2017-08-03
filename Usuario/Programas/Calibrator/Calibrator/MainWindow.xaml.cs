using System;
using System.Runtime.InteropServices;
using System.Windows;


namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("");
            
            uint n = 0;
            IntPtr hJoy = IntPtr.Zero;

            ucInfo.Iniciar(hJoy);
            //ucCalibrar.Iniciar(hJoy);
        }

        private void toggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                //ev = true;
                //tbCalibrar.IsChecked = false;
                //ev = false;
            }
        }

        private void toggleButton1_Checked(object sender, RoutedEventArgs e)
        {
                //ev = true;
                //tbPrueba.IsChecked = false;
                //ev = false;
        }
    }
}
