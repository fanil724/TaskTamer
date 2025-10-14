namespace TaskTamer_Application.Contracts
{
    public class EventDTO
    {
        public int RequestId { get; set; } = 0;
        public string EventName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;

        public string userName { get; set; } = string.Empty;
    }
}
