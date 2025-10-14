using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class RequestDTO
    {
        public int RequestID { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        [ValidateNever]
        public EmployeeDTO Author { get; set; }
        [ValidateNever]
        public RequestStatusDTO RequestStatus { get; set; }
        [ValidateNever]
        public RequestTypeDTO RequestType { get; set; }
        public string ProblemDescription { get; set; }
        public int Priority { get; set; } = 3;
        [ValidateNever]
        public EquipmentDTO Equipment { get; set; }
        [ValidateNever]
        public EmployeeDTO Executor { get; set; }
        public DateTime? Deadline { get; set; }
        public DateTime? CompletionDate { get; set; }
        public ICollection<RequestHistoryDTO> History { get; set; }=new List<RequestHistoryDTO>();


        [JsonConstructor]
        public RequestDTO()
        {
        }

        public RequestDTO(Request request)
        {
            RequestID = request.RequestID;
            CreationDate = request.CreationDate;
            Author = new EmployeeDTO(request.Author);
            RequestStatus = new RequestStatusDTO(request.RequestStatus);
            RequestType = new RequestTypeDTO(request.RequestType);
            ProblemDescription = request.ProblemDescription;
            Priority = request.Priority;
            Equipment = new EquipmentDTO(request.Equipment);
            Executor = new EmployeeDTO(request.Executor);
            Deadline = request.Deadline;
            CompletionDate = request.CompletionDate;
            History = request.History.Where(x => x != null).Select(x => new RequestHistoryDTO(x)).ToList();
        }
    }
}