using System.ComponentModel.DataAnnotations;
using StudentAutomation.Backend.Models;

namespace StudentAutomation.Backend.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public CourseStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int EnrolledStudentsCount { get; set; }
    }

    public class CreateCourseDto
    {
        [Required]
        [MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, 10)]
        public int Credits { get; set; }

        [Required]
        public int TeacherId { get; set; }
    }

    public class UpdateCourseDto
    {
        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, 10)]
        public int Credits { get; set; }

        public CourseStatus Status { get; set; }
    }

    public class CourseStudentDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public EnrollmentStatus EnrollmentStatus { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
    }

    public class AddStudentsToCourseDto
    {
        public List<int> StudentIds { get; set; } = new();
    }

    public class CourseEnrollmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public CourseDto Course { get; set; } = new();
        public StudentDto Student { get; set; } = new();
        public EnrollmentStatus Status { get; set; }
        public string? Comments { get; set; }
    }

    public class CreateCourseEnrollmentDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public string? Comments { get; set; }
    }
}
