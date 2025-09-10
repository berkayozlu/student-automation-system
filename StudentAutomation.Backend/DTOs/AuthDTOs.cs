using System.ComponentModel.DataAnnotations;

namespace StudentAutomation.Backend.DTOs
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
        public string Role { get; set; } = string.Empty; // Admin, Teacher, Student

        // Additional fields for specific roles
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
}
