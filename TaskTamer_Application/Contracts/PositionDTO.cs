using System.Text.Json.Serialization;
using TaskTamer_Logic.Models;

namespace TaskTamer_Application.Contracts
{
    public class PositionDTO
    {
        public int PositionID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int AccessLevel { get; set; } = 1;

        [JsonConstructor]
        public PositionDTO() { }
        
        public PositionDTO(Position position)
        {
            PositionID = position.PositionID;
            Title = position.Title;
            Description = position.Description;
            AccessLevel = position.AccessLevel;
        }
    }
}
