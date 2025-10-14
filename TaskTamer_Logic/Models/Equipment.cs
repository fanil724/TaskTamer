using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class Equipment
{
    public int EquipmentID { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(50)]
    public string Model { get; set; }
    
    [StringLength(50)]
    public string SerialNumber { get; set; }
    
    [StringLength(50)]
    public string Type { get; set; }
    
    [StringLength(100)]
    public string Manufacturer { get; set; }
    
    public DateTime? PurchaseDate { get; set; }
    
    public int ResponsibleEmployeeID { get; set; }
    public Employee ResponsibleEmployee { get; set; }
    
    public int DepartmentID { get; set; }
    public Department Department { get; set; }
    
    [StringLength(100)]
    public string Location { get; set; }
    
    [Required]
    [StringLength(100)]
    public string TechnicalDocumentation { get; set; }
    
    public ICollection<Request> Requests { get; set; }
    public ICollection<EquipmentResponsible> AdditionalResponsibles { get; set; }
}