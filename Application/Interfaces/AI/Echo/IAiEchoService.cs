using SyncVerse.Application.Common.Results;
using SyncVerse.Application.DTOs.AI.Echo;

namespace SyncVerse.Application.Interfaces.AI.Echo
{
    public interface IAiEchoService
    {
        Task<Result<EchoChatResponseDto>> TalkToEchoAsync(EchoChatRequestDto dto);
    }
}