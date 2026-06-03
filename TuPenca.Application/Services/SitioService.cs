using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TuPenca.Application.DTOs.Sitio;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces;
using TuPenca.Domain.Enums;

namespace TuPenca.Application.Services
{
    public class SitioService : ISitioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<SitioService> _logger;
        private readonly PasswordHasher<string> _hasher = new();

        public SitioService(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<SitioService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<SitioDto>> ObtenerSitiosAsync()
        {
            var sitios = await _unitOfWork.Sitios.GetAllAsync();
            var result = new List<SitioDto>();
            foreach (var sitio in sitios)
            {
                result.Add(new SitioDto()
                {
                    Id = sitio.Id,
                    Nombre = sitio.Nombre,
                    UrlPropia = sitio.UrlPropia,
                    ConfiguracionSitio = sitio.ConfiguracionSitio,
                    ColorPrimario = sitio.ColorPrimario,
                    ColorSecundario = sitio.ColorSecundario,
                    TipoRegistro = sitio.TipoRegistro,
                    Estado = sitio.Estado
                });
            }
            return result;
        }

        public async Task<SitioDto?> ObtenerSitioAsync(Guid sitioId)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId);
            if (sitio == null) return null;

            return new SitioDto
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                UrlPropia = sitio.UrlPropia,
                ConfiguracionSitio = sitio.ConfiguracionSitio,
                ColorPrimario = sitio.ColorPrimario,
                ColorSecundario = sitio.ColorSecundario,
                TipoRegistro = sitio.TipoRegistro
            };
        }

        public async Task<IEnumerable<SitioDto>> ObtenerSitiosPendientesAsync()
        {
            var sitios = await _unitOfWork.Sitios.GetAllAsync();
            sitios = sitios.Where(s => s.Estado == Domain.Enums.EstadoSitio.Pendiente);
            var result = new List<SitioDto>();
            foreach (var sitio in sitios)
            {
                result.Add(new SitioDto()
                {
                    Id = sitio.Id,
                    Nombre = sitio.Nombre,
                    UrlPropia = sitio.UrlPropia,
                    ConfiguracionSitio = sitio.ConfiguracionSitio,
                    ColorPrimario = sitio.ColorPrimario,
                    ColorSecundario = sitio.ColorSecundario,
                    TipoRegistro = sitio.TipoRegistro
                });
            }
            return result;
        }

        public async Task<SitioResponseDto> SolicitarSitioAsync(SitioPendienteRequestDto sitioDto)
        {
            var url = NormalizarHost(sitioDto.UrlPropia);
            if (string.IsNullOrWhiteSpace(url))
                throw new Exception("El dominio del sitio es obligatorio");

            // UrlPropia es único. Chequeamos antes para devolver un mensaje claro.
            var existentes = await _unitOfWork.Sitios.GetAllAsync();
            if (existentes.Any(s => string.Equals(NormalizarHost(s.UrlPropia), url, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Ese dominio ya está en uso. Elegí otro.");

            var sitio = new Sitio()
            {
                Id = Guid.NewGuid(),
                Nombre = sitioDto.Nombre,
                UrlPropia = url,
                ConfiguracionSitio = sitioDto.ConfiguracionSitio,
                ColorPrimario = sitioDto.ColorPrimario,
                ColorSecundario = sitioDto.ColorSecundario,
                TipoRegistro = sitioDto.TipoRegistro,
                Logo = sitioDto.Logo
            };

            await _unitOfWork.Sitios.AddAsync(sitio);

            var usuarioAdmin = new Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = sitioDto.NombreUsuario,
                Email = sitioDto.Email,
                PasswordHash = "",
                Rol = RolUsuario.AdministradorSitio,
                FechaRegistro = DateTime.UtcNow,
                Estado = EstadoUsuario.Pendiente,
                ProveedorAuth = ProveedorAuth.Local,
                SitioId = sitio.Id
            };

            await _unitOfWork.Usuarios.AddAsync(usuarioAdmin);
            await _unitOfWork.SaveChangesAsync();

            return new SitioResponseDto()
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Solicitud de sitio creada exitosamente."
            };
        }

        private static string NormalizarHost(string? host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return string.Empty;

            var valor = host.Trim().ToLowerInvariant();

            if (Uri.TryCreate(valor, UriKind.Absolute, out var uri))
                valor = uri.Host;
            else if (Uri.TryCreate($"https://{valor}", UriKind.Absolute, out var uriConEsquema))
                valor = uriConEsquema.Host;

            if (valor.StartsWith("www."))
                valor = valor[4..];

            return valor;
        }

        public async Task<SitioResponseDto> ActualizarSitioPendienteAsync(SitioActualizarEstadoRequest sitioDto)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioDto.Id);
            if (sitio == null)
                return new SitioResponseDto { Mensaje = "Sitio no encontrado" };

            sitio.Estado = sitioDto.Estado;

            await _unitOfWork.Sitios.UpdateAsync(sitio);
            await _unitOfWork.SaveChangesAsync();

            return new SitioResponseDto
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Sitio actualizado exitosamente"
            };
        }

        public async Task<SitioResponseDto> AprobarSitioAsync(Guid sitioId)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId);
            if (sitio == null)
                return new SitioResponseDto { Mensaje = "Sitio no encontrado" };

            sitio.Estado = EstadoSitio.Activo;

            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.SitioId == sitioId);
            string? password = null;

            if (usuario != null)
            {
                usuario.Estado = EstadoUsuario.Aprobado;
                password = Guid.NewGuid().ToString()[..8];
                usuario.PasswordHash = _hasher.HashPassword(null!, password);
                await _unitOfWork.Usuarios.UpdateAsync(usuario);
            }

            await _unitOfWork.Sitios.UpdateAsync(sitio);
            await _unitOfWork.SaveChangesAsync();

            if (usuario != null && password != null)
            {
                try
                {
                    await _emailService.EnviarAsync(
                        usuario.Email,
                        "Bienvenido a TuPenca - Credenciales de tu sitio",
                        $@"<h2>¡Tu sitio fue aprobado!</h2>
                        <p>Hola {usuario.Nombre},</p>
                        <p>Tu sitio <strong>{sitio.Nombre}</strong> fue aprobado exitosamente.</p>
                        <p>Tus credenciales de acceso como administrador del sitio:</p>
                        <ul>
                            <li><strong>Email:</strong> {usuario.Email}</li>
                            <li><strong>Contraseña:</strong> {password}</li>
                        </ul>
                        <p>Te recomendamos cambiar la contraseña después del primer inicio de sesión.</p>
                        <p>— Equipo TuPenca</p>"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando credenciales por email para el sitio {SitioId} a {Email}", sitioId, usuario.Email);
                }
            }

            return new SitioResponseDto
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Sitio aprobado exitosamente"
            };
        }

        public async Task<SitioResponseDto> RechazarSitioAsync(Guid sitioId)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId);
            if (sitio == null)
                return new SitioResponseDto { Mensaje = "Sitio no encontrado" };

            sitio.Estado = EstadoSitio.Inactivo;

            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuario = usuarios.FirstOrDefault(u => u.SitioId == sitioId);

            if (usuario != null)
            {
                usuario.Estado = EstadoUsuario.Rechazado;
                await _unitOfWork.Usuarios.UpdateAsync(usuario);
            }

            await _unitOfWork.Sitios.UpdateAsync(sitio);
            await _unitOfWork.SaveChangesAsync();

            if (usuario != null)
            {
                try
                {
                    await _emailService.EnviarAsync(
                        usuario.Email,
                        "Tu sitio fue rechazado",
                        $@"<h2>Solicitud de sitio rechazada</h2>
                        <p>Hola {usuario.Nombre},</p>
                        <p>Lamentamos informarte que tu solicitud para el sitio <strong>{sitio.Nombre}</strong> fue rechazada.</p>
                        <p>Si creés que fue un error, podés volver a comunicarte con el equipo de TuPenca.</p>
                        <p>— Equipo TuPenca</p>"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando rechazo por email para el sitio {SitioId} a {Email}", sitioId, usuario.Email);
                }
            }

            return new SitioResponseDto
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Sitio rechazado exitosamente"
            };
        }

        public async Task<SitioResponseDto> CrearSitioAsync(SitioRequestDto sitioDto)
        {
            sitioDto.Id = Guid.NewGuid();

            var sitio = new Sitio()
            {
                Id = sitioDto.Id,
                Nombre = sitioDto.Nombre,
                UrlPropia = sitioDto.UrlPropia,
                ConfiguracionSitio = sitioDto.ConfiguracionSitio,
                ColorPrimario = sitioDto.ColorPrimario,
                ColorSecundario = sitioDto.ColorSecundario,
                TipoRegistro = sitioDto.TipoRegistro,
                Estado = sitioDto.Estado,
                Logo = sitioDto.Logo
            };

            await _unitOfWork.Sitios.AddAsync(sitio);
            await _unitOfWork.SaveChangesAsync();

            return new SitioResponseDto()
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Sitio creado exitosamente"
            };
        }

        public async Task<SitioResponseDto> ActualizarSitioAsync(SitioRequestDto sitioDto)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioDto.Id);

            if (sitio == null)
                return new SitioResponseDto { Mensaje = "Sitio no encontrado" };

            if (!string.IsNullOrWhiteSpace(sitioDto.Nombre))
                sitio.Nombre = sitioDto.Nombre;

            if (!string.IsNullOrWhiteSpace(sitioDto.UrlPropia))
                sitio.UrlPropia = sitioDto.UrlPropia;

            if (!string.IsNullOrWhiteSpace(sitioDto.ConfiguracionSitio))
                sitio.ConfiguracionSitio = sitioDto.ConfiguracionSitio;

            if (!string.IsNullOrWhiteSpace(sitioDto.ColorPrimario))
                sitio.ColorPrimario = sitioDto.ColorPrimario;

            if (!string.IsNullOrWhiteSpace(sitioDto.ColorSecundario))
                sitio.ColorSecundario = sitioDto.ColorSecundario;

            if (sitioDto.Logo != null)
                sitio.Logo = sitioDto.Logo;

            // TipoRegistro intentionally omitted
            sitio.Estado = sitio.Estado;

            await _unitOfWork.Sitios.UpdateAsync(sitio);
            await _unitOfWork.SaveChangesAsync();

            return new SitioResponseDto
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Sitio actualizado exitosamente"
            };
        }

        public async Task<SitioResponseDto> EliminarSitioAsync(Guid sitioId)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId);
            if (sitio == null)
                return new SitioResponseDto { Mensaje = "Sitio no encontrado" };

            await _unitOfWork.Sitios.DeleteAsync(sitioId);
            await _unitOfWork.SaveChangesAsync();

            return new SitioResponseDto()
            {
                Id = sitio.Id,
                Nombre = sitio.Nombre,
                Mensaje = "Sitio Eliminado exitosamente"
            };
        }
    }
}
