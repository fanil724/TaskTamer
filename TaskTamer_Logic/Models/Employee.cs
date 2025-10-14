using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class Employee
{
    public int EmployeeID { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    public int PositionID { get; set; }
    public Position Position { get; set; }

    public int DepartmentID { get; set; }
    public Department Department { get; set; }

    [StringLength(20)]
    [RegularExpression(@"\+7\([0-9]{3}\)[0-9]{7}")]
    public string Phone { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(20)]
    public string UserType { get; set; } = "Employee";

    public DateTime RegistrationDate { get; set; } = DateTime.Now;
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Equipment> ResponsibleEquipment { get; set; }
    public ICollection<Request> CreatedRequests { get; set; }
    public ICollection<Request> ExecutedRequests { get; set; }
    
    public ICollection<EquipmentResponsible> EquipmentResponsibilities { get; set; }

    public Employee() { }
    public Employee(int id, string fullName, int positionID, int departmentID, string phone,
        string email, DateTime? dateTime, bool isActive, string userType = "Employee")
    {
        EmployeeID = id;
        FullName = fullName;
        PositionID = positionID;
        DepartmentID = departmentID;
        Phone = phone;
        Email = email;
        UserType = userType;
        TerminationDate = dateTime;
        IsActive = isActive;
    }
}