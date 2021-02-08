using System;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Input;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para Info.xaml
    /// </summary>
    internal partial class Info : UserControl
    {
        private readonly System.Collections.Generic.List<Key> shifted = new System.Collections.Generic.List<Key>();
        private readonly System.Collections.Generic.List<Key> teclas = new System.Collections.Generic.List<Key>();
        private readonly System.Collections.Generic.List<string> raton = new System.Collections.Generic.List<string>();
        private readonly System.Windows.Threading.DispatcherTimer borradoRueda = new System.Windows.Threading.DispatcherTimer();

        public Info()
        {
            InitializeComponent();
            borradoRueda.Interval = new TimeSpan(5000000);
        }

        private void FcheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(true);
        }
        private void FcheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            borradoRueda.Tick += BorradoRueda_Tick;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            borradoRueda.Tick -= BorradoRueda_Tick;
        }

        public void ActualizarEstado(byte[] hidData, bool pedales)
        {
            if (!pedales)
            {
                int x = (hidData[1] << 8) | hidData[0];
                int y = (hidData[3] << 8) | hidData[2];
                Labelxy.Text = x + " # " + y;
                ejeXY.Margin = new Thickness(x, y, 0, 0);

                int z = (hidData[5] << 8) | hidData[4];
                Labelz.Text = z.ToString();
                ejeZ.Height = z;

                int rx = (hidData[7] << 8) | hidData[6];
                Labelrx.Text = rx.ToString();
                double angulo = (Math.PI * (((double)360 / 2048) * rx)) / 180;
                String ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
                String ax = (rx == 2048) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
                ejeRX.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

                int ry = (hidData[9] << 8) | hidData[8];
                Labelry.Text = ry.ToString();
                angulo = (Math.PI * (((double)360 / 2048) * ry)) / 180;
                ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
                ax = (ry == 2048) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
                ejeRY.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

                int sl1 = (hidData[11] << 8) | hidData[10];
                Labelsl1.Text = sl1.ToString();
                ejeSl1.Width = sl1;

                System.Windows.Shapes.Ellipse[] bts = new System.Windows.Shapes.Ellipse[] { bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8,
                                                                                        bt9, bt10, bt11, bt12, bt13, bt14, bt15, bt16,
                                                                                        bt17, bt18, bt19, bt20, bt21, bt22, bt23, bt24,
                                                                                        bt25, bt26, bt27, bt28, bt29, bt30, bt31, bt32};
                for (int i = 0; i < 32; i++)
                {
                    bts[i].Visibility = ((hidData[16 + (i / 8)] & (1 << (i % 8))) != 0) ? Visibility.Visible : Visibility.Hidden;
                }

                pathP1.Visibility = (hidData[12] == 0) ? Visibility.Hidden : Visibility.Visible;
                pathP1.RenderTransform = new System.Windows.Media.RotateTransform((hidData[12] > 5) ? (hidData[12] - 9) * 45 : (hidData[12] - 1) * 45);
                pathP2.Visibility = (hidData[13] == 0) ? Visibility.Hidden : Visibility.Visible;
                pathP2.RenderTransform = new System.Windows.Media.RotateTransform((hidData[13] > 5) ? (hidData[13] - 9) * 45 : (hidData[13] - 1) * 45);
                pathP3.Visibility = (hidData[14] == 0) ? Visibility.Hidden : Visibility.Visible;
                pathP3.RenderTransform = new System.Windows.Media.RotateTransform((hidData[14] > 5) ? (hidData[14] - 9) * 45 : (hidData[14] - 1) * 45);
                pathP4.Visibility = (hidData[15] == 0) ? Visibility.Hidden : Visibility.Visible;
                pathP4.RenderTransform = new System.Windows.Media.RotateTransform((hidData[15] > 5) ? (hidData[15] - 9) * 45 : (hidData[15] - 1) * 45);
            }
            else
            {
                int r = (hidData[1] << 8) | hidData[0];
                Labelr.Text = r.ToString();
                ejeR.Width = r;

                int sl2 = (hidData[3] << 8) | hidData[2];
                Labelsl2.Text = sl2.ToString();
                ejeSl2.Width = sl2;

                int sl3 = (hidData[5] << 8) | hidData[4];
                Labelsl3.Text = sl3.ToString();
                ejeSl3.Width = sl3;

                int my = hidData[6] >> 4;
                int mx = hidData[6] & 0xf;
                Labelmxy.Text = "mX: " + mx + "\n" + "mY: " + my;
                ejeMXY.Margin = new Thickness(mx, my, 0, 0);
            }
        }

        public void ActualizarTeclado(CRawInput.RAWINPUTKEYBOARD data)
        {
            Key key = KeyInterop.KeyFromVirtualKey(data.VKey);
            if ((data.Flags & CRawInput.RawKeyboardFlags.KeyBreak) == 0)
            {
                if (key.ToString().Contains("Ctrl") || key.ToString().Contains("Alt") || key.ToString().Contains("Win"))
                {
                    if (shifted.IndexOf(key) == -1)  shifted.Add(key);
                }
                else
                {
                    if (teclas.IndexOf(key) == -1) teclas.Add(key);
                }
            }
            else
            {
                if (key.ToString().Contains("Ctrl") || key.ToString().Contains("Alt") || key.ToString().Contains("Win"))
                {
                    if (shifted.IndexOf(key) != -1) shifted.Remove(key);
                }
                else
                {
                    if (teclas.IndexOf(key) != -1) teclas.Remove(key);
                }
            }

            txtTeclado.Text = "";
            foreach (Key k in shifted)
            {
                txtTeclado.Text += " + " + k.ToString(); ;
            }
            foreach (Key k in teclas)
            {
                txtTeclado.Text += " + " + k.ToString();
            }
            if (txtTeclado.Text.StartsWith(" + "))
            {
                txtTeclado.Text = txtTeclado.Text.Remove(0, 3);
            }
        }

        public void ActualizarRaton(CRawInput.RAWINPUTMOUSE data)
        {
            string[] botones = new string[] { "Bt1", "-Bt1", "Bt2", "-Bt2", "Bt3", "-Bt3", "Bt4", "-Bt4", "Bt5", "-Bt5", "RuedaV", "RuedaH" };
            uint[] mapa = new uint[] { 0x01, 0x02, 0x4, 0x08, 0x10, 0x20, 0x40, 0x80, 0x100, 0x200, 0x400, 0x800 };
            string boton = "";
            for (int i = 0; i < mapa.Length; i++)
            {
                if ((mapa[i] & data.usButtonFlags) != 0)
                {
                    if (botones[i].StartsWith("-"))
                    {
                        if (raton.IndexOf(botones[i].Remove(0, 1)) != -1) raton.Remove(botones[i].Remove(0, 1));
                    }
                    else
                    {
                        string txt = botones[i] + ((data.usButtonData > 0) ? "+" : "");
                        txt += (data.usButtonData < 0) ? "-" : "";
                        if (raton.IndexOf(txt) == -1)
                        {
                            raton.Add(txt);
                        }
                        if ((data.usButtonData > 0) || (data.usButtonData < 0))
                        {
                            borradoRueda.Start();
                        }
                        
                    }
                    boton += botones[i];
                }
            }

            txtRaton.Text = "";
            foreach (string sr in raton)
            {
                txtRaton.Text += " " + sr;
            }
            if (txtTeclado.Text.StartsWith(" "))
            {
                txtTeclado.Text = txtTeclado.Text.Remove(0, 1);
            }
            txtRaton.Text = $"X: {data.lLastX} Y: {data.lLastY}" + txtRaton.Text;
        }

        private void BorradoRueda_Tick(object sender, EventArgs e)
        {
            int i = raton.Count - 1;
            while(i >= 0)
            {
                if (raton[i].StartsWith("RuedaV") || raton[i].StartsWith("RuedaH"))
                {
                    txtRaton.Text = txtRaton.Text.Replace(raton[i], "");
                    raton.RemoveAt(i);
                    
                }
                i--;
            }
            borradoRueda.Stop();
        }
    }
}
