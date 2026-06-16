using System;
using System.Collections.Generic;
using System.Text;

using TuPenca.Application.DTOs.Estadisticas;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IEstadisticasService
    {
        Task<EstadisticasGlobalesDto> ObtenerGlobalesAsync(EstadisticasGlobalesFiltroDto? filtro = null);
        Task<EstadisticasSitioDto> ObtenerPorSitioAsync(Guid sitioId, EstadisticasSitioFiltroDto? filtro = null);
    }
}