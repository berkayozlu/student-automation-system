using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<List<StudentDto>>> GetStudents()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Where(s => s.IsActive)
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    StudentNumber = s.StudentNumber,
                    UserId = s.UserId,
                    FirstName = s.User.FirstName,
                    LastName = s.User.LastName,
                    Email = s.User.Email!,
                    PhoneNumber = s.User.PhoneNumber,
                    Address = s.User.Address,
                    DateOfBirth = s.User.DateOfBirth,
                    EnrollmentDate = s.EnrollmentDate,
                    Department = s.Department,
                    Year = s.Year,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return Ok(students);
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentDto>> GetMyProfile()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"DEBUG Backend: Getting profile for user ID: {currentUserId}");

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.UserId == currentUserId);

                if (student == null)
                {
                    Console.WriteLine($"DEBUG Backend: Student not found for user ID: {currentUserId}");
                    return NotFound("Student profile not found.");
                }

                var studentDto = new StudentDto
                {
                    Id = student.Id,
                    StudentNumber = student.StudentNumber,
                    UserId = student.UserId,
                    FirstName = student.User.FirstName,
                    LastName = student.User.LastName,
                    Email = student.User.Email!,
                    PhoneNumber = student.User.PhoneNumber,
                    Address = student.User.Address,
                    DateOfBirth = student.User.DateOfBirth,
                    EnrollmentDate = student.EnrollmentDate,
                    Department = student.Department,
                    Year = student.Year,
                    IsActive = student.IsActive
                };

                Console.WriteLine($"DEBUG Backend: Returning profile for student: {studentDto.FirstName} {studentDto.LastName}");
                return Ok(studentDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in GetMyProfile: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentDto>> GetStudent(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // Students can only view their own profile, unless user is Admin or Teacher
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Teacher") && student.UserId != currentUserId)
            {
                return Forbid();
            }

            var studentDto = new StudentDto
            {
                Id = student.Id,
                StudentNumber = student.StudentNumber,
                UserId = student.UserId,
                FirstName = student.User.FirstName,
                LastName = student.User.LastName,
                Email = student.User.Email!,
                PhoneNumber = student.User.PhoneNumber,
                Address = student.User.Address,
                DateOfBirth = student.User.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate,
                Department = student.Department,
                Year = student.Year,
                IsActive = student.IsActive
            };

            return Ok(studentDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<StudentDto>> CreateStudent([FromBody] CreateStudentDto createStudentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if student number already exists
            if (await _context.Students.AnyAsync(s => s.StudentNumber == createStudentDto.StudentNumber))
            {
                return BadRequest("Student number already exists.");
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == createStudentDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = createStudentDto.Email,
                Email = createStudentDto.Email,
                FirstName = createStudentDto.FirstName,
                LastName = createStudentDto.LastName,
                PhoneNumber = createStudentDto.PhoneNumber,
                Address = createStudentDto.Address,
                DateOfBirth = createStudentDto.DateOfBirth,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, createStudentDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Add user to Student role
            await _userManager.AddToRoleAsync(user, "Student");

            var student = new Student
            {
                StudentNumber = createStudentDto.StudentNumber,
                UserId = user.Id,
                Department = createStudentDto.Department,
                Year = createStudentDto.Year,
                EnrollmentDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            var studentDto = new StudentDto
            {
                Id = student.Id,
                StudentNumber = student.StudentNumber,
                UserId = student.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                EnrollmentDate = student.EnrollmentDate,
                Department = student.Department,
                Year = student.Year,
                IsActive = student.IsActive
            };

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, studentDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto updateStudentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            student.User.FirstName = updateStudentDto.FirstName;
            student.User.LastName = updateStudentDto.LastName;
            student.User.PhoneNumber = updateStudentDto.PhoneNumber;
            student.User.Address = updateStudentDto.Address;
            student.User.DateOfBirth = updateStudentDto.DateOfBirth;
            student.User.UpdatedAt = DateTime.UtcNow;
            student.Department = updateStudentDto.Department;
            student.Year = updateStudentDto.Year;
            student.IsActive = updateStudentDto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            student.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/courses")]
        public async Task<ActionResult<List<CourseEnrollmentDto>>> GetStudentCourses(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            // Students can only view their own courses, unless user is Admin or Teacher
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Teacher") && student.UserId != currentUserId)
            {
                return Forbid();
            }

            var enrollments = await _context.CourseEnrollments
                .Include(ce => ce.Course)
                .Include(ce => ce.Student)
                .ThenInclude(s => s.User)
                .Where(ce => ce.StudentId == id)
                .Select(ce => new CourseEnrollmentDto
                {
                    Id = ce.Id,
                    StudentId = ce.StudentId,
                    StudentName = $"{ce.Student.User.FirstName} {ce.Student.User.LastName}",
                    StudentNumber = ce.Student.StudentNumber,
                    CourseId = ce.CourseId,
                    CourseName = ce.Course.CourseName,
                    CourseCode = ce.Course.CourseCode,
                    EnrollmentDate = ce.EnrollmentDate,
                    Status = ce.Status,
                    Comments = ce.Comments
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpGet("{id}/grades")]
        public async Task<ActionResult<List<GradeDto>>> GetStudentGrades(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            // Students can only view their own grades, unless user is Admin or Teacher
            if (!userRoles.Contains("Admin") && !userRoles.Contains("Teacher") && student.UserId != currentUserId)
            {
                return Forbid();
            }

            var grades = await _context.Grades
                .Include(g => g.Student)
                .ThenInclude(s => s.User)
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .ThenInclude(t => t.User)
                .Where(g => g.StudentId == id)
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
