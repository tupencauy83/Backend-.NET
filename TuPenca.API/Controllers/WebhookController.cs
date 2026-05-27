using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IPagoService pagoService, ILogger<WebhookController> logger)
        {
            _pagoService = pagoService;
            _logger = logger;
        }

        [HttpPost("mercadopago")]
        [AllowAnonymous] // 👈 MP no manda token, debe ser público
        public async Task<IActionResult> MercadoPagoWebhook()
        {
            try
            {
                // MP envía el id del pago como query param o en el body
                // Formato típico: ?id=123&topic=payment
                var topic = Request.Query["topic"].ToString();
                var id = Request.Query["id"].ToString();

                // También puede venir como JSON en el body (notificaciones v2)
                if (string.IsNullOrEmpty(id))
                {
                    using var reader = new StreamReader(Request.Body);
                    var body = await reader.ReadToEndAsync();
                    var json = JsonDocument.Parse(body);

                    if (json.RootElement.TryGetProperty("data", out var data) &&
                        data.TryGetProperty("id", out var dataId))
                    {
                        id = dataId.GetRawText().Trim('"');
                        topic = "payment";
                    }
                }

                _logger.LogInformation("Webhook MP recibido - topic: {topic}, id: {id}", topic, id);

                // Solo procesar notificaciones de pagos
                if (topic == "payment" && !string.IsNullOrEmpty(id))
                {
                    await _pagoService.ProcesarWebhookAsync(id);
                }

                // MP espera 200 OK, si no reintenta
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando webhook de MercadoPago");
                // Igual devolvemos 200 para que MP no reintente en errores nuestros
                return Ok();
            }
        }
    }
}