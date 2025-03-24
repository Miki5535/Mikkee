using System;
using System.Globalization;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Mikkee.Models;

namespace Mikkee.Converters
{
    public class BooleanToCommandConverter : IValueConverter
    {
        // Allow RegisterCommand and UnregisterCommand to be nullable
        public ICommand? RegisterCommand { get; set; }
        public ICommand? UnregisterCommand { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Check if value is a boolean and parameter is a Course2 object
            // if (value is bool isRegistered && parameter is Course2 course)
            // {
            //     // Return the appropriate command based on the registration status
            //     return isRegistered ? UnregisterCommand : RegisterCommand;
            // }

            // Return null if conditions are not met
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}