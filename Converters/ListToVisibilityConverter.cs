using System.Collections;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Mikkee.Converters
{
    public class ListToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // ตรวจสอบค่า null ก่อนใช้งาน
            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}