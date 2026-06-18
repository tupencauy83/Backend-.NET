using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TuPenca.Application.DTOs.Pago;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "UsuarioComun")]
    public class PagoController : ControllerBase
    {
        private readonly IPagoService _pagoService;

        public PagoController(IPagoService pagoService)
        {
            _pagoService = pagoService;
        }

        [HttpPost("realizar")]
        public async Task<IActionResult> RealizarPago([FromBody] PagoRequestDto dto)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null)
                    return Unauthorized();

                var usuarioId = Guid.Parse(usuarioIdClaim);
                var response = await _pagoService.RealizarPagoAsync(dto, usuarioId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("mis-inscripciones")]
        public async Task<IActionResult> ObtenerInscripciones()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null)
                return Unauthorized();

            var usuarioId = Guid.Parse(usuarioIdClaim);
            var response = await _pagoService.ObtenerInscripcionesAsync(usuarioId);
            return Ok(response);
        }

    }
}