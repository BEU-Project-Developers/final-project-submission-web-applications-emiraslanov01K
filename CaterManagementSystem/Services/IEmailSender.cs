using System.Threading.Tasks;

namespace CaterManagementSystem.Services 
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}