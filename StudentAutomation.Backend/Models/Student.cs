using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAutomation.Backend.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? Department { get; set; }

        public int? Year { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
