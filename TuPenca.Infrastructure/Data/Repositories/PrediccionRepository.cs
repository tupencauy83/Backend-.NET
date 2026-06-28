using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces.Repositories;

namespace TuPenca.Infrastructure.Data.Repositories
{
    public class PrediccionRepository : Repository<Prediccion>, IPrediccionRepository
    {
        public PrediccionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Prediccion>> GetByPartidoConDetalleAsync(Guid partidoId)
     => await _context.Predicciones
         .IgnoreQueryFilters() // proceso de sync global — no corre en contexto de un sitio puntual
         .Include(p => p.Penca)
             .ThenInclude(penca => penca.Plantilla)
                 .ThenInclude(plantilla => plantilla.Reglas)
         .Where(p => p.PartidoId == partidoId)
         .ToListAsync();
    }
}