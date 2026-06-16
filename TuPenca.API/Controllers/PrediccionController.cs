using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TuPenca.Application.DTOs.Prediccion;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "UsuarioComun")]
    public class PrediccionController : ControllerBase
    {
        private readonly IPrediccionService _prediccionService;

        public PrediccionController(IPrediccionService prediccionService)
        {
            _prediccionService = prediccionService;
        }

        [HttpPost]
        public async Task<IActionResult> CrearOModificar([FromBody] PrediccionRequestDto dto)
        {
            try
            {
                var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var response = await _prediccionService.CrearOModificarAsync(dto, usuarioId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{pencaId}")]
        public async Task<IActionResult> ObtenerMisPredicciones(Guid pencaId)
        {
            try
            {
                var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var response = await _prediccionService.ObtenerMisPrediccionesAsync(usuarioId, pencaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("{pencaId}/historial")]
        public async Task<IActionResult> ObtenerHistorial(Guid pencaId)
        {
            try
            {
                var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var response = await _prediccionService.ObtenerHistorialAsync(usuarioId, pencaId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}