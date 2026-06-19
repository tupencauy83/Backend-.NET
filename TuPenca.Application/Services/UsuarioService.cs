using Microsoft.AspNetCore.Identity;
using TuPenca.Application.DTOs.Usuario;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;
using TuPenca.Domain.Interfaces.Repositories;

namespace TuPenca.Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PasswordHasher<string> _hasher = new();

        public UsuarioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UsuarioDto>> ObtenerUsuariosAsync()
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var result = new List<UsuarioDto>();
            foreach (var usuario in usuarios)
            {
                result.Add(new UsuarioDto()
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    FechaRegistro = usuario.FechaRegistro,
                    Estado = usuario.Estado,
                    SitioId = usuario.SitioId

                });
            }
            return result;
        }

        public async Task<UsuarioDto?> ObtenerUsuarioAsync(Guid id)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);
            if (usuario == null) return null;

            return new UsuarioDto()
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                FechaRegistro = usuario.FechaRegistro,
                Estado = usuario.Estado,
                SitioId = usuario.SitioId,
                Foto = usuario.Foto,
            };
        }

        public async Task<UsuarioResponseDto> ActualizarEstadoAsync(UsuarioActualizarEstadoRequestDto usuarioDto)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioDto.Id);
            if (usuario == null)
                return new UsuarioResponseDto { Mensaje = "Usuario no encontrado" };

            if (usuarioDto.Estado == EstadoUsuario.Aprobado)
                usuario.Estado = EstadoUsuario.Aprobado;
            else
                usuario.Estado = EstadoUsuario.Rechazado;

            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Mensaje = "Usuario actualizado exitosamente"
            };
        }

        public async Task<UsuarioResponseDto> ActualizarUsuarioAsync(UsuarioRequestDto usuarioDto)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioDto.Id);
            if (usuario == null)
                return new UsuarioResponseDto { Mensaje = "Usuario no encontrado" };

            usuario.Nombre = usuarioDto.Nombre;
            usuario.Foto = usuarioDto.Foto;

            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Mensaje = "Usuario actualizado exitosamente"
            };
        }

        public async Task<UsuarioResponseDto> ActualizarPasswordAsync(UsuarioActualizarPasswordRequestDto usuarioDto)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioDto.Id);
            if (usuario == null)
                return new UsuarioResponseDto { Mensaje = "Usuario no encontrado" };

            if (string.IsNullOrEmpty(usuarioDto.Password) || string.IsNullOrEmpty(usuarioDto.PasswordConfirm))
                return new UsuarioResponseDto { Mensaje = "Las passwords deben pueden ser vacias" };

            if (usuarioDto.Password != usuarioDto.PasswordConfirm)
                return new UsuarioResponseDto { Mensaje = "Las passwords deben ser iguales" };

            usuario.PasswordHash = HashPassword(usuarioDto.Password);
            usuario.ProveedorAuth = ProveedorAuth.Local;

            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();

            return new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Mensaje = "Password de usuario actualizado exitosamente"
            };
        }

        // registrar token en el dispositivo mobile
        public async Task RegistrarFcmTokenAsync(Guid usuarioId, string fcmToken)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            usuario.FcmToken = fcmToken;
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync(); // or however you commit in your service layer
        }


        // registrar token en el navegador web
        public async Task RegistrarFcmTokenWebAsync(Guid usuarioId, string fcmTokenWeb)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            usuario.FcmTokenWeb = fcmTokenWeb;
            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync(); // or however you commit in your service layer
        }

        public async Task ActualizarPreferenciasNotificacionAsync(Guid usuarioId, ActualizarPreferenciasNotificacionRequestDto dto)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            usuario.NotifRecordatorioPrediccion = dto.NotifRecordatorioPrediccion;
            usuario.NotifResultadoPartido = dto.NotifResultadoPartido;
            usuario.NotifResumenSemanal = dto.NotifResumenSemanal;

            await _unitOfWork.Usuarios.UpdateAsync(usuario);
            await _unitOfWork.SaveChangesAsync();
        }

        // para ver las preferencias de notificacion en web o mobile
        public async Task<PreferenciasNotificacionResponseDto> ObtenerPreferenciasNotificacionAsync(Guid usuarioId)
        {
            var usuario = await _unitOfWork.Usuarios.GetByIdAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            return new PreferenciasNotificacionResponseDto
            {
                NotifRecordatorioPrediccion = usuario.NotifRecordatorioPrediccion,
                NotifResultadoPartido = usuario.NotifResultadoPartido,
                NotifResumenSemanal = usuario.NotifResumenSemanal
            };
        }

        public string HashPassword(string password) => _hasher.HashPassword(null!, password);
    }
}
