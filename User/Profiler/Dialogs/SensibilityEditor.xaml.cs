using System;
using System.Linq;

namespace Profiler.Dialogs
{
    #region MVVM Model
    [Microsoft.UI.Xaml.Data.Bindable]
    public class SensibilityEditorColumnModel(SensibilityEditorModel.DoubleItem x, SensibilityEditorModel.DoubleItem y, bool last)
    {
        public SensibilityEditorModel.DoubleItem X { get; } = x;
        public SensibilityEditorModel.DoubleItem Y { get; } = y;
        public bool NotLast { get; } = !last;
    }

    [Microsoft.UI.Xaml.Data.Bindable]
    public class SensibilityEditorModel
    {
        public class DoubleItem : System.ComponentModel.INotifyPropertyChanged
        {
            private double _value;
            public double Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        PropertyChanged?.Invoke(this, new(nameof(Value)));
                    }
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

            public DoubleItem(double f) { Value = f; }
        }
        public class ByteItem : System.ComponentModel.INotifyPropertyChanged
        {
            private byte _value;
            public byte Value
            {
                get => _value;
                set
                {
                    if (_value != value)
                    {
                        _value = value;
                        PropertyChanged?.Invoke(this, new(nameof(Value)));
                    }
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

            public ByteItem(byte f) { Value = f; }
        }

        public System.Collections.ObjectModel.ObservableCollection<SensibilityEditorColumnModel> Columns { get; } = [];
        public DoubleItem Blend { get; set; } = new(0.5);
        public DoubleItem Exponent { get; set; } = new(1.5);
        public DoubleItem DampingK { get; set; } = new(0.25);
        //public DoubleItem Inertia { get; set; } = new(0.1);
        public ByteItem SoftDeadZone { get; set; } = new(0);
    }
    #endregion

    internal partial class SensibilityEditor : Microsoft.UI.Xaml.Controls.Frame
    {
        private readonly MainPage parent = ((App)Microsoft.UI.Xaml.Application.Current).GetMainPage();
        private readonly Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData;
        private readonly SensibilityEditorModel model = new();

#if DEBUG
        public SensibilityEditor() { InitializeComponent(); axisData = new(); }
#endif
        private SensibilityEditor(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData)
        {
            InitializeComponent();
            this.DataContext = model;
            this.axisData = axisData;
            Init();
        }

        public static async System.Threading.Tasks.Task Show(Shared.ProfileModel.AxisMapModel.ModeModel.AxisModel axisData)
        {
            SensibilityEditor content = new(axisData);

            // Create a new style based on the previous
            Microsoft.UI.Xaml.Controls.ContentDialog dlg = new()
            {
                // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
                XamlRoot = ((App)Microsoft.UI.Xaml.Application.Current).GetRoot(),
                Style = Microsoft.UI.Xaml.Application.Current.Resources["DefaultContentDialogStyle"] as Microsoft.UI.Xaml.Style,
                Title = Translate.Get("sensibility_curve"),
                PrimaryButtonText = Translate.Get("ok"),
                SecondaryButtonText = Translate.Get("cancel"),
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
                Content = content
            };
            dlg.Resources["ContentDialogMaxWidth"] = 960;
            dlg.Resources["ContentDialogMaxHeight"] = 960;

            if (await dlg.ShowAsync() == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                content.Save();
            }
        }

        private void Init()
        {
            for (byte i = 0; i < 20; i++)
            {
                if (!axisData.IsSensibilityAsCurve)
                {
                    model.Columns.Add(new(new(axisData.SensibilityX[i] * 100.0), new(axisData.SensibilityY[i] * 100.0), i == 19));
                }
                else
                {
                    model.Columns.Add(new(new((i + 1) * 5), new((i + 1) * 5), i == 19));
                }
            }
            chkCurve.IsOn = axisData.IsSensibilityAsCurve;
            chkSlider.IsOn = axisData.IsSensibilityForSlider;
            model.Blend.Value = axisData.Curve.Blend;
            model.Exponent.Value = axisData.Curve.Exponent;
            model.DampingK.Value = axisData.DamplingK;
            //model.Inertia.Value = axisData.Intertia;
            model.SoftDeadZone.Value = axisData.SoftDeadZone;
        }

        private void Save()
        {
            System.Collections.Generic.List<double> xs = [];
            System.Collections.Generic.List<double> ys = [];
            if (chkCurve.IsOn)
            {
                SampleOptimal(model.Exponent.Value, model.Blend.Value, xs, ys);
                axisData.Curve = new() { Blend = model.Blend.Value, Exponent = model.Exponent.Value };
                axisData.IsSensibilityAsCurve = true;
            }
            else
            {
                xs = [.. model.Columns.Select(x => x.X.Value)];
                ys = [.. model.Columns.Select(y => y.Y.Value)];
                axisData.IsSensibilityAsCurve = false;
            }

            FritschCarlsonSpline fSp = new(xs, ys);
            double[] s = fSp.GetSlopes();
            for (byte i = 0; i < 20; i++)
            {
                axisData.SensibilityS[i] = s[i];
                axisData.SensibilityX[i] = xs[i] / 100;
                axisData.SensibilityY[i] = ys[i] / 100;
            }

            axisData.IsSensibilityForSlider = chkSlider.IsOn == true;
            axisData.DamplingK = model.DampingK.Value;
            axisData.SoftDeadZone = model.SoftDeadZone.Value;
            parent.GetData().Modified = true;
        }

