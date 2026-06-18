using System;
using System.Collections.Generic;
using System.Text;

using TuPenca.Application.DTOs.Prediccion;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IPrediccionService
    {
        Task<PrediccionResponseDto> CrearOModificarAsync(PrediccionRequestDto dto, Guid usuarioId);
        Task<IEnumerable<PrediccionResponseDto>> ObtenerMisPrediccionesAsync(Guid usuarioId, Guid pencaId);
        Task<HistorialPencaResponseDto> ObtenerHistorialAsync(Guid usuarioId, Guid pencaId);
    }
}