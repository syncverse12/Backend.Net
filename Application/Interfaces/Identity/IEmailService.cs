namespace Graduation_Project.Application.Interfaces.Identity
{
    public interface IEmailService
    {
        System.Threading.Tasks.Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
