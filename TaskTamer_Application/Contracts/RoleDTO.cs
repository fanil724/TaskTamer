using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class RoleDTO
    {
        public int RoleID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int AccessLevel { get; set; } = 1;
        [JsonConstructor]
        public RoleDTO() { }
        public RoleDTO(Role role)
        {
            RoleID = role.RoleID;
            Name = role.Name;
            Description = role.Description;
            AccessLevel = role.AccessLevel;
        }
    }
}
