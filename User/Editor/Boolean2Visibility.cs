using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Profiler
{
	public sealed class Boolean2VisibilityConverter : IValueConverter
	{
		object IValueConverter.Convert(object value, Type targetType, object parameter, string language)
		{
			if (!value.GetType().Equals(typeof(bool)))
			{
				throw new ArgumentException("Only Boolean is supported");
			}
			if (targetType.Equals(typeof(Visibility)))
			{
				return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
			}
			else
			{
				throw new ArgumentException("Unsuported type {0}", targetType.FullName);
			}
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (Visibility)value != Visibility.Visible;
		}
	}

	public sealed class Boolean2VisibilityInverse : IValueConverter
    {
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			return (bool?)value == true ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			return (Visibility)value == Visibility.Visible;
		}
	}
}
