using System.Collections.ObjectModel;
using System.Text.Json;

using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

using System.Windows.Input;
using Mikkee.Converters;
using Mikkee.Models;
namespace Mikkee.Pages
{
    public partial class RegisterSubPage : ContentPage
    {
        public ICommand RegisterCommand { get; }
        public ICommand UnregisterCommand { get; }

        private ObservableCollection<Course2> AllCourses { get; set; } = new();
        private ObservableCollection<Course2> FilteredCourses { get; set; } = new();
        private ObservableCollection<Course> RegistrationsInUser { get; set; } = new();
        private string Uid_from;

        public RegisterSubPage(string uid)
        {
            InitializeComponent();
            BindingContext = this;
            Uid_from = uid;
            FilterPicker.SelectedIndex = 0;

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
                string jsonPath = Path.Combine(Environment.CurrentDirectory, "Data", "courses.json");
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
                var regFilePath = Path.Combine(Environment.CurrentDirectory, "Data", "registrations.json");
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

					    // หลังโหลดข้อมูลเสร็จ
    foreach (var course in AllCourses)
    {
        course.IsSelected = false; // Reset selection
    }

                    Debug.WriteLine($"RegistrationsInUser: {RegistrationsInUser.Count} courses loaded.");


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("ข้อผิดพลาด", "ไม่สามารถโหลดข้อมูลได้", "ตกลง");
            }
        }



private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }


