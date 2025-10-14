
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class EmployeeDTO
    {
        public int EmployeeID { get; set; }

        public string FullName { get; set; }
        [ValidateNever]
        public PositionDTO positionDTO { get; set; }
        [ValidateNever]
        public DepartmentDTO departmentDTO { get; set; }

        [RegularExpression(@"\+7\([0-9]{3}\)[0-9]{7}")]
        public string Phone { get; set; }

        public string Email { get; set; }

        public string UserType { get; set; } = "Employee";

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public DateTime? TerminationDate { get; set; }
        public bool IsActive { get; set; } = true;

        [JsonConstructor]
        public EmployeeDTO() { }
        public EmployeeDTO(Employee employee)
        {
            EmployeeID = employee.EmployeeID;
            FullName = employee.FullName;
            positionDTO = new PositionDTO(employee.Position);
            departmentDTO = new DepartmentDTO(employee.Department);
            Phone = employee.Phone;
            Email = employee.Email;
            UserType = employee.UserType;
            RegistrationDate = employee.RegistrationDate;
            TerminationDate = employee.TerminationDate;
            IsActive = employee.IsActive;
        }

    }
}
