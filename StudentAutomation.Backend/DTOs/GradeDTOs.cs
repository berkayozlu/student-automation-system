using System.ComponentModel.DataAnnotations;

namespace StudentAutomation.Backend.DTOs
{
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

    public class CreateGradeDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ExamType { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        public decimal Score { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }
    }

    public class UpdateGradeDto
    {
        [Required]
        [MaxLength(100)]
        public string ExamType { get; set; } = string.Empty;

        [Required]
        [Range(0, 100)]
        public decimal Score { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }
    }
}
