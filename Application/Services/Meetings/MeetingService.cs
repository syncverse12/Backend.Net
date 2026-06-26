using Microsoft.EntityFrameworkCore;
using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Meeting;
using SyncVerse.Application.DTOs.Meetings;
using SyncVerse.Application.Interfaces.Meetings;
using SyncVerse.Application.Interfaces.Persistence;
using SyncVerse.Domain.Entities;

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

        public async Task<Result<bool>> SaveAiSummaryAsync(string meetingId, AiMeetingSummaryResponseDto aiDto)
        {
            var meeting = await _unitOfWork.Repository<Meeting>()
                .Query()
                .FirstOrDefaultAsync(m => m.Id == meetingId);

            if (meeting == null)
                return Result<bool>.Failure("Meeting not found");

            meeting.Summary = aiDto.Summary;

            meeting.KeyPoints = aiDto.KeyPoints != null ? string.Join("\n", aiDto.KeyPoints) : null;
            meeting.Decisions = aiDto.Decisions != null ? string.Join("\n", aiDto.Decisions) : null;

            _unitOfWork.Repository<Meeting>().Update(meeting);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true, "Meeting summary saved successfully to Database.");
        }
    }
}
