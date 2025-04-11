using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Comp2139_Assignment1.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailSender(IConfiguration configuration)
        {
            _smtpServer = configuration["Mailtrap:SmtpServer"];
            _smtpPort = int.Parse(configuration["Mailtrap:SmtpPort"]);
            _smtpUsername = configuration["Mailtrap:Username"];
            _smtpPassword = configuration["Mailtrap:Password"];

            Console.WriteLine($"Mailtrap SMTP Server: {_smtpServer}");
            Console.WriteLine($"Mailtrap SMTP Username: {_smtpUsername}");

            if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                throw new Exception("Mailtrap SMTP configuration is missing.");
            }
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var fromAddress = new MailAddress("danielquach5@gmail.com", "PM Tool Inc.");
                var toAddress = new MailAddress(email);
                var smtp = new SmtpClient
                {
                    Host = _smtpServer,
                    Port = _smtpPort,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                })
                {
                    await smtp.SendMailAsync(message);
                    Console.WriteLine("Email sent successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }
    }
}
