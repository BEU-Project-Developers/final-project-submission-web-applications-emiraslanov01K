// Services/MailSettings.cs
namespace CaterManagementSystem.Services
{
    public class MailSettings
    {
        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; } // Email üçün username
        public string? SmtpPassword { get; set; } // Email üçün app password 
        public string? FromName { get; set; }     // Göndərən
        public string? FromAddress { get; set; }  // Hansı emaildən gəldiyi
        public bool UseSsl { get; set; }          // SSL/TLS usage true or not 
}