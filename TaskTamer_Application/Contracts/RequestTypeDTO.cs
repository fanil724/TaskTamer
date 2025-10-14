using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class RequestTypeDTO
    {
        public int RequestTypeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonConstructor]
        public RequestTypeDTO() { }
        public RequestTypeDTO(RequestType requestType) {
            RequestTypeID = requestType.RequestTypeID;
            Name = requestType.Name;
            Description = requestType.Description;
        }

    }
}
