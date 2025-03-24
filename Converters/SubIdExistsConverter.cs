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
            // เช็คว่ามี SubId นี้ใน RegistrationsInUser หรือไม่
            var isRegistered = RegistrationsInUser.Any(registration => registration.SubId == subId);
            
            // ถ้าต้องการให้ปุ่ม "ลงทะเบียน" แสดงเมื่อไม่ได้ลงทะเบียน
            // และ "ถอนวิชา" แสดงเมื่อมีการลงทะเบียน
            if (parameter != null && bool.TryParse(parameter.ToString(), out bool invert))
            {
                // หาก parameter = true (ปุ่มถอนวิชา) ให้กลับค่าการแสดงผล
                return invert ? isRegistered : !isRegistered;
            }

            return !isRegistered; // ปุ่มลงทะเบียนแสดงเมื่อไม่ลงทะเบียน
        }
        return false;
    }

    public object? ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

}