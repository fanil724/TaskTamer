using System.ComponentModel.DataAnnotations;

namespace TaskTamer_Logic.Models;

public class RequestHistory
{
    [Key]
    public int HistoryID { get; set; }
    
    public int RequestID { get; set; }
    public Request Request { get; set; }
    
    public DateTime ChangeDate { get; set; } = DateTime.Now;
    
    public int? RequestStatusID { get; set; }
    public RequestStatus Status { get; set; }
    
    public string Comment { get; set; }
    
    public int ChangedByID { get; set; }
    public Employee ChangedBy { get; set; }
}