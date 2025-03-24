using System.Text.Json.Serialization;

namespace Mikkee.Models
{
    public class User
    {
        [JsonPropertyName("UID")]
        public string? UID { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("student_id")]
        public string? StudentId { get; set; }

        [JsonPropertyName("faculty_department")]
        public string? FacultyDepartment { get; set; }

        [JsonPropertyName("pro")]
        public string? Profile { get; set; }
    }
}