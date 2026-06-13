using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TuPenca.Application.Interfaces;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, HttpClient httpClient, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            var apiKey = _configuration["Resend:ApiKey"];
            var fromEmail = _configuration["Resend:FromEmail"] ?? "Tu Penca UY <admin@tupencauy.lat>";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Resend:ApiKey no está configurado en appsettings");
                throw new InvalidOperationException("Falta configurar Resend:ApiKey");
            }

            _logger.LogInformation("Enviando email a {Destinatario} con asunto '{Asunto}'", destinatario, asunto);
            _logger.LogDebug("Usando from: {From}", fromEmail);

            var payload = new
            {
                from = fromEmail,
                to = new[] { destinatario },
                subject = asunto,
                html = cuerpo
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error de red al contactar Resend API");
                throw;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Resend devolvió {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    responseBody
                );
                throw new Exception($"Resend devolvió {(int)response.StatusCode}: {responseBody}");
            }

            _logger.LogInformation(
                "Email enviado exitosamente a {Destinatario}. Respuesta: {Body}",
                destinatario,
                responseBody
            );
        }
    }
}