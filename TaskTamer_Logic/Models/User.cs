using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class User 
{
    [Key]
    public int UserID { get; set; }

    public int EmployeeID { get; set; }
    public Employee Employee { get; set; }

    [Required][StringLength(50)] public string Username { get; set; }

    [Required][StringLength(255)] public string PasswordHash { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;

    public int RoleID { get; set; }
    public Role Role { get; set; }

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
    public ICollection<AuthLog> AuthLogs { get; set; }

    public User() { }
    public User(int id, int employeeID, string userName, string password, DateTime registrDay, bool isActive, int roleID)
    {
        UserID = id;
        EmployeeID = employeeID;
        Username = userName;
        PasswordHash = password;
        RegistrationDate = registrDay;
        IsActive = isActive;
        RoleID = roleID;
    }
}