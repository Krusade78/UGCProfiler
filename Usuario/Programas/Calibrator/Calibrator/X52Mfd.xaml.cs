using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para X52Cal.xaml
    /// </summary>
    public partial class X52Mfd : UserControl
    {
        private byte luzMFD = 1;
        private byte luz = 1;

        public X52Mfd()
        {
            InitializeComponent();
            LeerRegistro();
        }

        private void LeerRegistro()
        {
            Microsoft.Win32.RegistryKey reg = null;
            int hora;
            byte[] buff = new byte[3] {0,0,0};
            try
            {
                reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\XHOTAS\\Calibrado", false);
                if (reg != null)
                {
                    luzMFD = ((byte[])reg.GetValue("LuzMFD", new byte[] {1}))[0];
                    luz = ((byte[])reg.GetValue("Luz", new byte[] {1}))[0];
                    buff = (byte[])reg.GetValue("Hora1", new byte[] {0, 0, 0});
                    if (buff[0] == 1) CheckBox1.IsChecked = true; else CheckBox1.IsChecked = false;
                    hora = ((int)buff[2] << 8) + buff[1];
                    if (hora > 32767) hora = hora - 65536;
                    NumericUpDown1.Value = hora / 60;
                    NumericUpDown2.Value = hora % 60;

                    buff = (byte[])reg.GetValue("Hora2", new byte[]  {0, 0, 0});
                    if (buff[0] == 1) CheckBox1.IsChecked = true; else CheckBox1.IsChecked = false;
                    if (buff[1] > 3)
                    {
                        NumericUpDown3.Value = -(((buff[1] - 4) * 256) + buff[2]) / 60;
                        NumericUpDown4.Value = (((buff[1] - 4) * 256) + buff[2]) % 60;
                    }
                    else
                    {
                        NumericUpDown3.Value = ((buff[1] * 256) + buff[2]) / 60;
                        NumericUpDown4.Value = ((buff[1] * 256) + buff[2]) % 60;
                    }

                    buff = (byte[])reg.GetValue("Hora3", new byte[]  {0, 0, 0});
                    if (buff[0] == 1) CheckBox1.IsChecked = true; else CheckBox1.IsChecked = false;
                    if (buff[1] > 3)
                    {
                        NumericUpDown5.Value = -(((buff[1] - 4) * 256) + buff[2]) / 60;
                        NumericUpDown6.Value = (((buff[1] - 4) * 256) + buff[2]) % 60;
                    }
                    else
                    {
                        NumericUpDown5.Value = ((buff[1] * 256) + buff[2]) / 60;
                        NumericUpDown6.Value = ((buff[1] * 256) + buff[2]) % 60;
                    }
                    reg.Close();
                }
            }
            catch (Exception ex)
            {
                if (reg != null) reg.Close();
                MessageBox.Show(ex.Message, "XCalibrator", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            RadioButton[] checks = new RadioButton[] {RadioButton1, RadioButton2, RadioButton3};
            BitmapImage[] imgs = new BitmapImage[] { new BitmapImage(new Uri("pack://application:,,,/res/b0.jpg")), new BitmapImage(new Uri("pack://application:,,,/res/b1.jpg")), new BitmapImage(new Uri("pack://application:,,,/res/b2.jpg")) };
            checks[luz].IsChecked = true;
            PictureBox1.Source = imgs[luz];
            RadioButton[] checks2 = new RadioButton[]{RadioButton4, RadioButton5, RadioButton6};
            BitmapImage[] imgs2 = new BitmapImage[] { new BitmapImage(new Uri("pack://application:,,,/res/mfd1.png")), new BitmapImage(new Uri("pack://application:,,,/res/mfd2.png")), new BitmapImage(new Uri("pack://application:,,,/res/mfd3.png")) };
            checks2[luzMFD].IsChecked = true;
            PictureBox2.Source = imgs2[luzMFD];
        }

        private void GuardarRegistro()
        {
            Microsoft.Win32.RegistryKey reg = null;
            byte[] buff = new byte[3];
            byte[] b = new byte[1];
            Int16 hora;
            try
            {
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\XHOTAS\\Calibrado");
                reg = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\XHOTAS\\Calibrado", true);
                if (reg != null)
                {
                    b[0] = luzMFD;
                    reg.SetValue("LuzMFD", b, Microsoft.Win32.RegistryValueKind.Binary);
                    b[0] = luz;
                    reg.SetValue("Luz", b, Microsoft.Win32.RegistryValueKind.Binary);

                    if (CheckBox1.IsChecked == true) buff[0] = 1; else buff[0] = 0;
                    if (NumericUpDown1.Value < 0) hora = (Int16) ((NumericUpDown1.Value * 60) - NumericUpDown2.Value); else hora = (Int16) ((NumericUpDown1.Value * 60) + NumericUpDown2.Value);
                    buff[2] = (byte)((hora >> 8) & 255);
                    buff[1] = (byte)(hora & 255);
                    reg.SetValue("Hora1", buff, Microsoft.Win32.RegistryValueKind.Binary);

                    if (CheckBox2.IsChecked == true) buff[0] = 1; else buff[0] = 0;
                    if (NumericUpDown3.Value < 0)
                    {
                        buff[1] = (byte)((((-NumericUpDown3.Value * 60) + NumericUpDown4.Value) / 256) + 4);
                        buff[2] = (byte)(((-NumericUpDown3.Value * 60) + NumericUpDown4.Value) % 256);
                    }
                    else
                    {
                        buff[1] = (byte)(((NumericUpDown3.Value * 60) + NumericUpDown4.Value) / 256);
                        buff[2] = (byte)(((NumericUpDown3.Value * 60) + NumericUpDown4.Value) % 256);
                    }
                    reg.SetValue("Hora2", buff, Microsoft.Win32.RegistryValueKind.Binary);

                    if (CheckBox3.IsChecked == true) buff[0] = 1; else buff[0] = 0;
                    if (NumericUpDown5.Value < 0)
                    {
                        buff[1] = (byte)((((-NumericUpDown5.Value * 60) + NumericUpDown6.Value) / 256) + 4);
                        buff[2] = (byte)(((-NumericUpDown5.Value * 60) + NumericUpDown6.Value) % 256);
                    }
                    else
                    {
                        buff[1] = (byte)(((NumericUpDown5.Value * 60) + NumericUpDown6.Value) / 256);
                        buff[2] = (byte)(((NumericUpDown5.Value * 60) + NumericUpDown6.Value) % 256);
                    }
                    reg.SetValue("Hora3", buff, Microsoft.Win32.RegistryValueKind.Binary);

                    reg.Close();
                }
            }
            catch (Exception ex)
            {
                if (reg != null) reg.Close();
                MessageBox.Show(ex.Message, "XCalibrator", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        [System.Runtime.InteropServices.DllImport("directinput.dll")]
        private static extern byte EnviarConfiguracionX52();

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            GuardarRegistro();
            EnviarConfiguracionX52();
        }

        private void RadioButton1_Checked(object sender, RoutedEventArgs e)
        {
                luz = 0;
                PictureBox1.Source = new BitmapImage(new Uri("pack://application:,,,/res/b0.jpg"));
        }

        private void RadioButton2_Checked(object sender, RoutedEventArgs e)
        {
                luz = 1;
                PictureBox1.Source = new BitmapImage(new Uri("pack://application:,,,/res/b1.jpg"));
        }

        private void RadioButton3_Checked(object sender, RoutedEventArgs e)
        {
                luz = 2;
                PictureBox1.Source = new BitmapImage(new Uri("pack://application:,,,/res/b2.jpg"));
        }

        private void RadioButton4_Checked(object sender, RoutedEventArgs e)
        {
                luzMFD = 0;
                PictureBox2.Source = new BitmapImage(new Uri("pack://application:,,,/res/mfd1.png"));
        }

        private void RadioButton5_Checked(object sender, RoutedEventArgs e)
        {
                luzMFD = 1;
                PictureBox2.Source = new BitmapImage(new Uri("pack://application:,,,/res/mfd2.png"));
        }

        private void RadioButton6_Checked(object sender, RoutedEventArgs e)
        {
                luzMFD = 2;
                PictureBox2.Source = new BitmapImage(new Uri("pack://application:,,,/res/mfd3.png"));
        }
    }
}
