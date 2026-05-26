using Microsoft.Extensions.Configuration;
using TuPenca.Application.Interfaces.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace TuPenca.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task EnviarAsync(string destinatario, string asunto, string cuerpo)
        {
            var apiKey = _configuration["Resend:ApiKey"];
            var fromEmail = _configuration["Resend:FromEmail"] ?? "Tu Penca UY <onboarding@resend.dev>";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Falta configurar Resend:ApiKey");

            var payload = new
            {
                from = fromEmail,
                to = new[] { destinatario },
                subject = asunto,
                html = cuerpo
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Resend devolvió {(int)response.StatusCode}: {error}");
            }
        }
    }
}
