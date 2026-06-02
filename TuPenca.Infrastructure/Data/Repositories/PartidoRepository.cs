using Microsoft.EntityFrameworkCore;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces.Repositories;

namespace TuPenca.Infrastructure.Data.Repositories
{
    public class PartidoRepository : Repository<Partido>, IPartidoRepository
    {

        public PartidoRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Partido>> ObtenerPendientesConExternalMatchIdAsync()
        {
            return await _context.Partidos
                .Where(p =>
                    !string.IsNullOrEmpty(p.ExternalMatchId) &&
                    p.ResultadoLocal == null &&
                    p.ResultadoVisitante == null &&
                    p.Fecha <= DateTime.Now)
                .ToListAsync();
        }
    }
}