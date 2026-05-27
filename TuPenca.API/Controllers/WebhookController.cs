using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        private readonly IConfiguration _config;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IPagoService pagoService, IConfiguration config, ILogger<WebhookController> logger)
        {
            _pagoService = pagoService;
            _config = config;
            _logger = logger;
        }

        [HttpPost("stripe")]
        [AllowAnonymous]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var webhookSecret = _config["Stripe:WebhookSecret"];

            try
            {
                // Stripe firma cada evento, esto verifica que sea legítimo
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    if (session == null) return Ok();

                    var pagoId = session.Metadata["pagoId"];
                    var estado = session.PaymentStatus == "paid" ? "approved" : "rejected";

                    await _pagoService.ProcesarWebhookAsync(pagoId, estado);
                    _logger.LogInformation("Pago {pagoId} actualizado a {estado}", pagoId, estado);
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Error verificando webhook de Stripe");
                return BadRequest();
            }
        }
    }
}