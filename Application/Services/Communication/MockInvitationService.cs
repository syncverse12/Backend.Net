public class MockInvitationService : IInvitationService
{
    public Task SendInvitationAsync(string to, string projectName)
    {
        Console.WriteLine($"Invitation sent to {to} for project {projectName}");
        return Task.CompletedTask;
    }
}
