using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TuPenca.Application.DTOs.Sitio;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Infrastructure.Interfaces.Providers;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SitioController : ControllerBase
    {
        private readonly ISitioService _sitioService;
        private readonly ISitioProvider _sitioProvider;

        public SitioController(ISitioService sitioService, ISitioProvider sitioProvider)
        {
            _sitioService = sitioService;
            _sitioProvider = sitioProvider;
        }

        [HttpGet("publicos")]
        public async Task<IActionResult> ObtenerSitiosPublicosAsync()
        {
            try
            {
                var sitios = await _sitioService.ObtenerSitiosAsync();
                var activos = sitios
                    .Where(s => s.Estado == Domain.Enums.EstadoSitio.Activo)
                    .Select(s => new { s.Id, s.Nombre, s.UrlPropia })
                    .ToList();
                return Ok(activos);
            }
            catch (Exception ex)
            {
                // EF suele envolver el error real (ej: unique index) en InnerException.
                return BadRequest(ex.InnerException?.Message ?? ex.Message);
            }
        }

        [HttpGet("obtener/todos")]
        [Authorize(Roles = "AdministradorPlataforma")]
        public async Task<IActionResult> ObtenerSitiosAsync()
        {
            try
            {
                var response = await _sitioService.ObtenerSitiosAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("obtener/{sitioId}")]
        [Authorize(Roles = "AdministradorSitio,AdministradorPlataforma")] 
        public async Task<IActionResult> ObtenerSitioAsync(Guid sitioId)
        {
            try
            {
                var response = await _sitioService.ObtenerSitioAsync(sitioId);
                if (response == null)
                    return NotFound("Sitio no encontrado");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("actual/{sitioId}")]
        public async Task<IActionResult> ObtenerColoresYLogoSitioActualAsync(Guid sitioId)
        {
            var sitios = await _sitioService.ObtenerSitiosAsync();

            var result = sitios.Select(s => new
            {
                s.ColorPrimario,
                s.ColorSecundario,
                s.Logo
            });

            return Ok(result);
        }

        [HttpGet("obtener/pendientes")]
        [Authorize(Roles = "AdministradorPlataforma")]
        public async Task<IActionResult> ObtenerSitiosPendientesAsync()
        {
            try
            {
                var response = await _sitioService.ObtenerSitiosPendientesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarSitioAsync([FromBody] SitioPendienteRequestDto solicitarSitioDto)
        {
            try
            {
                var response = await _sitioService.SolicitarSitioAsync(solicitarSitioDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("actualizar/estado")]
        [Authorize(Roles = "AdministradorPlataforma")]
        public async Task<IActionResult> ActualizarEstadoSitioAsync([FromBody] SitioActualizarEstadoRequest sitioDto)
        {
            try
            {
                var response = sitioDto.Estado == Domain.Enums.EstadoSitio.Activo
                    ? await _sitioService.AprobarSitioAsync(sitioDto.Id)
                    : await _sitioService.RechazarSitioAsync(sitioDto.Id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("crear")]
        [Authorize(Roles = "AdministradorPlataforma")]
        public async Task<IActionResult> CrearSitioAsync([FromBody] SitioRequestDto sitioDto)
        {
            try
            {
                var response = await _sitioService.CrearSitioAsync(sitioDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("actualizar")]
        [Authorize(Roles = "AdministradorPlataforma, AdministradorSitio")]
        public async Task<IActionResult> ActualizarSitioAsync([FromBody] SitioRequestDto sitioDto)
        {
            try
            {
                var rol = User.FindFirst(ClaimTypes.Role)!.Value;

                if (rol == "AdministradorSitio")
                {
                    var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                    if (sitioIdClaim == null || Guid.Parse(sitioIdClaim) != sitioDto.Id)
                        return Forbid();
                }

                var response = await _sitioService.ActualizarSitioAsync(sitioDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("eliminar/{sitioId}")]
        [Authorize(Roles = "AdministradorPlataforma")]
        public async Task<IActionResult> EliminarSitioAsync(Guid sitioId)
        {
            try
            {
                var response = await _sitioService.EliminarSitioAsync(sitioId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
