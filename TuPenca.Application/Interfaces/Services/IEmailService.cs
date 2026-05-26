namespace TuPenca.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task EnviarAsync(string destinatario, string asunto, string cuerpo);
    }
}
