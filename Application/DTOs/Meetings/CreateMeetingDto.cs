namespace SyncVerse.Application.DTOs.Meetings
{
    public class CreateMeetingDto
    {
        public string OrgCode { get; set; } = null!;
        public string RoomId { get; set; } = null!;
        public string VivoxChannelName { get; set; } = null!;
    }
}
