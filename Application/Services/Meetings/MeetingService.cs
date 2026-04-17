using SyncVerse.Domain.Entities;
using SyncVerse.Application.DTOs.Meetings;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Application.Interfaces.Meetings;
using Microsoft.EntityFrameworkCore;

namespace SyncVerse.Application.Services.Meetings
{
    public class MeetingService : IMeetingService
    {
        private readonly IUnitOfWork _unitOfWork;
        public MeetingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<MeetingResponseDto>> CreateAsync(CreateMeetingDto dto)
        {
            var meeting = new Meeting
            {
                OrgCode = dto.OrgCode,
                RoomId = dto.RoomId,
                VivoxChannelName = dto.VivoxChannelName,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<Meeting>().AddAsync(meeting);
            await _unitOfWork.SaveChangesAsync();
            return Result<MeetingResponseDto>.Success(new MeetingResponseDto
            {
                Id = meeting.Id,
                OrgCode = meeting.OrgCode,
                RoomId = meeting.RoomId,
                VivoxChannelName = meeting.VivoxChannelName,
                CreatedAt = meeting.CreatedAt
            });
        }

        public async Task<List<MeetingResponseDto>> GetActiveMeetings(string orgCode)
        {
            var meetings = await _unitOfWork.Repository<Meeting>()
                .Query()
                .Where(m => m.OrgCode == orgCode)
                .ToListAsync();
            return meetings.Select(m => new MeetingResponseDto
            {
                Id = m.Id,
                OrgCode = m.OrgCode,
                RoomId = m.RoomId,
                VivoxChannelName = m.VivoxChannelName,
                CreatedAt = m.CreatedAt
            }).ToList();
        }

        public async Task<bool> DeleteMeeting(string roomId)
        {
            var meeting = await _unitOfWork.Repository<Meeting>()
                .Query()
                .FirstOrDefaultAsync(m => m.RoomId == roomId);
            if (meeting == null) return false;
            _unitOfWork.Repository<Meeting>().Delete(meeting);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
