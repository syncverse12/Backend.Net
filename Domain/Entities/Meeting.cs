using System;

namespace SyncVerse.Domain.Entities
{
    public class Meeting
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string OrgCode { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public string VivoxChannelName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? Summary { get; set; }
        public string? KeyPoints { get; set; } 
        public string? Decisions { get; set; }
    }
}
