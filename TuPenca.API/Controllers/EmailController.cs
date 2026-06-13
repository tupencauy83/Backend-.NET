using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TuPenca.Application.Interfaces;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(IEmailService emailService, IConfiguration configuration, ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de prueba — GET /api/emailtest/ping
        /// Verifica que la config de Resend esté presente sin enviar nada
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            var apiKey = _configuration["Resend:ApiKey"];
            var fromEmail = _configuration["Resend:FromEmail"];

            _logger.LogInformation("EmailTest ping ejecutado");

            return Ok(new
            {
                apiKeyPresente = !string.IsNullOrWhiteSpace(apiKey),
                // Never log/return the actual key
                apiKeyPrimeros4 = apiKey?.Length >= 4 ? apiKey[..4] + "..." : "(vacío)",
                fromEmail = fromEmail ?? "(no configurado, usará default)",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Envía un email de prueba — POST /api/emailtest/send
        /// Body: { "destinatario": "test@example.com" }
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Destinatario))
                return BadRequest(new { error = "El campo 'destinatario' es requerido" });

            _logger.LogInformation("Iniciando test de envío a {Destinatario}", request.Destinatario);

            try
            {
                await _emailService.EnviarAsync(
                    destinatario: request.Destinatario,
                    asunto: "✅ Test TuPenca — Email funcionando",
                    cuerpo: """
                        <h2>¡Funciona!</h2>
                        <p>Este es un email de prueba enviado desde <strong>TuPenca</strong>.</p>
                        <p>Si ves esto, Resend está configurado correctamente.</p>
                    """
                );

                return Ok(new { mensaje = "Email enviado correctamente", destinatario = request.Destinatario });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error de configuración");
                return StatusCode(500, new { error = "Error de configuración", detalle = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fallo al enviar email de prueba");
                return StatusCode(500, new { error = "Fallo al enviar", detalle = ex.Message });
            }
        }
    }

    public record TestEmailRequest(string Destinatario);
}