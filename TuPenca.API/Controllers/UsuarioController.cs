using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuPenca.Application.DTOs.Usuario;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Infrastructure.Interfaces.Providers;

namespace TuPenca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ISitioProvider _sitioProvider;

        public UsuarioController(IUsuarioService usuarioService, ISitioProvider sitioProvider)
        {
            _usuarioService = usuarioService;
            _sitioProvider = sitioProvider;
        }

        [HttpGet("obtener/todos")]
        [Authorize(Roles = "AdministradorSitio")]
        public async Task<IActionResult> ObtenerUsuariosAsync()
        {
            try
            {
                var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                if (sitioIdClaim == null || Guid.Parse(sitioIdClaim) != _sitioProvider.GetSitioId())
                    return Forbid();

                var response = await _usuarioService.ObtenerUsuariosAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("obtener/{usuarioId}")]
        [Authorize(Roles = "AdministradorSitio,UsuarioComun")]
        public async Task<IActionResult> ObtenerUsuarioAsync(Guid usuarioId)
        {
            try
            {
                var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                if (sitioIdClaim == null || Guid.Parse(sitioIdClaim) != _sitioProvider.GetSitioId())
                    return Forbid();

                var response = await _usuarioService.ObtenerUsuarioAsync(usuarioId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("actualizar")]
        [Authorize(Roles = "AdministradorSitio,UsuarioComun")]
        public async Task<IActionResult> ActualizarUsuarioAsync([FromBody] UsuarioRequestDto request)
        {
            try
            {
                var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                if (sitioIdClaim == null || Guid.Parse(sitioIdClaim) != _sitioProvider.GetSitioId())
                    return Forbid();

                var response = await _usuarioService.ActualizarUsuarioAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("actualizar/password")]
        [Authorize(Roles = "AdministradorSitio,UsuarioComun")]
        public async Task<IActionResult> ActualizarPasswordAsync([FromBody] UsuarioActualizarPasswordRequestDto request)
        {
            try
            {
                var sitioIdClaim = User.FindFirst("sitioId")?.Value;
                if (sitioIdClaim == null || Guid.Parse(sitioIdClaim) != _sitioProvider.GetSitioId())
                    return Forbid();

                var response = await _usuarioService.ActualizarPasswordAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Registramos el token FCM del celular del usuario para mandarle notificaciones push
        [HttpPost("registrar/fcm-token")]
        [Authorize(Roles = "UsuarioComun")]
        public async Task<IActionResult> RegistrarFcmTokenAsync([FromBody] RegistrarFcmTokenRequestDto request)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst("sub")?.Value;
                if (usuarioIdClaim == null)
                    return Unauthorized();

                var usuarioId = Guid.Parse(usuarioIdClaim);
                await _usuarioService.RegistrarFcmTokenAsync(usuarioId, request.FcmToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
