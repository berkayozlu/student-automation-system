using System.ComponentModel.DataAnnotations;

namespace StudentAutomation.Frontend.Models
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? StudentNumber { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public int? Year { get; set; }
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public StudentDto? Student { get; set; }
        public TeacherDto? Teacher { get; set; }
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? Department { get; set; }
        public int? Year { get; set; }
        public bool IsActive { get; set; }
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
        public EnrollmentStatus Status { get; set; }
        public string? Comments { get; set; }
        public CourseDto Course { get; set; } = new();
        public StudentDto Student { get; set; } = new();
    }

    public class TeacherDto
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public DateTime HireDate { get; set; }
        public bool IsActive { get; set; }
    }

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

    public class GradeDto
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
        public string ExamType { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

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
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Notes { get; set; }
    }

    public class BulkAttendanceDto
    {
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public List<StudentAttendanceDto> StudentAttendances { get; set; } = new List<StudentAttendanceDto>();
    }

    public class StudentAttendanceDto
    {
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Notes { get; set; }
    }

    public enum AttendanceStatus
    {
        Present = 0,
        Absent = 1,
        Late = 2,
        Excused = 3
    }

    public enum CourseStatus
    {
        Active,
        Inactive,
        Completed
    }

    public enum EnrollmentStatus
    {
        Active = 0,
        Dropped = 1,
        Completed = 2
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

    public class CreateTeacherDto
    {
        public string EmployeeNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Department { get; set; }
        public string? Title { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class CreateStudentDto
    {
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Department { get; set; }
        public int? Year { get; set; }
    }

    public class CreateCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Credits { get; set; }
        public int TeacherId { get; set; }
    }

    public class CreateGradeDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string ExamType { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string? Comments { get; set; }
    }

}
