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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AttendanceDto>>> GetAttendances()
        {
            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .ThenInclude(t => t.User)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
                    StudentNumber = a.Student.StudentNumber,
                    CourseId = a.CourseId,
                    CourseName = a.Course.CourseName,
                    CourseCode = a.Course.CourseCode,
                    TeacherId = a.TeacherId,
                    TeacherName = $"{a.Teacher.User.FirstName} {a.Teacher.User.LastName}",
                    Date = a.Date,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(attendances);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AttendanceDto>> GetAttendance(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Students can only view their own attendance, teachers can view attendance for their courses
            if (!userRoles.Contains("Admin"))
            {
                if (userRoles.Contains("Student") && attendance.Student.UserId != currentUserId)
                {
                    return Forbid();
                }
                if (userRoles.Contains("Teacher") && attendance.Teacher.UserId != currentUserId)
                {
                    return Forbid();
                }
            }

            var attendanceDto = new AttendanceDto
            {
                Id = attendance.Id,
                StudentId = attendance.StudentId,
                StudentName = $"{attendance.Student.User.FirstName} {attendance.Student.User.LastName}",
                StudentNumber = attendance.Student.StudentNumber,
                CourseId = attendance.CourseId,
                CourseName = attendance.Course.CourseName,
                CourseCode = attendance.Course.CourseCode,
                TeacherId = attendance.TeacherId,
                TeacherName = $"{attendance.Teacher.User.FirstName} {attendance.Teacher.User.LastName}",
                Date = attendance.Date,
                Status = attendance.Status,
                Notes = attendance.Notes,
                CreatedAt = attendance.CreatedAt
            };

            return Ok(attendanceDto);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<AttendanceDto>> CreateAttendance([FromBody] CreateAttendanceDto createAttendanceDto)
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
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == createAttendanceDto.CourseId && c.TeacherId == teacher.Id);
            if (course == null)
            {
                return BadRequest("Course not found or you don't have permission to manage attendance for this course.");
            }

            // Check if student exists and is enrolled in the course
            var enrollment = await _context.CourseEnrollments
                .FirstOrDefaultAsync(ce => ce.StudentId == createAttendanceDto.StudentId && ce.CourseId == createAttendanceDto.CourseId && ce.Status == EnrollmentStatus.Active);

            if (enrollment == null)
            {
                return BadRequest("Student is not enrolled in this course.");
            }

            // Check if attendance already exists for this date
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == createAttendanceDto.StudentId && a.CourseId == createAttendanceDto.CourseId && a.Date.Date == createAttendanceDto.Date.Date);

            if (existingAttendance != null)
            {
                return BadRequest("Attendance record already exists for this student on this date.");
            }

            var attendance = new Attendance
            {
                StudentId = createAttendanceDto.StudentId,
                CourseId = createAttendanceDto.CourseId,
                TeacherId = teacher.Id,
                Date = createAttendanceDto.Date,
                Status = createAttendanceDto.Status,
                Notes = createAttendanceDto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == createAttendanceDto.StudentId);

            var attendanceDto = new AttendanceDto
            {
                Id = attendance.Id,
                StudentId = attendance.StudentId,
                StudentName = $"{student!.User.FirstName} {student.User.LastName}",
                StudentNumber = student.StudentNumber,
                CourseId = attendance.CourseId,
                CourseName = course.CourseName,
                CourseCode = course.CourseCode,
                TeacherId = attendance.TeacherId,
                TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                Date = attendance.Date,
                Status = attendance.Status,
                Notes = attendance.Notes,
                CreatedAt = attendance.CreatedAt
            };

            Console.WriteLine($"DEBUG Backend: Attendance created successfully with ID {attendance.Id}");
            return Ok(attendanceDto);
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<List<AttendanceDto>>> CreateBulkAttendance([FromBody] BulkAttendanceDto bulkAttendanceDto)
        {
            try
            {
                Console.WriteLine($"DEBUG Backend: CreateBulkAttendance called for course {bulkAttendanceDto.CourseId} on {bulkAttendanceDto.Date:yyyy-MM-dd}");
                Console.WriteLine($"DEBUG Backend: Student attendances count: {bulkAttendanceDto.StudentAttendances?.Count ?? 0}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("DEBUG Backend: ModelState is invalid");
                    return BadRequest(ModelState);
                }

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"DEBUG Backend: Current user ID: {currentUserId}");

                var teacher = await _context.Teachers
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserId == currentUserId);

                if (teacher == null)
                {
                    Console.WriteLine("DEBUG Backend: Teacher not found by UserId, trying email lookup");
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    Console.WriteLine($"DEBUG Backend: User email: {userEmail}");

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        teacher = await _context.Teachers
                            .Include(t => t.User)
                            .FirstOrDefaultAsync(t => t.User.Email == userEmail);
                    }
                }

                if (teacher == null)
                {
                    Console.WriteLine("DEBUG Backend: Teacher not found for user ID or email");
                    return BadRequest("Teacher not found.");
                }

                Console.WriteLine($"DEBUG Backend: Teacher found: {teacher.Id} - {teacher.User.FirstName} {teacher.User.LastName}");

                // Check if course exists and belongs to the teacher
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == bulkAttendanceDto.CourseId && c.TeacherId == teacher.Id);
                if (course == null)
                {
                    Console.WriteLine($"DEBUG Backend: Course {bulkAttendanceDto.CourseId} not found or not assigned to teacher {teacher.Id}");
                    return BadRequest("Course not found or you don't have permission to manage attendance for this course.");
                }

                Console.WriteLine($"DEBUG Backend: Course found: {course.CourseName} ({course.CourseCode})");

                // Convert date to UTC to avoid PostgreSQL timezone issues
                var utcDate = bulkAttendanceDto.Date.Kind == DateTimeKind.Local
                    ? bulkAttendanceDto.Date.ToUniversalTime()
                    : bulkAttendanceDto.Date;

                Console.WriteLine($"DEBUG Backend: Original date: {bulkAttendanceDto.Date} (Kind: {bulkAttendanceDto.Date.Kind})");
                Console.WriteLine($"DEBUG Backend: UTC date: {utcDate} (Kind: {utcDate.Kind})");

                var attendanceRecords = new List<Attendance>();
                var attendanceDtos = new List<AttendanceDto>();

                foreach (var studentAttendance in bulkAttendanceDto.StudentAttendances)
                {
                    // Check if student exists and is enrolled in the course
                    var enrollment = await _context.CourseEnrollments
                        .Include(ce => ce.Student)
                        .ThenInclude(s => s.User)
                        .FirstOrDefaultAsync(ce => ce.StudentId == studentAttendance.StudentId && ce.CourseId == bulkAttendanceDto.CourseId && ce.Status == EnrollmentStatus.Active);

                    if (enrollment == null)
                    {
                        continue; // Skip students not enrolled
                    }

                    // Check if attendance already exists for this date (using UTC date)
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == studentAttendance.StudentId && a.CourseId == bulkAttendanceDto.CourseId && a.Date.Date == utcDate.Date);

                    if (existingAttendance != null)
                    {
                        // Update existing attendance
                        existingAttendance.Status = studentAttendance.Status;
                        existingAttendance.Notes = studentAttendance.Notes;
                    }
                    else
                    {
                        // Create new attendance record (using UTC date)
                        var attendance = new Attendance
                        {
                            StudentId = studentAttendance.StudentId,
                            CourseId = bulkAttendanceDto.CourseId,
                            TeacherId = teacher.Id,
                            Date = utcDate,
                            Status = studentAttendance.Status,
                            Notes = studentAttendance.Notes,
                            CreatedAt = DateTime.UtcNow
                        };

                        attendanceRecords.Add(attendance);
                    }

                    attendanceDtos.Add(new AttendanceDto
                    {
                        StudentId = studentAttendance.StudentId,
                        StudentName = $"{enrollment.Student.User.FirstName} {enrollment.Student.User.LastName}",
                        StudentNumber = enrollment.Student.StudentNumber,
                        CourseId = bulkAttendanceDto.CourseId,
                        CourseName = course.CourseName,
                        CourseCode = course.CourseCode,
                        TeacherId = teacher.Id,
                        TeacherName = $"{teacher.User.FirstName} {teacher.User.LastName}",
                        Date = utcDate,
                        Status = studentAttendance.Status,
                        Notes = studentAttendance.Notes,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (attendanceRecords.Any())
                {
                    _context.Attendances.AddRange(attendanceRecords);
                }

                Console.WriteLine($"DEBUG Backend: Saving {attendanceRecords.Count} new attendance records");
                await _context.SaveChangesAsync();

                Console.WriteLine($"DEBUG Backend: Returning {attendanceDtos.Count} attendance DTOs");
                return Ok(attendanceDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in CreateBulkAttendance: {ex.Message}");
                Console.WriteLine($"DEBUG Backend: Stack trace: {ex.StackTrace}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] UpdateAttendanceDto updateAttendanceDto)
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

            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);
            if (attendance == null)
            {
                return NotFound("Attendance record not found or you don't have permission to update this record.");
            }

            attendance.Date = updateAttendanceDto.Date;
            attendance.Status = updateAttendanceDto.Status;
            attendance.Notes = updateAttendanceDto.Notes;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == currentUserId);

            if (teacher == null)
            {
                return BadRequest("Teacher not found.");
            }

            var attendance = await _context.Attendances.FirstOrDefaultAsync(a => a.Id == id && a.TeacherId == teacher.Id);
            if (attendance == null)
            {
                return NotFound("Attendance record not found or you don't have permission to delete this record.");
            }

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<List<AttendanceDto>>> GetAttendanceByCourse(int courseId)
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

            var attendances = await _context.Attendances
                .Include(a => a.Student)
                .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Teacher)
                .ThenInclude(t => t.User)
                .Where(a => a.CourseId == courseId)
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
                    StudentNumber = a.Student.StudentNumber,
                    CourseId = a.CourseId,
                    CourseName = a.Course.CourseName,
                    CourseCode = a.Course.CourseCode,
                    TeacherId = a.TeacherId,
                    TeacherName = $"{a.Teacher.User.FirstName} {a.Teacher.User.LastName}",
                    Date = a.Date,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();

            return Ok(attendances);
        }

        [HttpGet("my-attendance")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<List<AttendanceDto>>> GetMyAttendance()
        {
            try
            {
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"DEBUG Backend: GetMyAttendance - Current user ID: {currentUserId}");

                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.UserId == currentUserId);

                if (student == null)
                {
                    Console.WriteLine("DEBUG Backend: Student not found by UserId, trying email lookup");
                    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                    Console.WriteLine($"DEBUG Backend: User email: {userEmail}");

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        student = await _context.Students
                            .Include(s => s.User)
                            .FirstOrDefaultAsync(s => s.User.Email == userEmail);
                    }
                }

                if (student == null)
                {
                    Console.WriteLine("DEBUG Backend: Student not found for user ID or email");
                    return BadRequest("Student not found.");
                }

                Console.WriteLine($"DEBUG Backend: Student found: {student.Id} - {student.User.FirstName} {student.User.LastName}");

                var attendances = await _context.Attendances
                    .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                    .Include(a => a.Course)
                    .Include(a => a.Teacher)
                    .ThenInclude(t => t.User)
                    .Where(a => a.StudentId == student.Id)
                    .OrderByDescending(a => a.Date)
                    .Select(a => new AttendanceDto
                    {
                        Id = a.Id,
                        StudentId = a.StudentId,
                        StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
                        StudentNumber = a.Student.StudentNumber,
                        CourseId = a.CourseId,
                        CourseName = a.Course.CourseName,
                        CourseCode = a.Course.CourseCode,
                        TeacherId = a.TeacherId,
                        TeacherName = $"{a.Teacher.User.FirstName} {a.Teacher.User.LastName}",
                        Date = a.Date,
                        Status = a.Status,
                        Notes = a.Notes,
                        CreatedAt = a.CreatedAt
                    })
                    .ToListAsync();

                Console.WriteLine($"DEBUG Backend: Found {attendances.Count} attendance records for student {student.Id}");
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG Backend: Exception in GetMyAttendance: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
