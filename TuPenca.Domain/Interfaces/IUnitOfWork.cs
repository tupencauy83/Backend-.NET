using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces.Repositories;

namespace TuPenca.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUsuarioRepository Usuarios { get; }
        IAdministradorRepository Administrador { get; }
        IRepository<Sitio> Sitios { get; }
        IPencaRepository Pencas { get; }
        IPartidoRepository Partidos { get; }
        IPrediccionRepository Predicciones { get; }
        IPuntajeUsuarioRepository PuntajesUsuario { get; }
        IPremioRepository Premios { get; }
        IRepository<Pago> Pagos { get; }
        IRepository<MensajeChat> MensajesChat { get; }
        IRepository<Notificacion> Notificaciones { get; }
        IRepository<Invitacion> Invitaciones { get; }
        IPlantillaPencaRepository PlantillasPenca { get; }
        IRepository<EventoDeportivo> EventosDeportivos { get; }
        IRepository<Equipo> Equipos { get; }
        IRepository<Deporte> Deportes { get; }
        IRepository<TipoCompetencia> TiposCompetencia { get; }
        IRepository<ParametrosSitio> ParametrosSitio { get; }

        Task<int> SaveChangesAsync(); // ← confirma todos los cambios pendientes
    }
}
