using System;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace Calibrator
{
    /// <summary>
    /// Lógica de interacción para Info.xaml
    /// </summary>
    internal partial class Info : UserControl
    {
        public Info()
        {
            InitializeComponent();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(true);
        }
        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ((MainWindow)((Grid)((Grid)((Grid)this.Parent).Parent).Parent).Parent).SetModoRaw(false);
        }

        public void ActualizarEstado(byte[] hidData)
        {
            int x = (hidData[1] << 8) | hidData[0];
            int y = (hidData[3] << 8) | hidData[2];
            Labelxy.Text =  x + " # " + y;
            ejeXY.Margin = new Thickness(x, y, 0, 0);

            int r = (hidData[5] << 8) | hidData[4];
            Labelr.Text = r.ToString();
            ejeR.Width = r;

            int z = (hidData[7] << 8) | hidData[6];
            Labelz.Text = z.ToString();
            ejeZ.Height = z;

            int rx = (hidData[9] << 8) | hidData[8];
            Labelrx.Text = rx.ToString();
            double angulo = (Math.PI * (((double)360 / 2048) * rx)) / 180;
            String ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
            String ax = (rx == 2048) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
            ejeRX.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1": "0") + " 1 " + ax + "," + ay + " Z");

            int ry = (hidData[11] << 8) | hidData[10];
            Labelry.Text = ry.ToString();
            angulo = (Math.PI * (((double)360 / 2048) * ry)) / 180;
            ay = (25 - (Math.Cos(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
            ax = (ry == 2048) ? "24.999" : (25 + (Math.Sin(angulo) * 25)).ToString("0.##########", CultureInfo.InvariantCulture);
            ejeRY.Data = System.Windows.Media.Geometry.Parse("M25,25 L25,0 A25,25 0 " + ((angulo > Math.PI) ? "1" : "0") + " 1 " + ax + "," + ay + " Z");

            int sl1 = (hidData[13] << 8) | hidData[12];
            Labelsl1.Text = sl1.ToString();
            ejeSl1.Width = sl1;

            int sl2 = (hidData[15] >> 8) | hidData[14];
            Labelsl2.Text = sl2.ToString();
            ejeSl2.Width = sl2;

            int mx = hidData[24] >> 4;
            int my = hidData[24] & 0xf;
            Labelmxy.Text = "mX: " + mx + "\n" + "mY: " + my;
            ejeMXY.Margin = new Thickness(mx, my, 0, 0);

            System.Windows.Shapes.Ellipse[] bts = new System.Windows.Shapes.Ellipse[] { bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8,
                                                                                        bt9, bt10, bt11, bt12, bt13, bt14, bt15, bt16,
                                                                                        bt17, bt18, bt19, bt20, bt21, bt22, bt23, bt24,
                                                                                        bt25, bt26, bt27, bt28, bt29, bt30, bt31, bt32};
            for(int i = 0; i < 32; i++)
                bts[i].Visibility = ((hidData[20 + (i / 8)] & (1 << (i % 8))) != 0) ? Visibility.Visible : Visibility.Hidden;

            pathP1.Visibility = (hidData[16] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP1.RenderTransform = new System.Windows.Media.RotateTransform((hidData[16] > 5) ? (hidData[16] - 9) * 45 : (hidData[16] - 1) * 45);
            pathP2.Visibility = (hidData[17] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP2.RenderTransform = new System.Windows.Media.RotateTransform((hidData[17] > 5) ? (hidData[17] - 9) * 45 : (hidData[17] - 1) * 45);
            pathP3.Visibility = (hidData[18] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP3.RenderTransform = new System.Windows.Media.RotateTransform((hidData[18] > 5) ? (hidData[18] - 9) * 45 : (hidData[18] - 1) * 45);
            pathP4.Visibility = (hidData[19] == 0) ? Visibility.Hidden : Visibility.Visible;
            pathP4.RenderTransform = new System.Windows.Media.RotateTransform((hidData[19] > 5) ? (hidData[19] - 9) * 45 : (hidData[19] - 1) * 45);
        }

        public void ActualizarTeclado(data)
        {

        }

        public void ActualizarMouse(data)
    }
}
