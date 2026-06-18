using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstadisticasController : ControllerBase
    {
        private readonly IEstadisticasService _estadisticasService;

        public EstadisticasController(IEstadisticasService estadisticasService)
        {
            _estadisticasService = estadisticasService;
        }

        [HttpGet("global")]
        [Authorize(Roles = "AdministradorPlataforma")]
        public async Task<IActionResult> ObtenerGlobales(
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] Domain.Enums.EstadoSitio? estadoSitio,
            [FromQuery] string? buscar)
        {
            try
            {
                var filtro = new Application.DTOs.Estadisticas.EstadisticasGlobalesFiltroDto
                {
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    EstadoSitio = estadoSitio,
                    Buscar = buscar
                };

                var response = await _estadisticasService.ObtenerGlobalesAsync(filtro);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("sitio/{sitioId}")]
        [Authorize(Roles = "AdministradorSitio,AdministradorPlataforma")]
        public async Task<IActionResult> ObtenerPorSitio(
            Guid sitioId,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] Domain.Enums.EstadoPenca? estadoPenca,
            [FromQuery] string? buscar)
        {
            try
            {
                var rol = User.FindFirst(ClaimTypes.Role)!.Value;

                if (rol == "AdministradorSitio")
                {
                    var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                    if (sitioIdClaim == null || Guid.Parse(sitioIdClaim) != sitioId)
                        return Forbid();
                }

                var filtro = new Application.DTOs.Estadisticas.EstadisticasSitioFiltroDto
                {
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    EstadoPenca = estadoPenca,
                    Buscar = buscar
                };

                var response = await _estadisticasService.ObtenerPorSitioAsync(sitioId, filtro);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}