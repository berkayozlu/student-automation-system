using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAutomation.Backend.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int Credits { get; set; }

        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; } = null!;

        public CourseStatus Status { get; set; } = CourseStatus.NotStarted;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }

    public enum CourseStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}
