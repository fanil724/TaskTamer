using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class UserDTO
    {
        public int UserID { get; set; }
        public EmployeeDTO employeeDTO { get; set; }
        public string Username { get; set; }
        public string? PasswordHash { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        [ValidateNever]
        public RoleDTO roleDTO { get; set; }
        [JsonConstructor]
        public UserDTO()
        {
        }

        
        public UserDTO(User user)
        {
            UserID = user.UserID;
            employeeDTO = new EmployeeDTO(user.Employee);
            roleDTO = new RoleDTO(user.Role);
            RegistrationDate = user.RegistrationDate;
            IsActive = user.IsActive;
            Username = user.Username;
            PasswordHash = "";
        }
    }
}