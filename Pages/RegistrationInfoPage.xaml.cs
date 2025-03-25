using System.Text.Json;
using System.Diagnostics;
using Microsoft.Maui.Graphics;
using Mikkee.Models;


namespace Mikkee.Pages
{
    public partial class RegistrationInfoPage : ContentPage
    {

        private string UserUID { get; set; }

        public RegistrationInfoPage(string userUID)
        {
            InitializeComponent();
            UserUID = userUID;
            LoadRegistrationData();

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

                    // อัพเดตหน้าจอด้วยข้อมูลว่างเปล่า
                    DisplayCurrentSemester(new List<Course>());
                    DisplayPastSemesters(new List<PastSemester>());
                    return;
                }
                else
                {
                    var currentCourses = (userRegistration.CurrentSemester ?? Enumerable.Empty<UserCourse>())
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
                  }).ToList();

                    // Debug.WriteLine($"Current Semester Courses (After Join): {JsonSerializer.Serialize(currentCourses)}");

                    DisplayCurrentSemester(currentCourses);



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

                    DisplayPastSemesters(pastSemesters);


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("ข้อผิดพลาด", "ไม่สามารถโหลดข้อมูลได้", "ตกลง");
            }

        }



        private void DisplayCurrentSemester(List<Course> courses)
        {
            // Debug.WriteLine("Displaying Current Semester Courses:");
            // foreach (var course in courses)
            // {
            //     Debug.WriteLine($"CourseCode: {course.CourseCode}, CourseName: {course.CourseName}, Grade: {course.Grade}");
            // }

            currentSemesterGrid.Children.Clear();
            currentSemesterGrid.RowDefinitions.Clear();

            // Add Header Row
            currentSemesterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var headers = new[] { "รหัสวิชา", "ชื่อวิชา", "แบบการศึกษา", "หน่วยกิต", "เกรด" };
            for (int i = 0; i < headers.Length; i++)
            {
                var headerLabel = CreateLabel(headers[i], TextAlignment.Center);
                headerLabel.BackgroundColor = Colors.LightBlue;
                headerLabel.TextColor = Colors.White;
                Grid.SetRow(headerLabel, 0);
                Grid.SetColumn(headerLabel, i);
                currentSemesterGrid.Children.Add(headerLabel);
            }

            // Add Course Rows
            int rowIndex = 1;
            foreach (var course in courses)
            {
                currentSemesterGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var codeLabel = CreateLabel(course.CourseCode ?? "N/A", TextAlignment.Center);
                var nameLabel = CreateLabel(course.CourseName ?? "N/A", TextAlignment.Start);
                var studyTypeLabel = CreateLabel(course.StudyType ?? "N/A", TextAlignment.Center);
                var creditLabel = CreateLabel(course.Credit.ToString(), TextAlignment.Center);
                var gradeLabel = CreateLabel(course.Grade ?? "N/A", TextAlignment.Center);

                Grid.SetRow(codeLabel, rowIndex);
                Grid.SetColumn(codeLabel, 0);

                Grid.SetRow(nameLabel, rowIndex);
                Grid.SetColumn(nameLabel, 1);

                Grid.SetRow(studyTypeLabel, rowIndex);
                Grid.SetColumn(studyTypeLabel, 2);

                Grid.SetRow(creditLabel, rowIndex);
                Grid.SetColumn(creditLabel, 3);

                Grid.SetRow(gradeLabel, rowIndex);
                Grid.SetColumn(gradeLabel, 4);

                currentSemesterGrid.Children.Add(codeLabel);
                currentSemesterGrid.Children.Add(nameLabel);
                currentSemesterGrid.Children.Add(studyTypeLabel);
                currentSemesterGrid.Children.Add(creditLabel);
                currentSemesterGrid.Children.Add(gradeLabel);

                rowIndex++;
            }
        }

        private void DisplayPastSemesters(List<PastSemester> pastSemesters)
        {
            pastSemestersGrid.Children.Clear();
            pastSemestersGrid.RowDefinitions.Clear();

            int rowIndex = 0;

            foreach (var semester in pastSemesters)
            {
                // Add Term Label
                pastSemestersGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                var termLabel = new Label
                {
                    Text = semester.Term,
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Gray,
                    Margin = new Thickness(0, 10, 0, 5)
                };
                Grid.SetRow(termLabel, rowIndex);
                Grid.SetColumnSpan(termLabel, 5);
                pastSemestersGrid.Children.Add(termLabel);
                rowIndex++;

                // Add Header Row
                pastSemestersGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var headers = new[] { "รหัสวิชา", "ชื่อวิชา", "แบบการศึกษา", "หน่วยกิต", "เกรด" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var headerLabel = CreateLabel(headers[i], TextAlignment.Center);
                    headerLabel.BackgroundColor = Colors.LightBlue;
                    headerLabel.TextColor = Colors.White;
                    Grid.SetRow(headerLabel, rowIndex);
                    Grid.SetColumn(headerLabel, i);
                    pastSemestersGrid.Children.Add(headerLabel);
                }
                rowIndex++;

                // Add Course Rows
                if (semester.Courses == null) continue;

                foreach (var course in semester.Courses)
                {

                    pastSemestersGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    var codeLabel = CreateLabel(course.CourseCode ?? "N/A", TextAlignment.Center);
                    var nameLabel = CreateLabel(course.CourseName ?? "N/A", TextAlignment.Start);
                    var studyTypeLabel = CreateLabel(course.StudyType ?? "N/A", TextAlignment.Center);
                    var creditLabel = CreateLabel(course.Credit.ToString(), TextAlignment.Center);
                    var gradeLabel = CreateLabel(course.Grade ?? "N/A", TextAlignment.Center);

                    Grid.SetRow(codeLabel, rowIndex);
                    Grid.SetColumn(codeLabel, 0);

                    Grid.SetRow(nameLabel, rowIndex);
                    Grid.SetColumn(nameLabel, 1);

                    Grid.SetRow(studyTypeLabel, rowIndex);
                    Grid.SetColumn(studyTypeLabel, 2);

                    Grid.SetRow(creditLabel, rowIndex);
                    Grid.SetColumn(creditLabel, 3);

                    Grid.SetRow(gradeLabel, rowIndex);
                    Grid.SetColumn(gradeLabel, 4);

                    pastSemestersGrid.Children.Add(codeLabel);
                    pastSemestersGrid.Children.Add(nameLabel);
                    pastSemestersGrid.Children.Add(studyTypeLabel);
                    pastSemestersGrid.Children.Add(creditLabel);
                    pastSemestersGrid.Children.Add(gradeLabel);

                    rowIndex++;
                }
            }
        }

        private Label CreateLabel(string? text, TextAlignment alignment)
        {
            return new Label
            {
                Text = text ?? "N/A", // ใช้ "N/A" หากข้อความเป็น null
                HorizontalTextAlignment = alignment,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                Padding = 8
            };
        }



        private async void OnBackToProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterSubPage(UserUID));
        }
    }
}
