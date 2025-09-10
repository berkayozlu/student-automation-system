using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Backend.Data;
using StudentAutomation.Backend.DTOs;
using StudentAutomation.Backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace StudentAutomation.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = await _tokenService.GenerateTokenAsync(user);

            var userDto = await MapToUserDto(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                User = userDto
            };
        }

        private async Task<string> GenerateUniqueStudentNumberAsync()
        {
            string studentNumber;
            do
            {
                // Generate student number in format: STU + current year + random 4 digits
                var year = DateTime.Now.Year.ToString();
                var random = new Random().Next(1000, 9999).ToString();
                studentNumber = $"STU{year}{random}";
            }
            while (await _context.Students.AnyAsync(s => s.StudentNumber == studentNumber));
            
            return studentNumber;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User with this email already exists."
                };
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                DateOfBirth = registerDto.DateOfBirth.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Assign role
            await AssignRoleAsync(user.Id, registerDto.Role);

            // Create role-specific records
            if (registerDto.Role.ToLower() == "student")
            {
                // Generate unique student number if not provided
                string studentNumber = registerDto.StudentNumber;
                if (string.IsNullOrEmpty(studentNumber))
                {
                    studentNumber = await GenerateUniqueStudentNumberAsync();
                }
                else
                {
                    // Check if provided student number already exists
                    if (await _context.Students.AnyAsync(s => s.StudentNumber == studentNumber))
                    {
                        studentNumber = await GenerateUniqueStudentNumberAsync();
                    }
                }

                var student = new Student
                {
                    StudentNumber = studentNumber,
                    UserId = user.Id,
                    Department = registerDto.Department,
                    Year = registerDto.Year,
                    EnrollmentDate = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Students.Add(student);
            }
            else if (registerDto.Role.ToLower() == "teacher" && !string.IsNullOrEmpty(registerDto.EmployeeNumber))
            {
                var teacher = new Teacher
                {
                    EmployeeNumber = registerDto.EmployeeNumber,
                    UserId = user.Id,
                    Department = registerDto.Department,
                    Title = registerDto.Title,
                    HireDate = DateTime.UtcNow,
                    IsActive = true
                };
                _context.Teachers.Add(teacher);
            }

            await _context.SaveChangesAsync();

            var token = await _tokenService.GenerateTokenAsync(user);
            var userDto = await MapToUserDto(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful.",
                Token = token,
                User = userDto
            };
        }

        public async Task<AuthResponseDto> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            var userDto = await MapToUserDto(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "User profile retrieved successfully.",
                User = userDto
            };
        }

        public async Task<bool> AssignRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            return result.Succeeded;
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        private async Task<UserDto> MapToUserDto(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Roles = roles.ToList()
            };

            // Load student or teacher data if applicable
            if (roles.Contains("Student"))
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);
                if (student != null)
                {
                    userDto.Student = new StudentDto
                    {
                        Id = student.Id,
                        StudentNumber = student.StudentNumber,
                        UserId = student.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        DateOfBirth = user.DateOfBirth,
                        EnrollmentDate = student.EnrollmentDate,
                        Department = student.Department,
                        Year = student.Year,
                        IsActive = student.IsActive
                    };
                }
            }

            if (roles.Contains("Teacher"))
            {
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);
                if (teacher != null)
                {
                    userDto.Teacher = new TeacherDto
                    {
                        Id = teacher.Id,
                        EmployeeNumber = teacher.EmployeeNumber,
                        UserId = teacher.UserId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber,
                        Address = user.Address,
                        DateOfBirth = user.DateOfBirth,
                        Department = teacher.Department,
                        Title = teacher.Title,
                        HireDate = teacher.HireDate,
                        IsActive = teacher.IsActive
                    };
                }
            }

            return userDto;
        }
    }
}
