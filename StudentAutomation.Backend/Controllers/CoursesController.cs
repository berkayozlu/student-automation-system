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
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<CourseDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Include(c => c.CourseEnrollments)
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

        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .ThenInclude(t => t.User)
                .Include(c => c.CourseEnrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var courseDto = new CourseDto
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Description = course.Description,
                Credits = course.Credits,
                TeacherId = course.TeacherId,
                TeacherName = $"{course.Teacher.User.FirstName} {course.Teacher.User.LastName}",
                Status = course.Status,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                EnrolledStudentsCount = course.CourseEnrollments.Count(ce => ce.Status == EnrollmentStatus.Active)
            };

            return Ok(courseDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CreateCourseDto createCourseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if course code already exists
            if (await _context.Courses.AnyAsync(c => c.CourseCode == createCourseDto.CourseCode))
            {
                return BadRequest("Course code already exists.");
            }

            // Check if teacher exists
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == createCourseDto.TeacherId);
            if (teacher == null)
            {
                return BadRequest("Teacher not found.");
            }

            var course = new Course
            {
                CourseCode = createCourseDto.CourseCode,
                CourseName = createCourseDto.CourseName,
                Description = createCourseDto.Description,
                Credits = createCourseDto.Credits,
                TeacherId = createCourseDto.TeacherId,
                Status = CourseStatus.NotStarted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var courseDto = new CourseDto
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Description = course.Description,
                Credits = course.Credits,
                TeacherId = course.TeacherId,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                Status = course.Status,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                EnrolledStudentsCount = 0
            };

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, courseDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto updateCourseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Teachers can only update their own courses
            if (userRoles.Contains("Teacher") && !userRoles.Contains("Admin") && course.Teacher.UserId != currentUserId)
            {
                return Forbid();
            }

            course.CourseName = updateCourseDto.CourseName;
            course.Description = updateCourseDto.Description;
            course.Credits = updateCourseDto.Credits;
            course.Status = updateCourseDto.Status;
            course.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/students")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult<List<CourseStudentDto>>> GetCourseStudents(int id)
        {
            try
            {
                Console.WriteLine($"DEBUG Backend: Getting students for course {id}");

                var course = await _context.Courses
                    .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                {
                    Console.WriteLine($"DEBUG Backend: Course {id} not found");
                    return NotFound("Course not found.");
                }

                // Check if user is authorized to view this course's students
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin")
                {
                    var teacher = await _context.Teachers
                        .FirstOrDefaultAsync(t => t.UserId == currentUserId);

                    if (teacher == null || course.TeacherId != teacher.Id)
                    {
                        Console.WriteLine($"DEBUG Backend: User not authorized to view students for course {id}");
                        return Forbid("You are not authorized to view students for this course.");
                    }
                }

                var students = await _context.CourseEnrollments
                    .Include(ce => ce.Student)
                    .ThenInclude(s => s.User)
                    .Where(ce => ce.CourseId == id)
                    .Select(ce => new CourseStudentDto
                    {
                        Id = ce.Student.Id,
                        StudentNumber = ce.Student.StudentNumber,
                        FirstName = ce.Student.User.FirstName,
                        LastName = ce.Student.User.LastName,
                        Email = ce.Student.User.Email,
                        EnrollmentDate = ce.EnrollmentDate,
                        EnrollmentStatus = ce.Status,
                        CourseId = id,
                        CourseName = course.CourseName,
                        CourseCode = course.CourseCode
                    })
                    .ToListAsync();

                Console.WriteLine($"DEBUG Backend: Found {students.Count} students for course {id}");
                return Ok(students);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in GetCourseStudents: {ex.Message}");
                Console.WriteLine($"DEBUG Backend: Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{id}/enroll")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<CourseEnrollmentDto>> EnrollStudent(int id, [FromBody] CreateCourseEnrollmentDto enrollmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Teachers can only enroll students in their own courses
            if (userRoles.Contains("Teacher") && !userRoles.Contains("Admin") && course.Teacher.UserId != currentUserId)
            {
                return Forbid();
            }

            var student = await _context.Students.FindAsync(enrollmentDto.StudentId);
            if (student == null)
            {
                return BadRequest("Student not found.");
            }

            // Check if student is already enrolled
            var existingEnrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.StudentId == enrollmentDto.StudentId && ce.CourseId == id);

            if (existingEnrollment != null)
            {
                return BadRequest("Student is already enrolled in this course.");
            }

            var enrollment = new CourseEnrollment
            {
                StudentId = enrollmentDto.StudentId,
                CourseId = id,
                EnrollmentDate = DateTime.UtcNow,
                Status = EnrollmentStatus.Active,
                Comments = enrollmentDto.Comments
            };

            _context.CourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var enrollmentDto2 = new CourseEnrollmentDto
            {
                Id = enrollment.Id,
                StudentId = enrollment.StudentId,
                StudentName = $"{student.User.FirstName} {student.User.LastName}",
                StudentNumber = student.StudentNumber,
                CourseId = enrollment.CourseId,
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                EnrollmentDate = enrollment.EnrollmentDate,
                Status = enrollment.Status,
                Comments = enrollment.Comments
            };

            return Ok(enrollmentDto2);
        }

        [HttpDelete("{courseId}/students/{studentId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UnenrollStudent(int courseId, int studentId)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Teachers can only unenroll students from their own courses
            if (userRoles.Contains("Teacher") && !userRoles.Contains("Admin") && course.Teacher.UserId != currentUserId)
            {
                return Forbid();
            }

            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.StudentId == studentId && ce.CourseId == courseId);

            if (enrollment == null)
            {
                return NotFound("Enrollment not found.");
            }

            enrollment.Status = EnrollmentStatus.Dropped;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<CourseEnrollmentDto>>> GetMyCourses()
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

            var enrollments = await _context.CourseEnrollments
                .Include(ce => ce.Course)
                    .ThenInclude(c => c.Teacher)
                        .ThenInclude(t => t.User)
                .Where(ce => ce.StudentId == student.Id && ce.Status == EnrollmentStatus.Active)
                .Select(ce => new CourseEnrollmentDto
                {
                    Id = ce.Id,
                    CourseId = ce.CourseId,
                    StudentId = ce.StudentId,
                    EnrollmentDate = ce.EnrollmentDate,
                    Status = ce.Status,
                    Comments = ce.Comments,
                    CourseName = ce.Course.CourseName,
                    CourseCode = ce.Course.CourseCode,
                    Course = new CourseDto
                    {
                        Id = ce.Course.Id,
                        CourseCode = ce.Course.CourseCode,
                        CourseName = ce.Course.CourseName,
                        Description = ce.Course.Description,
                        Credits = ce.Course.Credits,
                        TeacherId = ce.Course.TeacherId,
                        TeacherName = $"{ce.Course.Teacher.User.FirstName} {ce.Course.Teacher.User.LastName}",
                        Status = ce.Course.Status,
                        CreatedAt = ce.Course.CreatedAt,
                        UpdatedAt = ce.Course.UpdatedAt
                    }
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        [HttpGet("my-teacher-courses")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<List<CourseDto>>> GetMyTeacherCourses()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value;

                Console.WriteLine($"DEBUG Backend: Current user ID: {currentUserId}");
                Console.WriteLine($"DEBUG Backend: Current user email: {userEmail}");
                Console.WriteLine($"DEBUG Backend: Current user name: {userName}");

                if (string.IsNullOrEmpty(currentUserId))
                {
                    Console.WriteLine("DEBUG Backend: User ID is null or empty");
                    return Unauthorized();
                }

                // Debug: Check all teachers in database
                var allTeachers = await _context.Teachers.Include(t => t.User).ToListAsync();
                Console.WriteLine($"DEBUG Backend: Total teachers in database: {allTeachers.Count}");
                foreach (var t in allTeachers)
                {
                    Console.WriteLine($"DEBUG Backend: Teacher ID: {t.Id}, UserId: {t.UserId}, Email: {t.User.Email}, Name: {t.User.FirstName} {t.User.LastName}");
                }

                var teacher = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserId == currentUserId);

                Console.WriteLine($"DEBUG Backend: Teacher found: {teacher != null}");
                if (teacher != null)
                {
                    Console.WriteLine($"DEBUG Backend: Found Teacher ID: {teacher.Id}, Email: {teacher.User.Email}");
                }
                else
                {
                    Console.WriteLine("DEBUG Backend: No teacher found with current user ID, checking by email...");

                    // Try to find teacher by email as backup
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        teacher = await _context.Teachers
                            .Include(t => t.User)
                            .FirstOrDefaultAsync(t => t.User.Email == userEmail);

                        if (teacher != null)
                        {
                            Console.WriteLine($"DEBUG Backend: Found teacher by email - Teacher ID: {teacher.Id}");
                        }
                        else
                        {
                            Console.WriteLine($"DEBUG Backend: No teacher found with email: {userEmail}");
                        }
                    }
                }

                if (teacher == null)
                {
                    Console.WriteLine("DEBUG Backend: Teacher not found in database");
                    return NotFound("Teacher record not found for current user.");
                }

                // Debug: Check all courses in database
                var allCourses = await _context.Courses.Include(c => c.Teacher).ThenInclude(t => t.User).ToListAsync();
                Console.WriteLine($"DEBUG Backend: Total courses in database: {allCourses.Count}");
                foreach (var c in allCourses)
                {
                    Console.WriteLine($"DEBUG Backend: Course ID: {c.Id}, Name: {c.CourseName}, TeacherId: {c.TeacherId}, Teacher: {c.Teacher?.User?.FirstName} {c.Teacher?.User?.LastName}");
                }

                var courses = await _context.Courses
                    .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                    .Where(c => c.TeacherId == teacher.Id)
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
                        UpdatedAt = c.UpdatedAt
                    })
                    .ToListAsync();

                Console.WriteLine($"DEBUG Backend: Found {courses.Count} courses for teacher ID {teacher.Id}");
                return Ok(courses);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in GetMyTeacherCourses: {ex.Message}");
                Console.WriteLine($"DEBUG Backend: Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{courseId}/available-students")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult<List<StudentDto>>> GetAvailableStudentsForCourse(int courseId)
        {
            try
            {
                Console.WriteLine($"DEBUG Backend: Getting available students for course {courseId}");

                var course = await _context.Courses
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    Console.WriteLine($"DEBUG Backend: Course {courseId} not found");
                    return NotFound("Course not found.");
                }

                // Check if user is authorized to manage this course
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin")
                {
                    var teacher = await _context.Teachers
                        .FirstOrDefaultAsync(t => t.UserId == currentUserId);

                    if (teacher == null || course.TeacherId != teacher.Id)
                    {
                        Console.WriteLine($"DEBUG Backend: User not authorized to manage course {courseId}");
                        return Forbid("You are not authorized to manage this course.");
                    }
                }

                // Get students who are NOT already enrolled in this course
                var enrolledStudentIds = await _context.CourseEnrollments
                    .Where(ce => ce.CourseId == courseId)
                    .Select(ce => ce.StudentId)
                    .ToListAsync();

                var availableStudents = await _context.Students
                    .Include(s => s.User)
                    .Where(s => !enrolledStudentIds.Contains(s.Id))
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        StudentNumber = s.StudentNumber,
                        FirstName = s.User.FirstName,
                        LastName = s.User.LastName,
                        Email = s.User.Email,
                        PhoneNumber = s.User.PhoneNumber,
                        Address = s.User.Address,
                        DateOfBirth = s.User.DateOfBirth,
                        Department = s.Department,
                        Year = s.Year
                    })
                    .ToListAsync();

                Console.WriteLine($"DEBUG Backend: Found {availableStudents.Count} available students for course {courseId}");
                return Ok(availableStudents);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in GetAvailableStudentsForCourse: {ex.Message}");
                Console.WriteLine($"DEBUG Backend: Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("{courseId}/add-students")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult> AddStudentsToCourse(int courseId, [FromBody] AddStudentsToCourseDto request)
        {
            try
            {
                Console.WriteLine($"DEBUG Backend: Adding {request.StudentIds.Count} students to course {courseId}");

                var course = await _context.Courses
                    .Include(c => c.Teacher)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    Console.WriteLine($"DEBUG Backend: Course {courseId} not found");
                    return NotFound("Course not found.");
                }

                // Check if user is authorized to manage this course
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (userRole != "Admin")
                {
                    var teacher = await _context.Teachers
                        .FirstOrDefaultAsync(t => t.UserId == currentUserId);

                    if (teacher == null || course.TeacherId != teacher.Id)
                    {
                        Console.WriteLine($"DEBUG Backend: User not authorized to manage course {courseId}");
                        return Forbid("You are not authorized to manage this course.");
                    }
                }

                // Validate that all students exist and are not already enrolled
                var existingEnrollments = await _context.CourseEnrollments
                    .Where(ce => ce.CourseId == courseId && request.StudentIds.Contains(ce.StudentId))
                    .Select(ce => ce.StudentId)
                    .ToListAsync();

                if (existingEnrollments.Any())
                {
                    return BadRequest($"Some students are already enrolled in this course.");
                }

                var validStudents = await _context.Students
                    .Where(s => request.StudentIds.Contains(s.Id))
                    .ToListAsync();

                if (validStudents.Count != request.StudentIds.Count)
                {
                    return BadRequest("Some student IDs are invalid.");
                }

                // Create enrollments
                var enrollments = request.StudentIds.Select(studentId => new CourseEnrollment
                {
                    CourseId = courseId,
                    StudentId = studentId,
                    EnrollmentDate = DateTime.UtcNow,
                    Status = EnrollmentStatus.Active
                }).ToList();

                _context.CourseEnrollments.AddRange(enrollments);
                await _context.SaveChangesAsync();

                Console.WriteLine($"DEBUG Backend: Successfully added {enrollments.Count} students to course {courseId}");
                return Ok(new { Message = $"Successfully enrolled {enrollments.Count} students in the course." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in AddStudentsToCourse: {ex.Message}");
                Console.WriteLine($"DEBUG Backend: Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
