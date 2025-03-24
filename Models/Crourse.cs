using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;



namespace Mikkee.Models
{
    public class Course2 :INotifyPropertyChanged
    {
        public bool IsRegistered { get; set; }

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

        // Add Commands to the model
        private ICommand? _registerCommand;
        private ICommand? _unregisterCommand;

        public ICommand RegisterCommand
        {
            get => _registerCommand ??= new RelayCommand(() =>
            {
                // Logic for registering a course
                Debug.WriteLine($"Registering course: {CourseName}");
                IsRegistered = true;
                OnPropertyChanged(nameof(IsRegistered));
            });
        }

        public ICommand UnregisterCommand
        {
            get => _unregisterCommand ??= new RelayCommand(() =>
            {
                // Logic for unregistering a course
                Debug.WriteLine($"Unregistering course: {CourseName}");
                IsRegistered = false;
                OnPropertyChanged(nameof(IsRegistered));
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}