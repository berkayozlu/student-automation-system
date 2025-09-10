using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentAutomation.Backend.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual Teacher Teacher { get; set; } = null!;

        public DateTime Date { get; set; }

        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AttendanceStatus
    {
        Present = 0,
        Absent = 1,
        Late = 2,
        Excused = 3
    }
}
