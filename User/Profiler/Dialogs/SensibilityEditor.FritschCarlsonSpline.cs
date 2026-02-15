using System;
using System.Collections.Generic;
using System.Linq;

namespace Profiler.Dialogs
{
    internal partial class SensibilityEditor
    {
        private sealed class FritschCarlsonSpline
        {
            private readonly double[] _x;
            private readonly double[] _y;
            private readonly double[] _m;
            private readonly byte _n;

            public FritschCarlsonSpline(List<double> x, List<double> y)
            {
                x.Insert(0, 0);
                y.Insert(0, 0);
                _n = (byte)x.Count;
                _x = [.. x];
                _y = [.. y];
                x.RemoveAt(0);
                y.RemoveAt(0);
                _m = new double[_n];

                ComputeSlopes();
            }

            public double[] GetSlopes() => _m;

            // =============================
            //  Fritsch–Carlson core
            // =============================
            private void ComputeSlopes()
            {
                int n = _n;
                double[] d = new double[n - 1];

                for (byte i = 0; i < n - 1; i++)
                {
                    double h = _x[i + 1] - _x[i];
                    d[i] = (_y[i + 1] - _y[i]) / h;
                }

                _m[0] = d[0];
                _m[n - 1] = d[n - 2];

                for (int i = 1; i < n - 1; i++)
                {
                    if (d[i - 1] * d[i] <= 0)
                    {
                        _m[i] = 0;
                    }
                    else
                    {
                        double w1 = 2 * (_x[i + 1] - _x[i]) + (_x[i] - _x[i - 1]);
                        double w2 = (_x[i + 1] - _x[i]) + 2 * (_x[i] - _x[i - 1]);
                        _m[i] = (w1 + w2) / (w1 / d[i - 1] + w2 / d[i]);
                    }
                }
            }

            public double Evaluate(double x)
            {
                int i = 0;
                while (_x[i + 1] < x)
                    i++;

                double h = _x[i + 1] - _x[i];
                double t = (x - _x[i]) / h;

                double t2 = t * t;
                double t3 = t2 * t;

                // Cubic Hermite
                return
                    (2 * t3 - 3 * t2 + 1) * _y[i] +
                    (t3 - 2 * t2 + t) * h * _m[i] +
                    (-2 * t3 + 3 * t2) * _y[i + 1] +
                    (t3 - t2) * h * _m[i + 1];
            }
        }
    }
}
