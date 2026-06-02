using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Domain.Entities;

namespace TuPenca.Domain.Interfaces.Repositories
{
    public interface IPartidoRepository : IRepository<Partido>
    {
        Task<IEnumerable<Partido>> ObtenerPendientesConExternalMatchIdAsync();
    }
}