        private void Slider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            byte i;
            if (((Microsoft.UI.Xaml.FrameworkElement)sender).DataContext is SensibilityEditorColumnModel col)
            {
                i = (byte)model.Columns.IndexOf(col);
            }
            else
            {
                i = 19;
            }

            if (i < 19)
            {
                if (model.Columns[i].Y.Value > model.Columns[i + 1].Y.Value)
                {
                    model.Columns[i].Y.Value = model.Columns[i + 1].Y.Value;
                }
            }
            if (i > 0)
            {
                if (model.Columns[i].Y.Value < model.Columns[i - 1].Y.Value)
                {
                    model.Columns[i].Y.Value = model.Columns[i - 1].Y.Value;
                }
            }
            RefreshDraw();
        }

        private void BtXl_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (((Microsoft.UI.Xaml.FrameworkElement)sender).DataContext is SensibilityEditorColumnModel col)
            {
                byte i = (byte)model.Columns.IndexOf(col);
                if ((i == 0) && (model.Columns[i].X.Value != 0.5))
                {
                    model.Columns[i].X.Value -= 0.5;
                    RefreshDraw();
                }
                else if ((i != 0) && ((model.Columns[i].X.Value - 0.5) > model.Columns[i - 1].X.Value))
                {
                    model.Columns[i].X.Value -= 0.5;
                    RefreshDraw();
                }
            }
        }

        private void BtXg_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (((Microsoft.UI.Xaml.FrameworkElement)sender).DataContext is SensibilityEditorColumnModel col)
            {
                byte i = (byte)model.Columns.IndexOf(col);
                if ((model.Columns[i].X.Value + 0.5) < model.Columns[i + 1].X.Value)
                {
                    model.Columns[i].X.Value += 0.5;
                    RefreshDraw();
                }
            }
        }

        private void RefreshDraw()
        {
            SplineToGeometry(DrawUserCurve, [.. model.Columns.Select(x => x.X.Value)], [.. model.Columns.Select(y => y.Y.Value)]);
        }

        private static void SplineToGeometry(Microsoft.UI.Xaml.Shapes.Path path, System.Collections.Generic.List<double> xs, System.Collections.Generic.List<double> ys)
        {
            FritschCarlsonSpline fSp = new(xs, ys);
            System.Collections.Generic.List<Windows.Foundation.Point> points = [];
            for (double x = 0; x <= 100; x += 0.25)
            {
                double y = fSp.Evaluate(x);
                points.Add(new(x, 100 - y));
            }

            Microsoft.UI.Xaml.Media.PathGeometry geometry = new();
            Microsoft.UI.Xaml.Media.PathFigure figure = new()
            {
                StartPoint = points[0],
                IsClosed = false
            };

            for (short i = 1; i < points.Count; i++)
            {
                figure.Segments.Add(new Microsoft.UI.Xaml.Media.LineSegment
                {
                    Point = points[i],
                });
            }
            geometry.Figures.Add(figure);
            path.Data = geometry;
        }

        private void SliderP_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {            
            RefreshPDraw();
        }

        private void RefreshPDraw()
        {
            System.Collections.Generic.List<double> xs = [];
            System.Collections.Generic.List<double> ys = [];
            SampleOptimal(model.Exponent.Value, model.Blend.Value, xs, ys);
            SplineToGeometry(DrawParamCurve, xs, ys);
        }

        private static void SampleOptimal(double exponent, double blend, System.Collections.Generic.List<double> xs, System.Collections.Generic.List<double> ys)
        {
            const byte N = 200; // internal resolution
            double[] xFull = new double[N];
            double[] yFull = new double[N];
            double[] curvature = new double[N];

            // step 1 - calculate curve
            for (int i = 0; i < N; i++)
            {
                double t = (double)i / (N - 1);
                xFull[i] = t;
                yFull[i] = (1.0 - blend) * t + blend * Math.Pow(t, exponent);
            }

            // step 2 - aproximated numeric curvature
            for (int i = 1; i < N - 1; i++)
            {
                double yPrev = yFull[i - 1];
                double yCurr = yFull[i];
                double yNext = yFull[i + 1];

                curvature[i] = Math.Abs(yNext - 2 * yCurr + yPrev);
            }

            // leave zeros apart
            for (int i = 0; i < N; i++)
            {
                curvature[i] = Math.Max(curvature[i], 1e-6);
            }

            // step 3 - normalize and distribute points
            double totalCurv = curvature.Sum();
            double target = totalCurv / (21);

            System.Collections.Generic.SortedSet<int> selected = [];
            selected.Add(0);

            double acc = 0.0;
            for (int i = 1; i < N - 1; i++)
            {
                acc += curvature[i];

                if (acc >= target)
                {
                    selected.Add(i);
                    acc = 0;
                }
            }

            for (int i = 10; (i < N - 1) && (selected.Count < 20); i += 10)
            {
                while (selected.Contains(i))
                {
                    i++;
                }
                selected.Add(i);
            }

            selected.Add(N - 1);
            selected.Remove(0);

            // step 4 - convert to list
            xs.AddRange(selected.Select(i => xFull[i] * 100));
            ys.AddRange(selected.Select(i => yFull[i] * 100));
        }

        private void ChkCurve_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (chkCurve.IsOn)
            {
                gParametric.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                gCustom.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                gParametric.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                gCustom.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            }
        }
    }
}
