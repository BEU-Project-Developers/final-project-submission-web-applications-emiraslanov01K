using System.Threading.Tasks;

namespace CaterManagementSystem.Services // Namespace-i layihənizə uyğunlaşdırın
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}