private void ApplyFilters()
{
    var searchText = SearchBar.Text?.ToLower() ?? "";
    var selectedFilter = FilterPicker.SelectedItem?.ToString() ?? "ทั้งหมด";

    var filtered = AllCourses.Where(c =>
        (c.CourseName?.ToLower().Contains(searchText) ?? false) ||
        (c.CourseCode?.ToLower().Contains(searchText) ?? false) ||
        (c.Instructor?.ToLower().Contains(searchText) ?? false)).ToList();

    if (selectedFilter == "ลงทะเบียนแล้ว")
    {
        filtered = filtered.Where(c => RegistrationsInUser.Any(r => r.SubId == c.SubId)).ToList();
    }
    else if (selectedFilter == "ยังไม่ลงทะเบียน")
    {
        filtered = filtered.Where(c => !RegistrationsInUser.Any(r => r.SubId == c.SubId)).ToList();
    }

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
                string jsonPath = Path.Combine(Environment.CurrentDirectory, "Data", "registered_courses.json");

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

private async void Addsub(object sender, EventArgs e)
{
    var button = sender as Button;
    if (button?.CommandParameter is Course2 course)
    {
        try
        {
            // อัปเดตข้อมูลการลงทะเบียน
            string jsonPath = Path.Combine(Environment.CurrentDirectory, "Data", "registrations.json");
            var registrations = JsonSerializer.Deserialize<List<Registration>>(await File.ReadAllTextAsync(jsonPath)) ?? new List<Registration>();

            var userRegistration = registrations.FirstOrDefault(r => r.UserUID == Uid_from) ?? new Registration { UserUID = Uid_from };
            userRegistration.CurrentSemester ??= new List<UserCourse>();

            if (!userRegistration.CurrentSemester.Any(c => c.SubId == course.SubId))
            {
                userRegistration.CurrentSemester.Add(new UserCourse { SubId = course.SubId, Grade = "ยังเรียนอยู่" });
                registrations.RemoveAll(r => r.UserUID == Uid_from);
                registrations.Add(userRegistration);
                
                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(registrations, new JsonSerializerOptions { WriteIndented = true }));
                
                // โหลดข้อมูลใหม่และอัปเดต UI
                await LoadRegistrationsAsync(Uid_from);
                ApplyFilters(); // ใช้ FilteredCourses ใหม่ทันที
                await DisplayAlert("สำเร็จ", $"ลงทะเบียน {course.CourseName} เรียบร้อย", "ตกลง");
            }
            else
            {
                await DisplayAlert("แจ้งเตือน", "วิชานี้ลงทะเบียนแล้ว", "ตกลง");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("ข้อผิดพลาด", ex.Message, "ตกลง");
        }
    }
}

private async void Deletesub(object sender, EventArgs e)
{
    var button = sender as Button;
    if (button?.CommandParameter is Course2 course)
    {
        try
        {
            string jsonPath = Path.Combine(Environment.CurrentDirectory, "Data", "registrations.json");
            var registrations = JsonSerializer.Deserialize<List<Registration>>(await File.ReadAllTextAsync(jsonPath)) ?? new List<Registration>();

            var userRegistration = registrations.FirstOrDefault(r => r.UserUID == Uid_from);
            if (userRegistration?.CurrentSemester?.RemoveAll(c => c.SubId == course.SubId) > 0)
            {
                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(registrations, new JsonSerializerOptions { WriteIndented = true }));
                
                // โหลดข้อมูลใหม่และอัปเดต UI
                await LoadRegistrationsAsync(Uid_from);
                ApplyFilters(); // ใช้ FilteredCourses ใหม่ทันที
                await DisplayAlert("สำเร็จ", $"ถอนวิชา {course.CourseName} เรียบร้อย", "ตกลง");
            }
            else
            {
                await DisplayAlert("แจ้งเตือน", "ไม่พบวิชานี้ในระบบ", "ตกลง");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("ข้อผิดพลาด", ex.Message, "ตกลง");
        }
    }
}


private async void OnฺBack(object sender, EventArgs e)
{
    // Pop all pages off the navigation stack and navigate back to the Login Page
    await Navigation.PopAsync();
}



// ในส่วนของ Bulk Register
private async void BulkRegister_Clicked(object sender, EventArgs e)
{
    var selectedCourses = FilteredCourses.Where(c => c.IsSelected).ToList();
    foreach (var course in selectedCourses)
    {
        await Addsub2(course); // เรียกเมธอดลงทะเบียนเดี่ยว
    }
    await LoadRegistrationsAsync(Uid_from);
    ApplyFilters();
}

// ในส่วนของ Bulk Unregister
private async void BulkUnregister_Clicked(object sender, EventArgs e)
{
    var selectedCourses = FilteredCourses.Where(c => c.IsSelected).ToList();
    foreach (var course in selectedCourses)
    {
        await Deletesub2(course); // เรียกเมธอดถอนวิชาเดี่ยว
    }
    await LoadRegistrationsAsync(Uid_from);
    ApplyFilters();
}

// ปรับปรุงเมธอด Addsub และ Deletesub ให้รับ Course2 โดยตรง
private async Task Addsub2(Course2 course)
{
    try
    {
        string jsonPath = Path.Combine(Environment.CurrentDirectory, "Data", "registrations.json");
        var registrations = JsonSerializer.Deserialize<List<Registration>>(await File.ReadAllTextAsync(jsonPath)) ?? new List<Registration>();
        var userRegistration = registrations.FirstOrDefault(r => r.UserUID == Uid_from) ?? new Registration { UserUID = Uid_from };
        userRegistration.CurrentSemester ??= new List<UserCourse>();

        if (!userRegistration.CurrentSemester.Any(c => c.SubId == course.SubId))
        {
            userRegistration.CurrentSemester.Add(new UserCourse { SubId = course.SubId, Grade = "ยังเรียนอยู่" });
            registrations.RemoveAll(r => r.UserUID == Uid_from);
            registrations.Add(userRegistration);
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(registrations, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("ข้อผิดพลาด", ex.Message, "ตกลง");
    }
}

private async Task Deletesub2(Course2 course)
{
    try
    {
        string jsonPath = Path.Combine(Environment.CurrentDirectory, "Data", "registrations.json");
        var registrations = JsonSerializer.Deserialize<List<Registration>>(await File.ReadAllTextAsync(jsonPath)) ?? new List<Registration>();
        var userRegistration = registrations.FirstOrDefault(r => r.UserUID == Uid_from);

        if (userRegistration?.CurrentSemester?.RemoveAll(c => c.SubId == course.SubId) > 0)
        {
            await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(registrations, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
    catch (Exception ex)
    {
        await DisplayAlert("ข้อผิดพลาด", ex.Message, "ตกลง");
    }
}




    }
}




