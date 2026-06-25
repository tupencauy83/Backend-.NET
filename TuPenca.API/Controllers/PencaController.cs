using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TuPenca.Application.DTOs.Penca;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Enums;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PencaController : ControllerBase
    {
        private readonly IPencaService _pencaService;

        public PencaController(IPencaService pencaService)
        {
            _pencaService = pencaService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObtenerTodas()
        {
            var response = await _pencaService.ObtenerTodasAsync();
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "AdministradorSitio,UsuarioComun")]
        public async Task<IActionResult> ObtenerPorId(Guid id)
        {
            var response = await _pencaService.ObtenerPorIdAsync(id);
            if (response == null) return NotFound("Penca no encontrada");
            return Ok(response);
        }

        [HttpPost("crear")]
        [Authorize(Roles = "AdministradorSitio")]
        public async Task<IActionResult> Crear([FromBody] PencaRequestDto dto)
        {
            try
            {
                var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                if (sitioIdClaim == null)
                    return Unauthorized("No se pudo determinar el sitio");

                var sitioId = Guid.Parse(sitioIdClaim);
                var response = await _pencaService.CrearAsync(dto, sitioId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/estado")]
        [Authorize(Roles = "AdministradorSitio")]
        public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] EstadoPenca nuevoEstado)
        {
            try
            {
                var response = await _pencaService.CambiarEstadoAsync(id, nuevoEstado);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "AdministradorSitio")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            try
            {
                await _pencaService.EliminarAsync(id);
                return Ok("Penca eliminada exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Tabla posicion

        [HttpGet("{pencaId}/tabla-posiciones")]
        [Authorize(Roles = "UsuarioComun,AdministradorSitio,AdministradorPlataforma")]
        public async Task<IActionResult> ObtenerTablaPosiciones(Guid pencaId)
        {
            try
            {
                var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var rol = User.FindFirst(ClaimTypes.Role)!.Value;
                var response = await _pencaService.ObtenerTablaPosicionesAsync(pencaId, usuarioId, rol);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Ganadores

        [HttpGet("{pencaId}/ganadores")]
        [Authorize(Roles = "UsuarioComun,AdministradorSitio,AdministradorPlataforma")]
        public async Task<IActionResult> ObtenerGanadores(Guid pencaId)
        {
            try
            {
                var response = await _pencaService.ObtenerGanadoresAsync(pencaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // ACTUALIZAR % PREMIOS DE GANADORES

        [HttpPut("{id}/premios")]
        [Authorize(Roles = "AdministradorSitio")]
        public async Task<IActionResult> EditarPremios(Guid id, [FromBody] PencaEditPremioDto dto)
        {
            try
            { 
                var response = await _pencaService.EditarPremiosAsync(id,dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{pencaId}/tiempo-limite-prediccion")]
        [Authorize]
        public async Task<IActionResult> ObtenerTiempoLimitePrediccion(Guid pencaId)
        {
            try
            {
                var response = await _pencaService.ObtenerTiempoLimitePrediccionAsync(pencaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}