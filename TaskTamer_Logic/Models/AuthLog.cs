using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTamer_Logic.Models;

public class AuthLog
{
    [Key]    
    public int LogID { get; set; }
    //[ForeignKey("UserID")]
    public int? UserID { get; set; }
    public User User { get; set; }
    
    public DateTime LoginTime { get; set; } = DateTime.Now;
    
    [StringLength(50)]
    public string IPAddress { get; set; }
    
    [StringLength(255)]
    public string UserAgent { get; set; }
    
    public bool IsSuccessful { get; set; }
}