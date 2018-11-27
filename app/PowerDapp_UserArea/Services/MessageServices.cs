using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PowerDapp_UserArea.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return SendMail(email, subject, message);
        }

        private Task SendMail(string email, string subject, string message)
        {
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.EnableSsl = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Host = "mail.u2cms.cz";
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("info@u2cms.cz", "U2CMS");

            MailMessage mail = new MailMessage();
            mail.Subject = subject;
            mail.To.Add(new MailAddress(email));
            mail.Body = message;
            mail.IsBodyHtml = true;
            mail.From = new MailAddress("info@PowerDapp.io");

            return client.SendMailAsync(mail);
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
