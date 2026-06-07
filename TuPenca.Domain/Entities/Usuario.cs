using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Domain.Entities
{
    public class Usuario : BaseEntity, ISitio
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public RolUsuario Rol { get; set; } = RolUsuario.UsuarioComun;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Pendiente;
        public ProveedorAuth ProveedorAuth { get; set; } = ProveedorAuth.Local;
        public byte[]? Foto { get; set; } = null;

        // N Usuarios → 1 Sitio
        public Guid SitioId { get; set; }
        public Sitio Sitio { get; set; } = null!;

        // 1 Usuario → N Pagos
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        // 1 Usuario → N Predicciones
        public ICollection<Prediccion> Predicciones { get; set; } = new List<Prediccion>();

        // 1 Usuario → N PuntajesUsuario
        public ICollection<PuntajeUsuario> Puntajes { get; set; } = new List<PuntajeUsuario>();

        // 1 Usuario → N MensajesChat
        public ICollection<MensajeChat> Mensajes { get; set; } = new List<MensajeChat>();

        // 1 Usuario → N Notificaciones
        public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

        // Token para notificaciones push de firebase, almacenamos el token del dispositivo mobile para mandarle
        // notificaciones personales
        public string? FcmToken { get; set; } = null;
    }
}
