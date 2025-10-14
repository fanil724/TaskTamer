using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTamer_Logic.Models;

public class Position
{
    public int PositionID { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Title { get; set; }
    
    [StringLength(200)]
    public string Description { get; set; }
    
    [Range(1, 10)]
    public int AccessLevel { get; set; } = 1;
    
    public ICollection<Employee> Employees { get; set; }
}