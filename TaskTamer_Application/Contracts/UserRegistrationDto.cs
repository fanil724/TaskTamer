using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Application.Contracts;

public class UserRegistrationDto
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
    
    public EmployeeDTO Employee { get; set; }
    public int RoleID { get; set; } = 2; // Default role (e.g., "User")
}

public class UserLoginDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}


public class PasswordResetDto
{
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string OLDPassword { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }
}


public class LogoutResult
{
    public bool Success;
    public string Message;
}


public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Role { get; set; }
    public string Department { get; set; }
    public int EmployeeId { get; set; }
    public string UserType { get; set; }
}

public class RegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int UserId { get; set; }
}

public class PasswordResetResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}