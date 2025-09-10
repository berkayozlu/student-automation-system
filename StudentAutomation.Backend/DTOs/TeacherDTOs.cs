using System.ComponentModel.DataAnnotations;

namespace StudentAutomation.Backend.DTOs
{
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

    public class CreateTeacherDto
    {
        [Required]
        [MaxLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Department { get; set; }

        public string? Title { get; set; }

        public DateTime HireDate { get; set; } = DateTime.UtcNow;
    }

    public class UpdateTeacherDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Department { get; set; }

        public string? Title { get; set; }

        public bool IsActive { get; set; }
    }
}
