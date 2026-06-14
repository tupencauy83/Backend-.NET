using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuPenca.Application.DTOs.ParametrosSitio;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Infrastructure.Interfaces.Providers;

namespace TuPenca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "AdministradorSitio")]
    public class ParametrosSitioController : ControllerBase
    {
        private readonly IParametrosSitioService _service;
        private readonly ISitioProvider _sitioProvider;

        public ParametrosSitioController(IParametrosSitioService service, ISitioProvider sitioProvider)
        {
            _service = service;
            _sitioProvider = sitioProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            try
            {
                var sitioId = _sitioProvider.GetSitioId();
                if (sitioId == null)
                    return Unauthorized("No se pudo determinar el sitio");

                var result = await _service.ObtenerAsync(sitioId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPatch]
        public async Task<IActionResult> Actualizar([FromBody] ActualizarParametrosSitioDto dto)
        {
            try
            {
                var sitioId = _sitioProvider.GetSitioId();
                if (sitioId == null)
                    return Unauthorized("No se pudo determinar el sitio");

                var result = await _service.ActualizarAsync(sitioId.Value, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}