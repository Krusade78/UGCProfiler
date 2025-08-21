using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Profiler.Controls
{
    public class BooleanToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
