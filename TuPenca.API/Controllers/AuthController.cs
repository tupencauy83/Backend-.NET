using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuPenca.Application.DTOs.Auth;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Infrastructure.Interfaces.Providers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ISitioProvider _sitioProvider;

    public AuthController(IAuthService authService, ISitioProvider sitioProvider)
    {
        _authService = authService;
        _sitioProvider = sitioProvider;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email y contraseña son requeridos");

        try
        {
            Guid? sitioId = _sitioProvider.GetSitioId();

            var response = await _authService.LoginAsync(request, sitioId);

            if (response == null)
                return Unauthorized("Credenciales incorrectas");

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("firebase")]
    public async Task<IActionResult> FirebaseLogin([FromBody] FirebaseLoginRequest request)
    {
        try
        {
            Guid? sitioId = _sitioProvider.GetSitioId();

            var response = await _authService.LoginFirebaseAsync(request.IdToken, sitioId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("registro/usuario")]
    public async Task<IActionResult> RegistrarUsuario([FromBody] RegistroUsuarioRequestDto request)
    {
        try
        {
            Guid? sitioId = _sitioProvider.GetSitioId();
            var response = await _authService.RegistrarUsuarioAsync(request, sitioId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("registro/administrador")]
    [Authorize(Roles = "AdministradorPlataforma")]
    public async Task<IActionResult> RegistrarAdmin([FromBody] RegistroAdminRequestDto request)
    {
        try
        {
            var response = await _authService.RegistrarAdminAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    // INVITACIONES - ENDPOINTS

    [HttpGet("usuarios/pendientes")]
    [Authorize(Roles = "AdministradorSitio")]
    public async Task<IActionResult> ObtenerUsuariosPendientes()
    {
        try
        {
            Guid? sitioId = _sitioProvider.GetSitioId();
            if (sitioId == null)
                return Unauthorized("No se pudo determinar el sitio");

            var response = await _authService.ObtenerUsuariosPendientesAsync(sitioId.Value);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("usuarios/{usuarioId}/aprobar")]
    [Authorize(Roles = "AdministradorSitio")]
    public async Task<IActionResult> AprobarUsuario(Guid usuarioId)
    {
        try
        {
            var response = await _authService.AprobarUsuarioAsync(usuarioId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("usuarios/{usuarioId}/rechazar")]
    [Authorize(Roles = "AdministradorSitio")]
    public async Task<IActionResult> RechazarUsuario(Guid usuarioId)
    {
        try
        {
            var response = await _authService.RechazarUsuarioAsync(usuarioId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    //////////////////////////////////////

    // ⚠️ TEMPORAL — eliminar después de crear el primer admin
    [HttpPost("setup/primer-admin")]
    public async Task<IActionResult> CrearPrimerAdmin([FromBody] RegistroAdminRequestDto request)
    {
        try
        {
            var response = await _authService.RegistrarAdminAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}