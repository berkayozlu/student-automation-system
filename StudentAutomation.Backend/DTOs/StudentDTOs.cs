using System.ComponentModel.DataAnnotations;

namespace StudentAutomation.Backend.DTOs
{
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

    public class CreateStudentDto
    {
        [Required]
        [MaxLength(20)]
        public string StudentNumber { get; set; } = string.Empty;

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

        public int? Year { get; set; }
    }

    public class UpdateStudentDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Department { get; set; }

        public int? Year { get; set; }

        public bool IsActive { get; set; }
    }
}
