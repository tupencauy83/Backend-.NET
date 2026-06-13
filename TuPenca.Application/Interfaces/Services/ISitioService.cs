using TuPenca.Application.DTOs.Sitio;

namespace TuPenca.Application.Interfaces.Services
{
    public interface ISitioService
    {
        Task<IEnumerable<SitioDto>> ObtenerSitiosAsync();

        Task<SitioDto?> ObtenerSitioAsync(Guid sitioId);

        Task<IEnumerable<SitioDto>> ObtenerSitiosPendientesAsync();

        Task<SitioResponseDto> SolicitarSitioAsync(SitioPendienteRequestDto sitioDto);

        Task<SitioResponseDto> ActualizarSitioPendienteAsync(SitioActualizarEstadoRequest sitioDto);

        Task<SitioResponseDto> AprobarSitioAsync(Guid sitioId);

        Task<SitioResponseDto> RechazarSitioAsync(Guid sitioId);

        Task<SitioResponseDto> CrearSitioAsync(SitioRequestDto sitioDto);

        Task<SitioResponseDto> ActualizarSitioAsync(SitioRequestDto sitioDto);

        Task<SitioResponseDto> EliminarSitioAsync(Guid sitioId);

        Task<SitioResponseDto> CambiarTipoRegistroAsync(CambiarTipoRegistroDto dto);
    }
}
