
using System.Text.Json;
using System.Diagnostics;
using Mikkee.Models;


namespace Mikkee.Pages
{
    
    public partial class LoginPage : ContentPage
    {

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string email = emailEntry.Text;
            string password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("ข้อผิดพลาด", "กรุณากรอก Email และ Password", "ตกลง");
                return;
            }

            var users = await LoadUsersAsync();

            if (users == null || users.Count == 0)
            {
                await DisplayAlert("ข้อผิดพลาด", "ไม่พบข้อมูลผู้ใช้ในระบบ", "ตกลง");
                return;
            }

            var user = users.Find(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                Debug.WriteLine($"Login SSSS");
                Debug.WriteLine($"Email: {user.Email}, Password: {user.Password}");
                await Navigation.PushAsync(new ProfilePage(user));
            }
            else
            {
                await DisplayAlert("ล้มเหลว", "อีเมลหรือรหัสผ่านไม่ถูกต้อง", "ตกลง");
            }

            // foreach (var u in users)
            // {
            //     Debug.WriteLine($"Email: {u.Email}, Password: {u.Password}");
            // }
        }

        private async Task<List<User>> LoadUsersAsync()
        {
            try
            {
                string jsonString;

#if ANDROID || IOS
        // อ่านไฟล์จาก Embedded Resource (Android/iOS)
        using var stream = await FileSystem.OpenAppPackageFileAsync("users.json");
        using var reader = new StreamReader(stream);
        jsonString = await reader.ReadToEndAsync();
#else
                // อ่านไฟล์จาก Path (Windows/Desktop)
                var filePath = Path.Combine(Environment.CurrentDirectory,  "Data", "users.json");
                if (!File.Exists(filePath))
                {
                    Debug.WriteLine("File not found: " + filePath);
                    return new List<User>();
                }
                jsonString = await File.ReadAllTextAsync(filePath);
#endif

                // Debug.WriteLine($"Loaded JSON: {jsonString}");

                // Deserialize JSON to List<User>
                var users = JsonSerializer.Deserialize<List<User>>(jsonString);

                if (users == null)
                {
                    Debug.WriteLine("Deserialization returned null.");
                    return new List<User>();
                }

                // Log all loaded users
                Debug.WriteLine($"Loaded {users.Count} users  from login pages");
                // foreach (var user in users)
                // {
                //     Debug.WriteLine($"- Email: {user.Email}, Name: {user.Name}, Student ID: {user.StudentId}, Faculty: {user.FacultyDepartment}");
                // }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading users: {ex.Message}");
                return new List<User>();
            }
        }
    }
}