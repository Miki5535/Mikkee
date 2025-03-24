using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Mikkee.Models;
using Microsoft.Maui.Controls;

namespace Mikkee.Converters
{
    public class SubIdExistsConverter : IValueConverter
{
    public ObservableCollection<Course> RegistrationsInUser { get; set; } = new();

    public object? Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        if (value is int subId && RegistrationsInUser != null)
        {
            return RegistrationsInUser.Any(registration => registration.SubId == subId);
        }
        return false;
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}
}