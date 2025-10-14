using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class RequestHistoryDTO
    {
        public int HistoryID { get; set; }
       // public RequestDTO Request { get; set; }
        public DateTime ChangeDate { get; set; } = DateTime.Now;
        public RequestStatusDTO Status { get; set; }
        public string Comment { get; set; }
        public EmployeeDTO ChangedBy { get; set; }

        [JsonConstructor]
        public RequestHistoryDTO() { }

        public RequestHistoryDTO(RequestHistory requestHistory)
        {
            HistoryID = requestHistory.HistoryID;
          //  Request = new RequestDTO(requestHistory.Request);
            ChangeDate = requestHistory.ChangeDate;
            Status = new RequestStatusDTO(requestHistory.Status);
            Comment = requestHistory.Comment;
            ChangedBy = new EmployeeDTO(requestHistory.ChangedBy);
        }

    }
}
