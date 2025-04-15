using BlogProject.Business.Abstract;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BlogProject.Business.Concrete
{
    public class EmailManager : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlMessage)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var smtpClient = new SmtpClient
            {
                Host = settings["Host"],
                Port = int.Parse(settings["Port"]),
                EnableSsl = bool.Parse(settings["EnableSSL"]),
                Credentials = new NetworkCredential(settings["Username"], settings["Password"])
            };

            var message = new MailMessage
            {
                From = new MailAddress(settings["Username"], "Devritory"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtpClient.SendMailAsync(message);
        }
    }
}
