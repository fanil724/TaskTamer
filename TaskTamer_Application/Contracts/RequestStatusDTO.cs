using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class RequestStatusDTO
    {
        public int StatusID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProcessingOrder { get; set; }

        [JsonConstructor]
        public RequestStatusDTO()
        {

        }

        public RequestStatusDTO(RequestStatus requestStatus)
        {
            StatusID = requestStatus.RequestStatusID;
            Name = requestStatus.Name;
            Description = requestStatus.Description;
            ProcessingOrder = requestStatus.ProcessingOrder;

        }
    }
}
