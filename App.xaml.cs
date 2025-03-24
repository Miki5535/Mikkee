using Mikkee.Converters;
using Mikkee.Pages;
using Microsoft.Maui.Controls;
using Mikkee.Models;

namespace Mikkee;

public partial class App : Application
{
   public App()
{
    try
    {
        InitializeComponent();
        RegisterResources();
    }
    catch (Exception ex)
    {
        // Handle or log the exception for debugging purposes
        Console.WriteLine($"App initialization failed: {ex.Message}");
    }
}


    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Initialize the application with AppShell as the root page
        return new Window(new AppShell());
    }

    private void RegisterResources()
    {
        // Add converters to the global resource dictionary
        Resources.Add("BooleanToTextConverter", new BooleanToTextConverter());
        Resources.Add("BooleanToCommandConverter", new BooleanToCommandConverter
        {
            RegisterCommand = new Command(async (course) =>
            {
                // Use the current window's page to display the alert
                if (Application.Current?.Windows.FirstOrDefault() is { } window && window.Page is not null)
                {
                    await window.Page.DisplayAlert("Register", $"Registered: {((Course2)course).CourseName}", "OK");
                }
            }),
            UnregisterCommand = new Command(async (course) =>
            {
                // Use the current window's page to display the alert
                if (Application.Current?.Windows.FirstOrDefault() is { } window && window.Page is not null)
                {
                    await window.Page.DisplayAlert("Unregister", $"Unregistered: {((Course2)course).CourseName}", "OK");
                }
            })
        });
    }
}