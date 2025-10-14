using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskTamer_Logic.Models;

public class Request
{
    [Key]
    public int RequestID { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;

    public int AuthorID { get; set; }
    public Employee Author { get; set; }
    
    public int RequestStatusID { get; set; } = 1;
    public RequestStatus RequestStatus { get; set; }

    public int RequestTypeID { get; set; }
    public RequestType RequestType { get; set; }

    public string ProblemDescription { get; set; }

    [Range(1, 5)] 
    public int Priority { get; set; } = 3;

    public int? EquipmentID { get; set; }
    public Equipment Equipment { get; set; }

    public int? ExecutorID { get; set; }
    public Employee Executor { get; set; }

    public DateTime? Deadline { get; set; }
    public DateTime? CompletionDate { get; set; }

    public ICollection<RequestHistory> History { get; set; }
}