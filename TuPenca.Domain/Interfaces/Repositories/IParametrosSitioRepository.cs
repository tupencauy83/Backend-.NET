using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Domain.Entities;

namespace TuPenca.Domain.Interfaces
{
    public interface IParametrosSitioRepository
    {
        Task<ParametrosSitio?> GetBySitioIdAsync(Guid sitioId);
        Task AddAsync(ParametrosSitio parametros);
        Task UpdateAsync(ParametrosSitio parametros);
    }
}