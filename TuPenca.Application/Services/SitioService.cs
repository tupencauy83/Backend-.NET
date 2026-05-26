using TuPenca.Application.DTOs.Sitio;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class SitioService : ISitioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SitioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

            sitio.Nombre = sitioDto.Nombre;
            sitio.UrlPropia = sitioDto.UrlPropia;
            sitio.ConfiguracionSitio = sitioDto.ConfiguracionSitio;
            sitio.ColorPrimario = sitioDto.ColorPrimario;
            sitio.ColorSecundario = sitioDto.ColorSecundario;
            sitio.TipoRegistro = sitioDto.TipoRegistro;
            sitio.Estado = sitio.Estado;
            sitio.Logo = sitio.Logo;

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
