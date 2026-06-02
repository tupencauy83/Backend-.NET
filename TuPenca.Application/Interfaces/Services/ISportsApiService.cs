using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.SportsApi;

namespace TuPenca.Application.Interfaces.Services
{
    public interface ISportsApiService
    {
        Task<ResultadoExternoDto?> ObtenerResultadoAsync(
            string externalMatchId);
    }
}
