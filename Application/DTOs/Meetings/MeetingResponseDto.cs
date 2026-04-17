using System;

namespace SyncVerse.Application.DTOs.Meetings
{
    public class MeetingResponseDto
    {
        public string Id { get; set; } = null!;
        public string OrgCode { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public string VivoxChannelName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
