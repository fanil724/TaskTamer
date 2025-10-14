using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class PasswordResetToken
{
    [Key]
    public int TokenID { get; set; }
    
    public int UserID { get; set; }
    public User User { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Token { get; set; }
    
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime ExpirationDate { get; set; }
    public bool IsUsed { get; set; } = false;
}