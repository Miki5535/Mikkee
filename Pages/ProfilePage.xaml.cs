using Mikkee.Models;
using Microsoft.Maui.Controls;


namespace Mikkee.Pages
{
    public partial class ProfilePage : ContentPage
    {

    
private User CurrentUser { get; set; }

        public ProfilePage(User user)
        {
            InitializeComponent();
            CurrentUser = user;

            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            if (CurrentUser != null)
            {
                if (!string.IsNullOrEmpty(CurrentUser.Profile))
                {
                    profileImage.Source = CurrentUser.Profile;
                }
                else
                {
                    profileImage.Source = "logo.png";
                }
                userNameLabel.Text = $"ชื่อ-นามสกุล: {CurrentUser.Name ?? "ไม่มีข้อมูล"}";
                studentIdLabel.Text = $"รหัสนิสิต: {CurrentUser.StudentId ?? "ไม่มีข้อมูล"}";
                facultyDepartmentLabel.Text = $"คณะ/สาขาวิชา: {CurrentUser.FacultyDepartment ?? "ไม่มีข้อมูล"}";
                emailLabel.Text = $"Email: {CurrentUser.Email ?? "ไม่มีข้อมูล"}";
            }
        }

       private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.UID))
        {
            // await Navigation.PushAsync(new RegisterPage(CurrentUser.UID));
            await Navigation.PushAsync(new RegisterSubPage(CurrentUser.UID));
        }
        else
        {
            // Handle the case where CurrentUser or UID is null
            await DisplayAlert("Error", "ข้อมูลผู้ใช้ไม่สมบูรณ์", "OK");
        }
    }

       private async void OnTimeTableClicked(object sender, EventArgs e)
    {
        if (CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.UID))
        {
            // await Navigation.PushAsync(new RegisterPage(CurrentUser.UID));
            await Navigation.PushAsync(new TimetablePage(CurrentUser.UID));
        }
        else
        {
            // Handle the case where CurrentUser or UID is null
            await DisplayAlert("Error", "ข้อมูลผู้ใช้ไม่สมบูรณ์", "OK");
        }
    }

        private async void OnRegistrationInfoClicked(object sender, EventArgs e)
        {
            if (CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.UID))
            {
                // Navigate to Registration Info Page with User UID
                await Navigation.PushAsync(new RegistrationInfoPage(CurrentUser.UID));
            }
            else
            {
                // Handle the case where CurrentUser or UID is null
                await DisplayAlert("Error", "ข้อมูลผู้ใช้ไม่สมบูรณ์", "OK");
            }
        }


private async void OnLogoutClicked(object sender, EventArgs e)
{
    // Pop all pages off the navigation stack and navigate back to the Login Page
    await Navigation.PopToRootAsync();
}

    }
}