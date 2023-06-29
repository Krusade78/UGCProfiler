using System;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace Ventanas.Colores
{
    internal class SliderColor : System.Windows.Controls.Slider
    {
        private System.Windows.Shapes.Rectangle m_spectrumDisplay;
        private LinearGradientBrush pickerBrush;

        static SliderColor()
        {
            //'Esta llamada a OverrideMetadata indica al sistema que este elemento desea proporcionar un estilo diferente al de su clase base.
            //'Este estilo se define en themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderColor), new FrameworkPropertyMetadata(typeof(SliderColor)));
        }

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color), typeof(SliderColor), new PropertyMetadata(Colors.Transparent));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            m_spectrumDisplay = (System.Windows.Shapes.Rectangle)GetTemplateChild("PART_SpectrumDisplay");
            if (m_spectrumDisplay != null) CreateSpectrum();
            OnValueChanged(Double.NaN, Value);
        }

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            Color theColor = ConvertHsvToRgb(360 - newValue, 1, 1);
            SetValue(SelectedColorProperty, theColor);
        }

        private void CreateSpectrum()
        {
            pickerBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation
            };

            List<Color> colorsList = GenerateHsvSpectrum();
            double stopIncrement = 1D / colorsList.Count;

            int i;
            for (i = 0; i < colorsList.Count; i++)
                pickerBrush.GradientStops.Add(new GradientStop(colorsList[i], i * stopIncrement));

            pickerBrush.GradientStops[i - 1].Offset = 1.0;
            m_spectrumDisplay.Fill = pickerBrush;
        }

        #region "Conversiones de color"
            public class HsvColor
            {
                public double H;
                public double S;
                public double V;

                public HsvColor(double h, double s, double v)
                {
                    this.H = h;
                    this.S = s;
                    this.V = v;
                }
            }
            // Converts an HSV color to an RGB color.
            public static Color ConvertHsvToRgb(double h, double s, double v) 
            {

                double r, g, b;

                if (s == 0)
                {
                    r = v;
                    g = v;
                    b = v;
                }
                else
                {
                    int i;
                    double f, p, q, t;

                    if (h == 360)
                        h = 0;
                    else
                        h /= 60;

                    i = (int)Math.Truncate(h);
                    f = h - i;

                    p = v * (1.0 - s);
                    q = v * (1.0 - (s * f));
                    t = v * (1.0 - (s * (1.0 - f)));

                    switch(i)
                    {
                        case 0:
                            r = v;
                            g = t;
                            b = p;
                            break;
                        case 1:
                            r = q;
                            g = v;
                            b = p;
                            break;
                        case 2:
                            r = p;
                            g = v;
                            b = t;
                            break;
                        case 3:
                            r = p;
                            g = q;
                            b = v;
                            break;
                        case 4:
                            r = t;
                            g = p;
                            b = v;
                            break;
                        default:
                            r = v;
                            g = p;
                            b = q;
                            break;
                    }
                }

                return Color.FromArgb(255, (byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
            }
            // Generates a list of colors with hues ranging from 0 360
            // and a saturation and value of 1. 
            private static List<Color> GenerateHsvSpectrum()
            {
                List<Color> colorsList = new();

                for (int i = 0; i < 29; i++)
                    colorsList.Add(ConvertHsvToRgb(i * 12, 1, 1));

                colorsList.Add(ConvertHsvToRgb(0, 1, 1));

                return colorsList;
            }
        #endregion
    }
}