using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NLog;
using TaskTamer_Application.Contracts;
using TaskTamer_Logic.Models;
using TaskTamer_Logic.Stores;

namespace TaskTamer_Application.Service;

public interface IAuthService
{
    Task<AuthResult> Authenticate(string username, string password, string ipAddress, string userAgent);
    Task<RegistrationResult> Register(UserRegistrationDto registrationDto);
    Task<PasswordResetResult> ResetPassword(string username, string oldPassword, string newPassword);
    Task<LogoutResult> Logout(int userId, string ipAddress, string userAgent);
    Task<PasswordResetResult> ChangePassword(string username, string oldPassword, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly IUserRepository _userRepository;
    private readonly IAuthlogRepository _authlogRepository;
    private readonly IEmployeeRepository _employeeRepository;


    public AuthService(IConfiguration configuration, IUserRepository userRepository,
        IAuthlogRepository authLogRepository, IEmployeeRepository employeeRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _authlogRepository = authLogRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<AuthResult> Authenticate(string username, string password, string ipAddress, string userAgent)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            var authLog = new AuthLog
            {
                LoginTime = DateTime.Now,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = false,
            };

            if (user != null)
            {
                authLog.UserID = user.UserID;
            }
            if (user == null || !user.IsActive || !VerifyPassword(password, user.PasswordHash))
            {
               
                await _authlogRepository.AddAsync(authLog);
                //TO DO не забыть закоментировать этот код по обновлению пароля 
                user.PasswordHash = HashPassword(password);
                _userRepository.UpdateAsync(user);

                return new AuthResult { Success = false, Message = "Invalid username or password" };
            }
            var token = GenerateJwtToken(user);

            authLog.IsSuccessful = true;
            await _authlogRepository.AddAsync(authLog);

            return new AuthResult
            {
                Success = true,
                Token = token,
                UserId = user.UserID,
                Username = user.Username,
                Role = user.Role.Name,
                Department=user.Employee.Department.Name,
                EmployeeId=user.EmployeeID,
                UserType=user.Employee.UserType
            };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Authentication error");
            return new AuthResult
            { Success = false, Message = "An error occurred during authentication " + ex.Message };
        }
    }

    public async Task<RegistrationResult> Register(UserRegistrationDto registrationDto)
    {
        try
        {
            if (await _userRepository.GetByUsernameAsync(registrationDto.Username) != null)
            {
                return new RegistrationResult { Success = false, Message = "Username already exists" };
            }

            var emp = registrationDto.Employee;
            var empID = await _employeeRepository.AddAsync(new Employee(emp.EmployeeID, emp.FullName,
                emp.positionDTO.PositionID,
                emp.departmentDTO.DepartmentID, emp.Phone, emp.Email, emp.TerminationDate, true));


            var user = new User
            {
                Username = registrationDto.Username,
                PasswordHash = HashPassword(registrationDto.Password),
                EmployeeID = empID,
                RoleID = registrationDto.RoleID,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };


            await _userRepository.AddAsync(user);

            return new RegistrationResult { Success = true, UserId = user.UserID };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Registration error");
            return new RegistrationResult
            { Success = false, Message = "An error occurred during registration " + ex.Message };
        }
    }
    public async Task<PasswordResetResult> ResetPassword(string username, string oldPassword, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
            {
                return new PasswordResetResult { Success = false, Message = "User not found" };
            }

            if (user.PasswordHash != HashPassword(oldPassword))
            {
                return new PasswordResetResult { Success = false, Message = "Incorrect password" };
            }
            user.PasswordHash = HashPassword(newPassword);


            await _userRepository.UpdateAsync(user);

            return new PasswordResetResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Password reset error");
            return new PasswordResetResult { Success = false, Message = "An error occurred " + ex.Message };
        }
    }

    public async Task<PasswordResetResult> ChangePassword(string username, string oldPassword, string newPassword)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
            {
                return new PasswordResetResult { Success = false, Message = "User not found" };
            }

            if (user.PasswordHash != HashPassword(oldPassword))
            {
                return new PasswordResetResult { Success = false, Message = "Incorrect password" };
            }

            if (!IsValidPassword(newPassword))
            {
                return new PasswordResetResult { Success = false, Message = "Incorrect password" };
            }
           
            user.PasswordHash = HashPassword(newPassword);


            await _userRepository.UpdateAsync(user);

            return new PasswordResetResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Password reset error");
            return new PasswordResetResult { Success = false, Message = "An error occurred " + ex.Message };
        }
    }


    public async Task<LogoutResult> Logout(int userId, string ipAddress, string userAgent)
    {
        try
        {
            var authLog = new AuthLog
            {
                UserID = userId,
                LoginTime = DateTime.Now,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = true
            };

            await _authlogRepository.AddAsync(authLog);

            return new LogoutResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Logout error");
            return new LogoutResult { Success = false, Message = ex.Message };
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("AccessLevel", user.Role.AccessLevel.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryHours"])),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        var hash = HashPassword(password);
        return hash == storedHash;
    }

    private string GenerateRandomToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?"":{}|<>]).{8,}$");
        return regex.IsMatch(password);
    }
}
