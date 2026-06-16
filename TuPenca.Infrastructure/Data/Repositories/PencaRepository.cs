using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces.Repositories;

namespace TuPenca.Infrastructure.Data.Repositories
{
    public class PencaRepository : Repository<Penca>, IPencaRepository
    {
        public PencaRepository(AppDbContext context) : base(context) { }

        public async Task<Penca?> GetByIdConDetalleAsync(Guid id)
            => await _context.Pencas
                .Include(p => p.Plantilla)
                    .ThenInclude(pl => pl.Evento)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Penca>> GetAllConDetalleAsync()
            => await _context.Pencas
                .Include(p => p.Plantilla)
                    .ThenInclude(pl => pl.Evento)
                .Include(p => p.Sitio)
                .ToListAsync();
    }
}