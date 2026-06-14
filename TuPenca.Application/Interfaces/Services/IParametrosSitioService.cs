using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.ParametrosSitio;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IParametrosSitioService
    {
        Task<ParametrosSitioResponseDto> ObtenerAsync(Guid sitioId);
        Task<ParametrosSitioResponseDto> ActualizarAsync(Guid sitioId, ActualizarParametrosSitioDto dto);
    }
}