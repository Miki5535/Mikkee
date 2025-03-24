using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Mikkee.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Check if value and parameter are not null before using them
            if (value is bool booleanValue && parameter is string param)
            {
                return booleanValue.ToString() == param ? true : false;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
