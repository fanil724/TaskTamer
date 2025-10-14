using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class DepartmentDTO
    {
        public int DepartmentID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DepartmentType { get; set; } = "Production";
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        [JsonConstructor]
        public DepartmentDTO() { }
        public DepartmentDTO(Department department)
        {
            DepartmentID = department.DepartmentID;
            Name = department.Name;
            Description = department.Description;
            DepartmentType = department.DepartmentType;
            CreationDate = department.CreationDate;
            IsActive = department.IsActive;
        }

    }
}
