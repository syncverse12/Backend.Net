public interface IInvitationService
{
    Task SendInvitationAsync(string to, string projectName);
}
