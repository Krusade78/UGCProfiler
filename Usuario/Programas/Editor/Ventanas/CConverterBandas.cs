using System;

namespace Editor
{
    [System.Windows.Data.ValueConversion(typeof(int), typeof(String))]
    internal class CConverterBandas : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)(double)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
