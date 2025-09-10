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
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeachersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("my-profile")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<TeacherDto>> GetMyProfile()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"DEBUG Backend: Getting teacher profile for user ID: {currentUserId}");

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var teacher = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserId == currentUserId);

                if (teacher == null)
                {
                    Console.WriteLine($"DEBUG Backend: Teacher not found for user ID: {currentUserId}");
                    return NotFound("Teacher profile not found.");
                }

                var teacherDto = new TeacherDto
                {
                    Id = teacher.Id,
                    EmployeeNumber = teacher.EmployeeNumber,
                    UserId = teacher.UserId,
                    FirstName = teacher.User.FirstName,
                    LastName = teacher.User.LastName,
                    Email = teacher.User.Email!,
                    PhoneNumber = teacher.User.PhoneNumber,
                    Address = teacher.User.Address,
                    DateOfBirth = teacher.User.DateOfBirth,
                    Department = teacher.Department,
                    Title = teacher.Title,
                    HireDate = teacher.HireDate,
                    IsActive = teacher.IsActive
                };

                Console.WriteLine($"DEBUG Backend: Returning teacher profile for: {teacherDto.FirstName} {teacherDto.LastName}");
                return Ok(teacherDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in GetMyProfile: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<TeacherDto>>> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.User)
                .Where(t => t.IsActive)
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    EmployeeNumber = t.EmployeeNumber,
                    UserId = t.UserId,
                    FirstName = t.User.FirstName,
                    LastName = t.User.LastName,
                    Email = t.User.Email!,
                    PhoneNumber = t.User.PhoneNumber,
                    Address = t.User.Address,
                    DateOfBirth = t.User.DateOfBirth,
                    Department = t.Department,
                    Title = t.Title,
                    HireDate = t.HireDate,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            return Ok(teachers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherDto>> GetTeacher(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Teachers can only view their own profile, unless user is Admin
            if (!userRoles.Contains("Admin") && teacher.UserId != currentUserId)
            {
                return Forbid();
            }

            var teacherDto = new TeacherDto
            {
                Id = teacher.Id,
                EmployeeNumber = teacher.EmployeeNumber,
                UserId = teacher.UserId,
                FirstName = teacher.User.FirstName,
                LastName = teacher.User.LastName,
                Email = teacher.User.Email!,
                PhoneNumber = teacher.User.PhoneNumber,
                Address = teacher.User.Address,
                DateOfBirth = teacher.User.DateOfBirth,
                Department = teacher.Department,
                Title = teacher.Title,
                HireDate = teacher.HireDate,
                IsActive = teacher.IsActive
            };

            return Ok(teacherDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TeacherDto>> CreateTeacher([FromBody] CreateTeacherDto createTeacherDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if employee number already exists
            if (await _context.Teachers.AnyAsync(t => t.EmployeeNumber == createTeacherDto.EmployeeNumber))
            {
                return BadRequest("Employee number already exists.");
            }

            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == createTeacherDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = createTeacherDto.Email,
                Email = createTeacherDto.Email,
                FirstName = createTeacherDto.FirstName,
                LastName = createTeacherDto.LastName,
                PhoneNumber = createTeacherDto.PhoneNumber,
                Address = createTeacherDto.Address,
                DateOfBirth = createTeacherDto.DateOfBirth.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, createTeacherDto.Password);
            if (!result.Succeeded)
            {
                return BadRequest($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            // Add user to Teacher role
            await _userManager.AddToRoleAsync(user, "Teacher");

            var teacher = new Teacher
            {
                EmployeeNumber = createTeacherDto.EmployeeNumber,
                UserId = user.Id,
                Department = createTeacherDto.Department,
                Title = createTeacherDto.Title,
                HireDate = createTeacherDto.HireDate.ToUniversalTime(),
                IsActive = true
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            var teacherDto = new TeacherDto
            {
                Id = teacher.Id,
                EmployeeNumber = teacher.EmployeeNumber,
                UserId = teacher.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Department = teacher.Department,
                Title = teacher.Title,
                HireDate = teacher.HireDate,
                IsActive = teacher.IsActive
            };

            return CreatedAtAction(nameof(GetTeacher), new { id = teacher.Id }, teacherDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherDto updateTeacherDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            teacher.User.FirstName = updateTeacherDto.FirstName;
            teacher.User.LastName = updateTeacherDto.LastName;
            teacher.User.PhoneNumber = updateTeacherDto.PhoneNumber;
            teacher.User.Address = updateTeacherDto.Address;
            teacher.User.DateOfBirth = updateTeacherDto.DateOfBirth;
            teacher.User.UpdatedAt = DateTime.UtcNow;
            teacher.Department = updateTeacherDto.Department;
            teacher.Title = updateTeacherDto.Title;
            teacher.IsActive = updateTeacherDto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/courses")]
        public async Task<ActionResult<List<CourseDto>>> GetTeacherCourses(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            // Teachers can only view their own courses, unless user is Admin
            if (!userRoles.Contains("Admin") && teacher.UserId != currentUserId)
            {
                return Forbid();
            }

            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Where(c => c.TeacherId == id)
                .Select(c => new CourseDto
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Description = c.Description,
                    Credits = c.Credits,
                    TeacherId = c.TeacherId,
                    TeacherName = $"{c.Teacher.User.FirstName} {c.Teacher.User.LastName}",
                    Status = c.Status,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    EnrolledStudentsCount = c.CourseEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active)
                })
                .ToListAsync();

            return Ok(courses);
        }
    }
}
