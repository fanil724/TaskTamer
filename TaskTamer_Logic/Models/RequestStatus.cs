using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTamer_Logic.Models;

public class RequestStatus
{
    [Key]   
    public int RequestStatusID { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(200)]
    public string Description { get; set; }

    public int ProcessingOrder { get; set; }

    public ICollection<Request> Requests { get; set; }
    public ICollection<RequestHistory> HistoryRecords { get; set; }
}