using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ventanas
{
    /// <summary>
    /// Lógica de interacción para VCtlCuadrado.xaml
    /// </summary>
    internal partial class VColor : Window
    {
        private readonly TranslateTransform markerTransform = new();
        private bool sinEventos = false;
        private Point ultimaPos;
        private bool cargado = false;
        private Color colorRGB;

        public string ColorSt { get; set; } = "0;0;0";
        public Color ColorSB { get; set; } = Color.FromRgb(0, 0, 0);

        public VColor(string color)
        {
            InitializeComponent();
            ColorSt = color;
            colorRGB = Color.FromRgb((byte)((int.Parse(ColorSt.Split(';')[0]) * 255) / 7), (byte)((int.Parse(ColorSt.Split(';')[1]) * 255) / 7), (byte)((int.Parse(ColorSt.Split(';')[2]) * 255) / 7));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Color c = colorRGB;
            sldR.Value = 1; sldR.Value = c.R;
            sldG.Value = 1; sldG.Value = c.G;
            sldB.Value = 1; sldB.Value = c.B;
        }

        private void BtAceptar_Click(object sender, RoutedEventArgs e)
        {
            ColorSt = (colorRGB.R * 7 / 255) + ";" + (colorRGB.G * 7 / 255) + ";" + (colorRGB.B * 7 / 255);
            ColorSB = colorRGB;
            this.DialogResult = true;
            this.Close();
        }

        #region "Métodos privados"
        private static string RGB2String(byte r, byte g, byte b)
        {
            return string.Format(string.Format("{0:X2}", r) + string.Format("{0:X2}", g) + string.Format("{0:X2}", b));
        }
        //' Converts an RGB color to an HSV color.
        private static Colores.SliderColor.HsvColor ConvertRgbToHsv(int r, int b, int g)
        {
            double delta, min;
            double h = 0, s, v;

            min = Math.Min(Math.Min(r, g), b);
            v = Math.Max(Math.Max(r, g), b);
            delta = v - min;

            if (v == 0.0)
                s = 0;
            else
                s = delta / v;


            if (s != 0)
            {
                if (r == v)
                    h = (g - b) / delta;
                else if (g == v)
                    h = 2 + (b - r) / delta;
                else if (b == v)
                    h = 4 + (r - g) / delta;

                h *= 60;
                if (h < 0.0)
                    h += 360;
            }

            Colores.SliderColor.HsvColor hColor = new(h, s, v / 255);
            return hColor;
        }

        private void UpdateMarkerPosition(Color c)
        {
            Colores.SliderColor.HsvColor hsv = ConvertRgbToHsv(c.R, c.G, c.B);
            Point p = new(hsv.S, 1 - hsv.V);

            p.X *= PART_ColorDetail.Width;
            p.Y *= PART_ColorDetail.Height;
            markerTransform.X = p.X;
            markerTransform.Y = p.Y;
            ultimaPos = p;
        }

        private void UpdateMarkerPosition(Point p)
        {
            ultimaPos = p;
            if (p.X > PART_ColorDetail.Width)
                p.X = PART_ColorDetail.Width;
            if (p.Y > PART_ColorDetail.Height)
                p.Y = PART_ColorDetail.Height;
            if (p.X < 0)
                p.X = 0;
            if (p.Y < 0)
                p.Y = 0;
            markerTransform.X = p.X;
            markerTransform.Y = p.Y;
            p.X /= PART_ColorDetail.Width;
            p.Y /= PART_ColorDetail.Height;
            CogerColor(p);
        }

        private void CogerColor(Point p)
        {
            Colores.SliderColor.HsvColor hsv = new(360 - PART_ColorSlider.Value, 1, 1);
            hsv.S = p.X;
            hsv.V = 1 - p.Y;
            Color m_color = Colores.SliderColor.ConvertHsvToRgb(hsv.H, hsv.S, hsv.V);

            colorRGB = m_color;
            rSeleccionado.Fill = new SolidColorBrush(m_color);
            txtColor.Text = RGB2String(m_color.R, m_color.G, m_color.B);
            try
            {
                txtR.Text = Convert.ToByte(txtColor.Text.Substring(2, 2), 16).ToString();
            }
            catch { }
            try
            {
                txtG.Text = Convert.ToByte(txtColor.Text.Substring(4, 2), 16).ToString();
            }
            catch { }
            try
            {
                txtB.Text = Convert.ToByte(txtColor.Text.Substring(6, 2), 16).ToString();
            }
            catch { }
        }
        #endregion

        #region "Eventos"
        private void PART_ColorDetail_Loaded(object sender, RoutedEventArgs e)
        {
            PART_ColorMarker.RenderTransform = markerTransform;
            PART_ColorMarker.RenderTransformOrigin = new Point(0.5, 0.5);
            cargado = true;
        }

        private void Sld_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sinEventos) return;
            sinEventos = true;
            {
                txtColor.Text = RGB2String((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value);
                PART_ColorSlider.Value = ConvertRgbToHsv((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value).H;
            }
            sinEventos = false;
            colorRGB = Color.FromRgb((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value);
            rSeleccionado.Fill = new SolidColorBrush(colorRGB);
            UpdateMarkerPosition(Color.FromRgb((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value));
        }

        private void TxtColor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sinEventos) return;
            if (txtColor.Text.Length == 8)
            {
                sinEventos = true;
                {
                    try
                    {
                        txtR.Text = Convert.ToByte(txtColor.Text.Substring(2, 2), 16).ToString();
                    }
                    catch { }
                    try
                    {
                        txtG.Text = Convert.ToByte(txtColor.Text.Substring(4, 2), 16).ToString();
                    }
                    catch { }
                    try
                    {
                        txtB.Text = Convert.ToByte(txtColor.Text.Substring(6, 2), 16).ToString();
                    }
                    catch { }
                    PART_ColorSlider.Value = ConvertRgbToHsv((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value).H;
                }
                sinEventos = false;
                colorRGB = Color.FromRgb((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value);
                rSeleccionado.Fill = new SolidColorBrush(colorRGB);
                UpdateMarkerPosition(Color.FromRgb((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value));
            }
        }

        private void PART_ColorDetail_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sinEventos = true;
            UpdateMarkerPosition(e.GetPosition(PART_ColorDetail));
            sinEventos = false;
        }

        private void PART_ColorDetail_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                sinEventos = true;
                UpdateMarkerPosition(e.GetPosition(PART_ColorDetail));
                sinEventos = false;
            }
        }

        private void PART_ColorSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PART_ColorDetail.IsLoaded && !sinEventos)
                UpdateMarkerPosition(ultimaPos);
        }

        private void RectR_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                sldR.Value = (int)((e.GetPosition(rectR).X * 255) / rectR.ActualWidth);
        }

        private void RectR_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sldR.Value = (int)((e.GetPosition(rectR).X * 255) / rectR.ActualWidth);
        }

        private void RectG_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                sldG.Value = (int)((e.GetPosition(rectG).X * 255) / rectG.ActualWidth);
        }

        private void RectG_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sldG.Value = (int)((e.GetPosition(rectG).X * 255) / rectG.ActualWidth);
        }

        private void RectB_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                sldB.Value = (int)((e.GetPosition(rectB).X * 255) / rectB.ActualWidth);
        }

        private void RectB_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sldB.Value = (int)((e.GetPosition(rectB).X * 255) / rectB.ActualWidth);
        }

        private void CbPredefinidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cargado) return;
            sinEventos = true;
            {
                try
                {
                    txtR.Text = Convert.ToByte(((string)((ComboBoxItem)cbPredefinidos.SelectedItem).Tag).Substring(2, 2), 16).ToString();
                }
                catch { }
                try
                {
                    txtG.Text = Convert.ToByte(((string)((ComboBoxItem)cbPredefinidos.SelectedItem).Tag).Substring(4, 2), 16).ToString();
                }
                catch { }
                try
                {
                    txtB.Text = Convert.ToByte(((string)((ComboBoxItem)cbPredefinidos.SelectedItem).Tag).Substring(6, 2), 16).ToString();
                }
                catch { }
                txtColor.Text = RGB2String((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value);
                PART_ColorSlider.Value = ConvertRgbToHsv((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value).H;
            }
            sinEventos = false;
            colorRGB = Color.FromRgb((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value);
            rSeleccionado.Fill = new SolidColorBrush(colorRGB);
            UpdateMarkerPosition(Color.FromRgb((byte)sldR.Value, (byte)sldG.Value, (byte)sldB.Value));
        }
        #endregion
    }
}
