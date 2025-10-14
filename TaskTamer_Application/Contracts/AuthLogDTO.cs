using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class AuthLogDTO
    {
        public int LogID { get; set; }
        public UserDTO User { get; set; }
        public DateTime LoginTime { get; set; } = DateTime.Now;
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
        public bool IsSuccessful { get; set; }
        [JsonConstructor]
        public AuthLogDTO() { }
        public AuthLogDTO(AuthLog authLog)
        {
            LogID = authLog.LogID;
            User = new UserDTO(authLog.User);
            UserAgent = authLog.UserAgent;
            IPAddress = authLog.IPAddress;
            IsSuccessful = authLog.IsSuccessful;
        }
    }
}
