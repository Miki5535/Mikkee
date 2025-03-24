using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Mikkee.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // ตรวจสอบว่า value ไม่เป็น null และเป็น boolean
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            // หาก value เป็น null หรือไม่ใช่ boolean ให้คืนค่า null
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // ตรวจสอบว่า value ไม่เป็น null และเป็น boolean
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            // หาก value เป็น null หรือไม่ใช่ boolean ให้คืนค่า null
            return null;
        }
    }
}