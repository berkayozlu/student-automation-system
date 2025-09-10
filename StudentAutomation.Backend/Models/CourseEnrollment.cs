using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAutomation.Backend.Models
{
    public class CourseEnrollment
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

        [MaxLength(500)]
        public string? Comments { get; set; }
    }

    public enum EnrollmentStatus
    {
        Active = 0,
        Dropped = 1,
        Completed = 2
    }
}
