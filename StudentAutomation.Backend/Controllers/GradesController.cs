using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Backend.Data;
using StudentAutomation.Backend.DTOs;
using StudentAutomation.Backend.Models;
using System.Security.Claims;

namespace StudentAutomation.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<GradeDto>>> GetGrades()
        {
            var grades = await _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = $"{g.Student.User.FirstName} {g.Student.User.LastName}",
                    StudentNumber = g.Student.StudentNumber,
                    CourseId = g.CourseId,
                    CourseName = g.Course.CourseName,
                    CourseCode = g.Course.CourseCode,
                    TeacherId = g.TeacherId,
                    TeacherName = $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}",
                    ExamType = g.ExamType,
                    Score = g.Score,
                    Comments = g.Comments,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(grades);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GradeDto>> GetGrade(int id)
        {
            var grade = await _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (grade == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Students can only view their own grades, teachers can view grades for their courses
            if (!userRoles.Contains("Admin"))
            {
                if (userRoles.Contains("Student") && grade.Student.UserId != currentUserId)
                {
                    return Forbid();
                }
                if (userRoles.Contains("Teacher") && grade.Teacher.UserId != currentUserId)
                {
                    return Forbid();
                }
            }

            var gradeDto = new GradeDto
            {
                Id = grade.Id,
                StudentId = grade.StudentId,
                StudentName = $"{grade.Student.User.FirstName} {grade.Student.User.LastName}",
                StudentNumber = grade.Student.StudentNumber,
                CourseId = grade.CourseId,
                CourseName = grade.Course.CourseName,
                CourseCode = grade.Course.CourseCode,
                TeacherId = grade.TeacherId,
                TeacherName = $"{grade.Teacher.User.FirstName} {grade.Teacher.User.LastName}",
                ExamType = grade.ExamType,
                Score = grade.Score,
                Comments = grade.Comments,
                CreatedAt = grade.CreatedAt,
                UpdatedAt = grade.UpdatedAt
            };

            return Ok(gradeDto);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<GradeDto>> CreateGrade([FromBody] CreateGradeDto createGradeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.UserId == currentUserId);

            if (teacher == null || teacher.User == null)
            {
                Console.WriteLine($"DEBUG Backend: Teacher or Teacher.User not found for UserId {currentUserId}");
                return BadRequest("Teacher not found.");
            }

            // Check if course exists and belongs to the teacher
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == createGradeDto.CourseId && c.TeacherId == teacher.Id);
            if (course == null)
            {
                return BadRequest("Course not found or you don't have permission to grade this course.");
            }

            // Check if student exists and is enrolled in the course
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.StudentId == createGradeDto.StudentId && ce.CourseId == createGradeDto.CourseId && ce.Status == EnrollmentStatus.Active);

            if (enrollment == null)
            {
                return BadRequest("Student is not enrolled in this course.");
            }

            var grade = new Grade
            {
                StudentId = createGradeDto.StudentId,
                CourseId = createGradeDto.CourseId,
                TeacherId = teacher.Id,
                ExamType = createGradeDto.ExamType,
                Score = createGradeDto.Score,
                Comments = createGradeDto.Comments,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == createGradeDto.StudentId);

            if (student == null || student.User == null)
            {
                Console.WriteLine($"DEBUG Backend: Student or User not found for StudentId {createGradeDto.StudentId}");
                return BadRequest("Student not found.");
            }

            var gradeDto = new GradeDto
            {
                Id = grade.Id,
                StudentId = grade.StudentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                StudentNumber = student.StudentNumber,
                CourseId = grade.CourseId,
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                TeacherId = grade.TeacherId,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                ExamType = grade.ExamType,
                Score = grade.Score,
                Comments = grade.Comments,
                CreatedAt = grade.CreatedAt,
                UpdatedAt = grade.UpdatedAt
            };

            Console.WriteLine($"DEBUG Backend: Grade created successfully with ID {grade.Id}");
            return Ok(gradeDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeDto updateGradeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);

            if (teacher == null)
            {
                return BadRequest("Teacher not found.");
            }

            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == id && g.TeacherId == teacher.Id);
            if (grade == null)
            {
                return NotFound("Grade not found or you don't have permission to update this grade.");
            }

            grade.ExamType = updateGradeDto.ExamType;
            grade.Score = updateGradeDto.Score;
            grade.Comments = updateGradeDto.Comments;
            grade.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);

            if (teacher == null)
            {
                return BadRequest("Teacher not found.");
            }

            var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == id && g.TeacherId == teacher.Id);
            if (grade == null)
            {
                return NotFound("Grade not found or you don't have permission to delete this grade.");
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<List<GradeDto>>> GetGradesByCourse(int courseId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            // Teachers can only view grades for their own courses
            if (userRoles.Contains("Teacher") && !userRoles.Contains("Admin") && course.Teacher.UserId != currentUserId)
            {
                return Forbid();
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
                .Where(g => g.CourseId == courseId)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = $"{g.Student.User.FirstName} {g.Student.User.LastName}",
                    StudentNumber = g.Student.StudentNumber,
                    CourseId = g.CourseId,
                    CourseName = g.Course.CourseName,
                    CourseCode = g.Course.CourseCode,
                    TeacherId = g.TeacherId,
                    TeacherName = $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}",
                    ExamType = g.ExamType,
                    Score = g.Score,
                    Comments = g.Comments,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(grades);
        }

        [HttpGet("my-grades")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<GradeDto>>> GetMyGrades()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == currentUserId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
                .Where(g => g.StudentId == student.Id)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = $"{g.Student.User.FirstName} {g.Student.User.LastName}",
                    StudentNumber = g.Student.StudentNumber,
                    CourseId = g.CourseId,
                    CourseName = g.Course.CourseName,
                    CourseCode = g.Course.CourseCode,
                    TeacherId = g.TeacherId,
                    TeacherName = $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}",
                    ExamType = g.ExamType,
                    Score = g.Score,
                    Comments = g.Comments,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(grades);
        }

        [HttpGet("my-grades/{courseId}")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<GradeDto>>> GetMyGradesByCourse(int courseId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == currentUserId);

            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Check if student is enrolled in the course
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.StudentId == student.Id && ce.CourseId == courseId);

            if (enrollment == null)
            {
                return Forbid("You are not enrolled in this course.");
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
                .Where(g => g.StudentId == student.Id && g.CourseId == courseId)
                .Select(g => new GradeDto
                {
                    Id = g.Id,
                    StudentId = g.StudentId,
                    StudentName = $"{g.Student.User.FirstName} {g.Student.User.LastName}",
                    StudentNumber = g.Student.StudentNumber,
                    CourseId = g.CourseId,
                    CourseName = g.Course.CourseName,
                    CourseCode = g.Course.CourseCode,
                    TeacherId = g.TeacherId,
                    TeacherName = $"{g.Teacher.User.FirstName} {g.Teacher.User.LastName}",
                    ExamType = g.ExamType,
                    Score = g.Score,
                    Comments = g.Comments,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(grades);
        }
    }
}
