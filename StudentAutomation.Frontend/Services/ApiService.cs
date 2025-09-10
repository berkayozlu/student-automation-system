using StudentAutomation.Frontend.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StudentAutomation.Frontend.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public ApiService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _authService.GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"Setting Authorization header with token: {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            else
            {
                Console.WriteLine("No token available for authorization");
            }
        }

        // Students
        public async Task<List<StudentDto>> GetStudentsAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<StudentDto>>("api/students");
                return response ?? new List<StudentDto>();
            }
            catch
            {
                return new List<StudentDto>();
            }
        }

        public async Task<StudentDto?> GetStudentAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<StudentDto>($"api/students/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<CourseDto>> GetStudentCoursesAsync(int studentId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CourseDto>>($"api/students/{studentId}/courses");
                return response ?? new List<CourseDto>();
            }
            catch
            {
                return new List<CourseDto>();
            }
        }

        public async Task<List<GradeDto>> GetStudentGradesAsync(int studentId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<GradeDto>>($"api/students/{studentId}/grades");
                return response ?? new List<GradeDto>();
            }
            catch
            {
                return new List<GradeDto>();
            }
        }

        public async Task<List<StudentDto>> GetAvailableStudentsForCourseAsync(int courseId)
        {
            try
            {
                Console.WriteLine($"DEBUG: Getting available students for course {courseId}");
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.GetFromJsonAsync<List<StudentDto>>($"api/courses/{courseId}/available-students");
                var students = response ?? new List<StudentDto>();
                Console.WriteLine($"DEBUG: Found {students.Count} available students for course {courseId}");
                return students;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception getting available students: {ex.Message}");
                return new List<StudentDto>();
            }
        }

        public async Task<bool> AddStudentsToCourseAsync(int courseId, List<int> studentIds)
        {
            try
            {
                Console.WriteLine($"DEBUG: Adding {studentIds.Count} students to course {courseId}");
                await SetAuthorizationHeaderAsync();
                
                var enrollmentData = new { StudentIds = studentIds };
                var response = await _httpClient.PostAsJsonAsync($"api/courses/{courseId}/add-students", enrollmentData);
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"DEBUG: Successfully added students to course {courseId}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: Failed to add students to course. Status: {response.StatusCode}, Error: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception adding students to course: {ex.Message}");
                return false;
            }
        }

        // Teachers
        public async Task<List<TeacherDto>> GetTeachersAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<TeacherDto>>("api/teachers");
                return response ?? new List<TeacherDto>();
            }
            catch
            {
                return new List<TeacherDto>();
            }
        }

        public async Task<TeacherDto?> GetTeacherAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<TeacherDto>($"api/teachers/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<CourseDto>> GetTeacherCoursesAsync(int teacherId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CourseDto>>($"api/teachers/{teacherId}/courses");
                return response ?? new List<CourseDto>();
            }
            catch
            {
                return new List<CourseDto>();
            }
        }

        public async Task<List<CourseDto>> GetTeacherCoursesAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine("DEBUG ApiService: Making request to /api/courses/my-teacher-courses");
                var response = await _httpClient.GetFromJsonAsync<List<CourseDto>>("api/courses/my-teacher-courses");
                Console.WriteLine($"DEBUG ApiService: Received response with {response?.Count ?? 0} courses");
                return response ?? new List<CourseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG ApiService: Exception - {ex.Message}");
                Console.WriteLine($"DEBUG ApiService: Stack trace - {ex.StackTrace}");
                return new List<CourseDto>();
            }
        }

        public async Task<TeacherDto?> CreateTeacherAsync(CreateTeacherDto createTeacherDto)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/teachers", createTeacherDto);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("You don't have permission to create teachers. Admin role required.");
                    }
                    throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
                }

                // Parse response from string since we already read it
                var result = System.Text.Json.JsonSerializer.Deserialize<TeacherDto>(responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create teacher: {ex.Message}");
            }
        }

        // Courses
        public async Task<List<CourseDto>> GetCoursesAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CourseDto>>("api/courses");
                return response ?? new List<CourseDto>();
            }
            catch
            {
                return new List<CourseDto>();
            }
        }

        public async Task<CourseDto?> GetCourseAsync(int id)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                return await _httpClient.GetFromJsonAsync<CourseDto>($"api/courses/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<CourseDto?> CreateCourseAsync(CreateCourseDto createCourseDto)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/courses", createCourseDto);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("You don't have permission to create courses. Admin role required.");
                    }
                    throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<CourseDto>(responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create course: {ex.Message}");
            }
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<CourseDto>($"api/courses/{courseId}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting course by ID: {ex.Message}");
                return null;
            }
        }

        public async Task<List<CourseStudentDto>> GetCourseStudentsAsync(int courseId)
        {
            try
            {
                Console.WriteLine($"DEBUG: Calling GetCourseStudentsAsync for course {courseId}");
                await SetAuthorizationHeaderAsync();
                var response = await _httpClient.GetFromJsonAsync<List<CourseStudentDto>>($"api/courses/{courseId}/students");
                var students = response ?? new List<CourseStudentDto>();
                Console.WriteLine($"DEBUG: GetCourseStudentsAsync returned {students.Count} students");
                return students;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception in GetCourseStudentsAsync: {ex.Message}");
                return new List<CourseStudentDto>();
            }
        }

        // Course Enrollments
        public async Task<List<CourseEnrollmentDto>> GetMyCoursesAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine("DEBUG: Calling GetMyCoursesAsync");
                var response = await _httpClient.GetFromJsonAsync<List<CourseEnrollmentDto>>("api/courses/my-courses");
                Console.WriteLine($"DEBUG: GetMyCoursesAsync response received: {response?.Count ?? 0} courses");
                return response ?? new List<CourseEnrollmentDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: GetMyCoursesAsync error: {ex.Message}");
                Console.WriteLine($"DEBUG: GetMyCoursesAsync stack trace: {ex.StackTrace}");
                throw; // Re-throw to let the calling code handle it
            }
        }

        public async Task<List<CourseEnrollmentDto>> GetCourseEnrollmentsAsync(int courseId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<CourseEnrollmentDto>>($"api/courses/{courseId}/enrollments");
                return response ?? new List<CourseEnrollmentDto>();
            }
            catch
            {
                return new List<CourseEnrollmentDto>();
            }
        }

        // Grades
        public async Task<List<GradeDto>> GetGradesAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<GradeDto>>("api/grades");
                return response ?? new List<GradeDto>();
            }
            catch
            {
                return new List<GradeDto>();
            }
        }

        public async Task<GradeDto?> CreateGradeAsync(CreateGradeDto createGradeDto)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine($"DEBUG: Creating grade for student {createGradeDto.StudentId} in course {createGradeDto.CourseId}");
                var response = await _httpClient.PostAsJsonAsync("api/grades", createGradeDto);
                
                Console.WriteLine($"DEBUG: CreateGrade response status: {response.StatusCode}");
                Console.WriteLine($"DEBUG: CreateGrade response success: {response.IsSuccessStatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GradeDto>();
                    Console.WriteLine($"DEBUG: CreateGrade success - Grade ID: {result?.Id}");
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: CreateGrade error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: CreateGrade exception: {ex.Message}");
                Console.WriteLine($"DEBUG: CreateGrade exception stack: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<List<GradeDto>> GetGradesByCourseAsync(int courseId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<GradeDto>>($"api/grades/course/{courseId}");
                return response ?? new List<GradeDto>();
            }
            catch
            {
                return new List<GradeDto>();
            }
        }

        // Attendance methods
        public async Task<List<AttendanceDto>> GetAttendanceAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AttendanceDto>>("api/attendance");
                return response ?? new List<AttendanceDto>();
            }
            catch
            {
                return new List<AttendanceDto>();
            }
        }

        public async Task<List<AttendanceDto>> GetAttendanceByCourseAsync(int courseId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AttendanceDto>>($"api/attendance/course/{courseId}");
                return response ?? new List<AttendanceDto>();
            }
            catch
            {
                return new List<AttendanceDto>();
            }
        }

        public async Task<AttendanceDto?> CreateAttendanceAsync(CreateAttendanceDto createAttendanceDto)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine($"DEBUG: Creating attendance for student {createAttendanceDto.StudentId} in course {createAttendanceDto.CourseId}");
                var response = await _httpClient.PostAsJsonAsync("api/attendance", createAttendanceDto);
                
                Console.WriteLine($"DEBUG: CreateAttendance response status: {response.StatusCode}");
                Console.WriteLine($"DEBUG: CreateAttendance response success: {response.IsSuccessStatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AttendanceDto>();
                    Console.WriteLine($"DEBUG: CreateAttendance success - Attendance ID: {result?.Id}");
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: CreateAttendance error: {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: CreateAttendance exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AttendanceDto>> CreateBulkAttendanceAsync(BulkAttendanceDto bulkAttendanceDto)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine($"DEBUG: Creating bulk attendance for course {bulkAttendanceDto.CourseId} on {bulkAttendanceDto.Date:yyyy-MM-dd}");
                var response = await _httpClient.PostAsJsonAsync("api/attendance/bulk", bulkAttendanceDto);
                
                Console.WriteLine($"DEBUG: CreateBulkAttendance response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<AttendanceDto>>();
                    Console.WriteLine($"DEBUG: CreateBulkAttendance success - {result?.Count} records");
                    return result ?? new List<AttendanceDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: CreateBulkAttendance error: {errorContent}");
                    return new List<AttendanceDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: CreateBulkAttendance exception: {ex.Message}");
                return new List<AttendanceDto>();
            }
        }

        public async Task<List<AttendanceDto>> GetMyAttendanceAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine("DEBUG: Getting my attendance records");
                var response = await _httpClient.GetAsync("api/attendance/my-attendance");
                
                Console.WriteLine($"DEBUG: GetMyAttendance response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<AttendanceDto>>();
                    Console.WriteLine($"DEBUG: GetMyAttendance success - {result?.Count} records");
                    return result ?? new List<AttendanceDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DEBUG: GetMyAttendance error: {errorContent}");
                    return new List<AttendanceDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: GetMyAttendance exception: {ex.Message}");
                return new List<AttendanceDto>();
            }
        }


        // Student Profile
        public async Task<StudentDto?> GetMyProfileAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<StudentDto>("api/students/my-profile");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error getting student profile: {ex.Message}");
                return null;
            }
        }

        // Teacher Profile
        public async Task<TeacherDto?> GetMyTeacherProfileAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<TeacherDto>("api/teachers/my-profile");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Error getting teacher profile: {ex.Message}");
                return null;
            }
        }

        // Student Grades
        public async Task<List<GradeDto>> GetMyGradesAsync()
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                Console.WriteLine("DEBUG: Calling GetMyGradesAsync");
                var response = await _httpClient.GetFromJsonAsync<List<GradeDto>>("api/grades/my-grades");
                Console.WriteLine($"DEBUG: GetMyGradesAsync response received: {response?.Count ?? 0} grades");
                return response ?? new List<GradeDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: GetMyGradesAsync error: {ex.Message}");
                Console.WriteLine($"DEBUG: GetMyGradesAsync stack trace: {ex.StackTrace}");
                throw; // Re-throw to let the calling code handle it
            }
        }

        public async Task<List<GradeDto>> GetMyGradesByCourseAsync(int courseId)
        {
            await SetAuthorizationHeaderAsync();
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<GradeDto>>($"api/grades/my-grades/{courseId}");
                return response ?? new List<GradeDto>();
            }
            catch
            {
                return new List<GradeDto>();
            }
        }
    }
}
