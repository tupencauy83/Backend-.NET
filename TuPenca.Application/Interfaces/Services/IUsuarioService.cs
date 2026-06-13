using TuPenca.Application.DTOs.Usuario;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDto>> ObtenerUsuariosAsync();
        Task<UsuarioDto?> ObtenerUsuarioAsync(Guid id);
        Task<UsuarioResponseDto> ActualizarEstadoAsync(UsuarioActualizarEstadoRequestDto usuarioDto);
        Task<UsuarioResponseDto> ActualizarUsuarioAsync(UsuarioRequestDto usuarioDto);
        Task<UsuarioResponseDto> ActualizarPasswordAsync(UsuarioActualizarPasswordRequestDto usuarioDto);
        Task RegistrarFcmTokenAsync(Guid usuarioId, string fcmToken);
        Task RegistrarFcmTokenWebAsync(Guid usuarioId, string fcmToken);
        Task ActualizarPreferenciasNotificacionAsync(Guid usuarioId, ActualizarPreferenciasNotificacionRequestDto dto);

        Task<PreferenciasNotificacionResponseDto> ObtenerPreferenciasNotificacionAsync(Guid usuarioId);

    }
}
