using System.Text.Json;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Mikkee.Models;
using System.Collections.ObjectModel;

namespace Mikkee.Pages;

public partial class TimetablePage : ContentPage
{
    public ObservableCollection<Course> CurrentCourses { get; } = new();
	public ObservableCollection<DayGroup> GroupedCourses { get; } = new();
    private string UserUID { get; set; }

    public TimetablePage(string userUID)
    {
        InitializeComponent();
        BindingContext = this;
        UserUID = userUID;
        LoadRegistrationData(); // เรียกโหลดข้อมูลทันทีที่สร้างหน้า
    }

 private async void LoadRegistrationData()
        {
            try
            {
                // Load course details
                string courseDetailsJson;
#if ANDROID || IOS
        using var courseStream = await FileSystem.OpenAppPackageFileAsync("courses.json");
        using var courseReader = new StreamReader(courseStream);
        courseDetailsJson = await courseReader.ReadToEndAsync();
#else
                var courseFilePath = Path.Combine(Environment.CurrentDirectory, "Data", "courses.json");
                courseDetailsJson = await File.ReadAllTextAsync(courseFilePath);
#endif

                var courses = JsonSerializer.Deserialize<List<Course>>(courseDetailsJson);

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

                Debug.WriteLine($"UserUID being searched: {UserUID}");
                Debug.WriteLine($"All UserUIDs in registration data: {string.Join(", ", registrations?.Select(r => r.UserUID) ?? new string[0])}");

                if (courses == null || registrations == null)
                {
                    Debug.WriteLine("Failed to deserialize data.");
                    await DisplayAlert("ข้อผิดพลาด", "ไม่สามารถโหลดข้อมูลได้", "ตกลง");
                    return;
                }

                // Find the user's registration data
                var userRegistration = registrations.FirstOrDefault(r => r.UserUID == UserUID);
                if (userRegistration == null)
                {
                    Debug.WriteLine($"No registration data found for UID: {UserUID}");

                    // สร้างข้อมูลการลงทะเบียนเปล่าสำหรับผู้ใช้นี้
                    userRegistration = new Registration
                    {
                        UserUID = UserUID,
                        CurrentSemester = new List<UserCourse>(),
                        PastSemesters = new List<PastSemester>()
                    };

                    // แสดงข้อความให้ผู้ใช้ทราบ
                    await DisplayAlert("แจ้งเตือน", "ไม่พบข้อมูลการลงทะเบียน กรุณาลงทะเบียนรายวิชา", "ตกลง");

                    
                    return;
                }
                else
                {
                   // ในส่วน else ของ LoadRegistrationData()
var currentCourses = (userRegistration.CurrentSemester ?? Enumerable.Empty<UserCourse>())
    .Join(courses,
        uc => uc.SubId,
        c => c.SubId,
        (uc, c) => new Course
        {
            SubId = c.SubId,
            CourseCode = c.CourseCode,
            CourseName = c.CourseName,
            Schedule = c.Schedule, // ตรวจสอบข้อมูลนี้ในไฟล์ JSON
            Instructor = c.Instructor,
            Grade = uc.Grade
        })
    .Where(c => !string.IsNullOrEmpty(c.Schedule)) // กรองข้อมูลที่ไม่มี Schedule
    .ToList();

    // จัดกลุ่มตามวัน
    var grouped = currentCourses
        .GroupBy(c => c.DayOfWeek)
        .Select(g => new DayGroup(g.Key, g.OrderBy(c => c.TimeRange).ToList()))
        .OrderBy(g => 
        {
            var days = new[] { "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์", "อาทิตย์" };
            return Array.IndexOf(days, g.Day);
        });

    // อัปเดต UI
    GroupedCourses.Clear();
    foreach (var group in grouped)
    {
        GroupedCourses.Add(group);
    }

                    // Debug.WriteLine($"Current Semester Courses (After Join): {JsonSerializer.Serialize(currentCourses)}");

                   



                    var pastSemesters = (userRegistration.PastSemesters ?? Enumerable.Empty<PastSemester>())
    .Select(ps => new PastSemester
    {
        Term = ps.Term,
        Courses = (ps.Courses ?? Enumerable.Empty<Course>())
            .Join(courses,
                  uc => uc.SubId,
                  c => c.SubId,
                  (uc, c) => new Course
                  {
                      CourseCode = c.CourseCode,
                      CourseName = c.CourseName,
                      StudyType = c.StudyType,
                      Credit = c.Credit,
                      Grade = uc.Grade
                  }).ToList()
    }).ToList();

                    // Debug.WriteLine($"Past Semesters (After Join): {JsonSerializer.Serialize(pastSemesters)}");

                   


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("ข้อผิดพลาด", "ไม่สามารถโหลดข้อมูลได้", "ตกลง");
            }

        }
	
private async void OnฺBack(object sender, EventArgs e)
{
    // Pop all pages off the navigation stack and navigate back to the Login Page
    await Navigation.PopAsync();
}
	

public class DayGroup : ObservableCollection<Course>
{
    public string Day { get; }

    public DayGroup(string day, List<Course> courses) : base(courses)
    {
        Day = day;
    }
}







}