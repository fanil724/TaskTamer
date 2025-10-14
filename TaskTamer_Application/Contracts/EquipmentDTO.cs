using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class EquipmentDTO
    {
        public int EquipmentID { get; set; } = 0;

        public string Name { get; set; }

        public string Model { get; set; }

        public string SerialNumber { get; set; }

        public string Type { get; set; }

        public string Manufacturer { get; set; }
        public DateTime? PurchaseDate { get; set; }
        [ValidateNever]
        public EmployeeDTO ResponsibleEmployee { get; set; }
        [ValidateNever]
        public DepartmentDTO departmentDTO { get; set; }

        public string Location { get; set; }
        [ValidateNever]
        public string TechnicalDocumentation { get; set; }

        public EquipmentDTO() { }
        public EquipmentDTO(Equipment equipment)
        {
            EquipmentID = equipment.EquipmentID;
            Name = equipment.Name;
            Model = equipment.Model;
            SerialNumber = equipment.SerialNumber;
            Type = equipment.Type;
            Manufacturer = equipment.Manufacturer;
            PurchaseDate = equipment.PurchaseDate;
            ResponsibleEmployee = new EmployeeDTO(equipment.ResponsibleEmployee);
            departmentDTO = new DepartmentDTO(equipment.Department);
            Location = equipment.Location;
            TechnicalDocumentation = equipment.TechnicalDocumentation;
        }

    }
}
