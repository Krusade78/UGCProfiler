using System;

namespace Profiler.Dialogs
{
    internal class SliderToModelConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double sliderValue)
            {
                return sliderValue * 2.0;
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double sliderValue)
            {
                return sliderValue / 2.0;
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }

    internal class DoubleToStringConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double sliderValue)
            {
                return parameter == null ? $"{sliderValue:F1}" : $"{sliderValue:F2}";
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }

    internal class BoolToVisibilityConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool x)
            {
                return x ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }

    internal class NotBoolToVisibilityConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool x)
            {
                return x ? Microsoft.UI.Xaml.Visibility.Collapsed : Microsoft.UI.Xaml.Visibility.Visible;
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }

    internal class SliderCToDoubleConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double sliderValue)
            {
                return sliderValue * 100.0;
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double sliderValue)
            {
                return sliderValue / 100.0;
            }
            return Microsoft.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }
}
