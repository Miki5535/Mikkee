using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Mikkee.Models
{
    public class Course
    {
        [JsonPropertyName("sub_id")]
        public int SubId { get; set; }

        [JsonPropertyName("course_code")]
        public string? CourseCode { get; set; }

        [JsonPropertyName("course_name")]
        public string? CourseName { get; set; }

        [JsonPropertyName("study_type")]
        public string? StudyType { get; set; }

        [JsonPropertyName("credit")]
        public int Credit { get; set; }

        [JsonPropertyName("instructor")]
        public string? Instructor { get; set; }

        [JsonPropertyName("schedule")]
        public string? Schedule { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }

        // คุณสมบัติใหม่สำหรับจัดกลุ่ม
    public string DayOfWeek 
    { 
        get 
        {
            if (string.IsNullOrEmpty(Schedule)) return "ไม่ระบุ";
            return Schedule.Split(' ')[0] switch
            {
                "จันทร์" => "จันทร์",
                "อังคาร" => "อังคาร",
                "พุธ" => "พุธ",
                "พฤหัสบดี" => "พฤหัสบดี",
                "ศุกร์" => "ศุกร์",
                "เสาร์" => "เสาร์",
                "อาทิตย์" => "อาทิตย์",
                _ => "ไม่ระบุ"
            };
        }
    }

    public string TimeRange 
    { 
        get 
        {
            if (string.IsNullOrEmpty(Schedule)) return "ไม่ระบุ";
            return Schedule.Contains(' ') ? Schedule.Split(' ')[1] : "ไม่ระบุ";
        }
    }
    }

    public class UserCourse
    {
        [JsonPropertyName("sub_id")]
        public int SubId { get; set; }

        [JsonPropertyName("grade")]
        public string? Grade { get; set; }
    }

    public class PastSemester
    {
        [JsonPropertyName("term")]
        public string? Term { get; set; }

        [JsonPropertyName("courses")]
        public List<Course>? Courses { get; set; }
    }

    public class Registration
    {
        [JsonPropertyName("user_uid")]
        public string? UserUID { get; set; }

        [JsonPropertyName("current_semester")]
        public List<UserCourse>? CurrentSemester { get; set; }

        [JsonPropertyName("past_semesters")]
        public List<PastSemester>? PastSemesters { get; set; }
    }

    
    
}
