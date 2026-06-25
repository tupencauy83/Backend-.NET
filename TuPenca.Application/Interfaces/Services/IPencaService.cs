using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.Penca;
using TuPenca.Domain.Enums;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IPencaService
    {
        Task<IEnumerable<PencaResponseDto>> ObtenerTodasAsync();
        Task<PencaResponseDto?> ObtenerPorIdAsync(Guid id);
        Task<PencaResponseDto> CrearAsync(PencaRequestDto dto, Guid sitioId);
        Task<PencaResponseDto> CambiarEstadoAsync(Guid id, EstadoPenca nuevoEstado);
        Task EliminarAsync(Guid id);

        //tabla posicion
        Task<TablaPosicionesDto> ObtenerTablaPosicionesAsync(Guid pencaId, Guid usuarioId, string rol);

        // Calcular ganadores
        Task<IEnumerable<PremioResponseDto>> ObtenerGanadoresAsync(Guid pencaId);

        // Editar % de premios de ganadores
        Task<PencaEditPremioDto> EditarPremiosAsync(Guid PencaId,PencaEditPremioDto dto);

        // Para mobile, utility

        Task<TiempoLimitePrediccionDto> ObtenerTiempoLimitePrediccionAsync(Guid pencaId);

    }
}