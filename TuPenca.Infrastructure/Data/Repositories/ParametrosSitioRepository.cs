using Microsoft.EntityFrameworkCore;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Infrastructure.Repositories
{
    public class ParametrosSitioRepository : IParametrosSitioRepository
    {
        private readonly AppDbContext _context;

        public ParametrosSitioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ParametrosSitio?> GetBySitioIdAsync(Guid sitioId)
            => await _context.ParametrosSitio.FirstOrDefaultAsync(p => p.SitioId == sitioId);

        public async Task AddAsync(ParametrosSitio parametros)
            => await _context.ParametrosSitio.AddAsync(parametros);

        public async Task UpdateAsync(ParametrosSitio parametros)
            => _context.ParametrosSitio.Update(parametros);
    }
}