
using System.Collections.ObjectModel;
using System.Text.Json;

using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

using System.Windows.Input;
using Mikkee.Converters;
using Mikkee.Models;
namespace Mikkee.Pages
{
    public partial class RegisterPage : ContentPage
    {
        public ICommand RegisterCommand { get; }
        public ICommand UnregisterCommand { get; }

        private ObservableCollection<Course2> AllCourses { get; set; } = new();
        private ObservableCollection<Course2> FilteredCourses { get; set; } = new();
        private ObservableCollection<Course> RegistrationsInUser { get; set; } = new();

       public RegisterPage(string uid)
{
    InitializeComponent();
    BindingContext = this;
    

    RegisterCommand = new AsyncRelayCommand<Course2>(async (course) =>
    {
        await UpdateJsonFile(course, true);
        await LoadRegistrationsAsync(uid); // Reload registrations after update
    });

    UnregisterCommand = new AsyncRelayCommand<Course2>(async (course) =>
    {
        await UpdateJsonFile(course, false);
        await LoadRegistrationsAsync(uid); // Reload registrations after update
    });

    _ = LoadCoursesAsync();
    _ = LoadRegistrationsAsync(uid);
    CoursesListView.ItemsSource = FilteredCourses;
}

        private async Task LoadCoursesAsync()
        {
            try
            {
                string jsonPath = Path.Combine(Environment.CurrentDirectory,  "Data", "courses.json");
                if (File.Exists(jsonPath))
                {
                    string json = await File.ReadAllTextAsync(jsonPath);
                    // Debug.WriteLine($"File read successfully. Length of data: {json.Length}");

                    var courseList = JsonSerializer.Deserialize<List<Course2>>(json);

                    if (courseList != null)
                    {
                        AllCourses.Clear();
                        FilteredCourses.Clear();

                        foreach (var course in courseList)
                        {
                            if (string.IsNullOrEmpty(course.CourseCode) || string.IsNullOrEmpty(course.CourseName))
                            {
                                Debug.WriteLine("Course has missing data:");
                            }
                            else
                            {
                                AllCourses.Add(course);
                                FilteredCourses.Add(course);
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Failed to deserialize JSON into course list.");
                    }
                }
                else
                {
                    Debug.WriteLine("courses.json does not exist.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading courses: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load courses: {ex.Message}", "OK");
            }
        }

private async Task LoadRegistrationsAsync(string UserUID)
{
    try
    {
        // Load user registration data
        string registrationJson;
#if ANDROID || IOS
        using var regStream = await FileSystem.OpenAppPackageFileAsync("registrations.json");
        using var regReader = new StreamReader(regStream);
        registrationJson = await regReader.ReadToEndAsync();
#else
        var regFilePath = Path.Combine(Environment.CurrentDirectory,  "Data", "registrations.json");
        registrationJson = await File.ReadAllTextAsync(regFilePath);
#endif

        var registrations = JsonSerializer.Deserialize<List<Registration>>(registrationJson);

        if (registrations == null)
        {
            Debug.WriteLine("Failed to deserialize registration data.");
            await DisplayAlert("ข้อผิดพลาด", "ไม่สามารถโหลดข้อมูลได้", "ตกลง");
            return;
        }

        // Find the user's registration data
        var userRegistration = registrations.FirstOrDefault(r => r.UserUID == UserUID);
        if (userRegistration == null)
        {
            Debug.WriteLine($"No registration data found for UID: {UserUID}");

            // Create empty registration data for the user
            userRegistration = new Registration
            {
                UserUID = UserUID,
                CurrentSemester = new List<UserCourse>(),
                PastSemesters = new List<PastSemester>()
            };

            // Inform the user to register
            await DisplayAlert("แจ้งเตือน", "ไม่พบข้อมูลการลงทะเบียน กรุณาลงทะเบียนรายวิชา", "ตกลง");
            return;
        }
        else
        {
            // Populate RegistrationsInUser with the current semester courses
            RegistrationsInUser.Clear(); // Clear previous data

            // Merge current semester courses into RegistrationsInUser
            var currentCourses = (userRegistration.CurrentSemester ?? Enumerable.Empty<UserCourse>())
                .Select(uc => new Course
                {
                    SubId = uc.SubId,
                    Grade = uc.Grade,
                }).ToList();

            foreach (var course in currentCourses)
            {
                RegistrationsInUser.Add(course); // Add to the ObservableCollection
            }

            // Assign RegistrationsInUser to the converter
            var converter = (SubIdExistsConverter)Resources["SubIdExistsConverter"];
            converter.RegistrationsInUser = RegistrationsInUser;

            Debug.WriteLine($"RegistrationsInUser: {RegistrationsInUser.Count} courses loaded.");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error: {ex.Message}");
        await DisplayAlert("ข้อผิดพลาด", "ไม่สามารถโหลดข้อมูลได้", "ตกลง");
    }
}



private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
{
    // รับค่าคำค้นหาจาก SearchBar
    string searchText = e.NewTextValue?.ToLower() ?? "";

    // กรองข้อมูลจาก AllCourses ตามคำค้นหา
    var filtered = AllCourses
        .Where(c => (c.CourseName?.ToLower().Contains(searchText) ?? false) ||
                    (c.CourseCode?.ToLower().Contains(searchText) ?? false) ||
                    (c.Instructor?.ToLower().Contains(searchText) ?? false))
        .ToList();

    // อัปเดต FilteredCourses
    FilteredCourses.Clear();
    foreach (var course in filtered)
    {
        FilteredCourses.Add(course);
    }
}

        

public async Task UpdateJsonFile(Course2? course, bool isRegistering)
{
    if (course == null)
    {
        Debug.WriteLine("Course is null. Cannot update JSON file.");
        return;
    }

    try
    {
        string jsonPath = Path.Combine(Environment.CurrentDirectory,  "Data", "registered_courses.json");

        List<Course2> registeredCourses = new List<Course2>();

        if (File.Exists(jsonPath))
        {
            string json = await File.ReadAllTextAsync(jsonPath);
            registeredCourses = JsonSerializer.Deserialize<List<Course2>>(json) ?? new List<Course2>();
        }

        if (isRegistering)
        {
            if (!registeredCourses.Any(c => c.SubId == course.SubId))
            {
                registeredCourses.Add(course); // Add course if not already registered
            }
        }
        else
        {
            registeredCourses.RemoveAll(c => c.SubId == course.SubId); // Remove course
        }

        string updatedJson = JsonSerializer.Serialize(registeredCourses, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, updatedJson);

        Debug.WriteLine($"Updated registered_courses.json successfully.");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error updating registered_courses.json: {ex.Message}");
        await DisplayAlert("Error", $"Failed to update registration: {ex.Message}", "OK");
    }
}


private void Addsub(object sender, EventArgs e){
Debug.WriteLine($"deletesub");
}


   private void Deletesub(object sender, EventArgs e){
    Debug.WriteLine($"addesub");
}














    }
}




