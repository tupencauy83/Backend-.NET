using System.Net;
using System.Net.Mail;

namespace TuPenca.Infrastructure.Services
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpo);
    }

    public class EmailService : IEmailService
    {
        private const string RemitenteMail = "uytupenca@gmail.com";
        private const string AppPassword = "bolvuytjqovwmfnz";
        private const string NombreRemitente = "TuPenca";

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            var mensaje = new MailMessage
            {
                From = new MailAddress(RemitenteMail, NombreRemitente),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };
            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(RemitenteMail, AppPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mensaje);
        }
    }
}
