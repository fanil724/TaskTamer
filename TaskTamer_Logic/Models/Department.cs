using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class Department
{
    public int DepartmentID { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; }
    
    [StringLength(200)]
    public string Description { get; set; }
    
    [Required]
    [StringLength(30)]
    public string DepartmentType { get; set; } = "Production";
    
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    
    public ICollection<Employee> Employees { get; set; }
    public ICollection<Equipment> Equipment { get; set; }


    public Department() { }
}