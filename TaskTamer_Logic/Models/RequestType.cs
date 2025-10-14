using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class RequestType
{ 
    [Key]
    public int RequestTypeID { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; }
    
    [StringLength(200)]
    public string Description { get; set; }
    
    public ICollection<Request> Requests { get; set; }
}