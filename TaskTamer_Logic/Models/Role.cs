
using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class Role
{
    [Key]
    public int RoleID { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(200)]
    public string Description { get; set; }

    public int AccessLevel { get; set; } = 1;

    public ICollection<User> Users { get; set; }

    public Role() { }
    public Role(int id, string name, string desciption, int accesLevel)
    {
        RoleID = id;
        Name = name;
        Description = desciption;
        AccessLevel = accesLevel;
    }
}