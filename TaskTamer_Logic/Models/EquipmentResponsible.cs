using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class EquipmentResponsible
{
    [Key]
    public int RecordID { get; set; }
    
    public int EquipmentID { get; set; }
    public Equipment Equipment { get; set; }
    
    public int EmployeeID { get; set; }
    public Employee Employee { get; set; }
    
    public DateTime AssignmentDate { get; set; } = DateTime.Now;
}