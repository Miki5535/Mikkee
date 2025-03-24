
using System.Globalization;

namespace Mikkee.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            // Check if value is a boolean and parameter is a string
            if (value is bool isVisible && parameter is string param)
            {
                return isVisible == bool.Parse(param);
            }
            return false;
        }

        public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            // Reverse the logic for two-way binding (if needed)
            if (value is bool isVisible && parameter is string param)
            {
                return isVisible != bool.Parse(param);
            }
            return null;
        }
    }
}