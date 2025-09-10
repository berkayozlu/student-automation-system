using StudentAutomation.Frontend.Models;

namespace StudentAutomation.Frontend.Services
{
    public interface IApiService
    {
        // Students
        Task<List<StudentDto>> GetStudentsAsync();
        Task<StudentDto?> GetStudentAsync(int id);
        Task<List<CourseDto>> GetStudentCoursesAsync(int studentId);
        Task<List<GradeDto>> GetStudentGradesAsync(int studentId);
        Task<List<StudentDto>> GetAvailableStudentsForCourseAsync(int courseId);
        Task<bool> AddStudentsToCourseAsync(int courseId, List<int> studentIds);

        // Teachers
        Task<List<TeacherDto>> GetTeachersAsync();
        Task<TeacherDto?> GetTeacherAsync(int id);
        Task<List<CourseDto>> GetTeacherCoursesAsync(int teacherId);
    Task<List<CourseDto>> GetTeacherCoursesAsync();
        Task<TeacherDto?> CreateTeacherAsync(CreateTeacherDto createTeacherDto);

        // Courses
        Task<List<CourseDto>> GetCoursesAsync();
        Task<CourseDto?> GetCourseAsync(int id);
        Task<CourseDto?> GetCourseByIdAsync(int courseId);
        Task<CourseDto?> CreateCourseAsync(CreateCourseDto createCourseDto);
        Task<List<CourseStudentDto>> GetCourseStudentsAsync(int courseId);

        // Grades
        Task<List<GradeDto>> GetGradesAsync();
        Task<GradeDto?> CreateGradeAsync(CreateGradeDto createGradeDto);
        Task<List<GradeDto>> GetGradesByCourseAsync(int courseId);

        // Course Enrollments
    Task<List<CourseEnrollmentDto>> GetMyCoursesAsync();
    Task<List<CourseEnrollmentDto>> GetCourseEnrollmentsAsync(int courseId);
    
    // Student Profile
    Task<StudentDto?> GetMyProfileAsync();
    
    // Teacher Profile
    Task<TeacherDto?> GetMyTeacherProfileAsync();
    
    // Student Grades
    Task<List<GradeDto>> GetMyGradesAsync();
    Task<List<GradeDto>> GetMyGradesByCourseAsync(int courseId);

        // Attendance
        Task<List<AttendanceDto>> GetAttendanceAsync();
        Task<List<AttendanceDto>> GetAttendanceByCourseAsync(int courseId);
        Task<AttendanceDto?> CreateAttendanceAsync(CreateAttendanceDto createAttendanceDto);
        Task<List<AttendanceDto>> CreateBulkAttendanceAsync(BulkAttendanceDto bulkAttendanceDto);
        Task<List<AttendanceDto>> GetMyAttendanceAsync();
    }
}
