using System.ComponentModel.DataAnnotations;
using StudentAutomation.Backend.Models;

namespace StudentAutomation.Backend.DTOs
{
    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateAttendanceDto
    {
        [Required]
        public DateTime Date { get; set; }

        public AttendanceStatus Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class BulkAttendanceDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public List<StudentAttendanceDto> StudentAttendances { get; set; } = new List<StudentAttendanceDto>();
    }

    public class StudentAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        public string? Notes { get; set; }
    }
}
