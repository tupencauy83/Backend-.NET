using System;
using System.Collections.Generic;
using System.Text;

using TuPenca.Application.DTOs.Estadisticas;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IEstadisticasService
    {
        Task<EstadisticasGlobalesDto> ObtenerGlobalesAsync();
        Task<EstadisticasSitioDto> ObtenerPorSitioAsync(Guid sitioId, EstadisticasSitioFiltroDto? filtro = null);
    }
}