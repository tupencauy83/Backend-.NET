using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var sitio = await _sitioService.ObtenerSitioAsync(sitioId);

            if (sitio == null)
                return NotFound();

            if (sitio.Estado != Domain.Enums.EstadoSitio.Activo)
                return StatusCode(StatusCodes.Status403Forbidden, "Este sitio fue desactivado o no está disponible.");

            var result = new
            {
                sitio.ColorPrimario,
                sitio.ColorSecundario,
                sitio.Logo,
                sitio.ConfiguracionSitio,
                sitio.TipoRegistro,
            };

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
            catch (DbUpdateException ex)
            {
                return BadRequest(new { mensaje = TraducirErrorDuplicado(ex) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        private static string TraducirErrorDuplicado(DbUpdateException ex)
        {
            var detalle = ex.InnerException?.Message ?? ex.Message;

            if (detalle.Contains("Email", StringComparison.OrdinalIgnoreCase) ||
                detalle.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                detalle.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                detalle.Contains("IX_Usuarios", StringComparison.OrdinalIgnoreCase))
            {
                return "Ese email ya está en uso en este sitio.";
            }

            if (detalle.Contains("UrlPropia", StringComparison.OrdinalIgnoreCase) ||
                detalle.Contains("IX_Sitios", StringComparison.OrdinalIgnoreCase))
            {
                return "Esa dirección ya está en uso. Elegí otro nombre.";
            }

            return detalle;
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

        [HttpPatch("tipo-registro")]
        [Authorize(Roles = "AdministradorPlataforma, AdministradorSitio")]
        public async Task<IActionResult> CambiarTipoRegistro([FromBody] CambiarTipoRegistroDto dto)
        {
            try
            {
                var resultado = await _sitioService.CambiarTipoRegistroAsync(dto);
                if (resultado.Id == Guid.Empty)
                    return NotFound(new { mensaje = resultado.Mensaje });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

    }
}
