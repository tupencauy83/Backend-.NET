using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.EventoDeportivo;
using TuPenca.Application.DTOs.Partido;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IEventoDeportivoService
    {
        Task<IEnumerable<EventoDeportivoResponseDto>> ObtenerTodosAsync();
        Task<EventoDeportivoResponseDto?> ObtenerPorIdAsync(Guid id);
        Task<EventoDeportivoResponseDto> CrearAsync(EventoDeportivoRequestDto dto);
        Task<PartidoResponseDto> AgregarPartidoAsync(PartidoRequestDto dto);

        // Para calculo de resultados
        Task<ResultadoResponseDto> CargarResultadoAsync(ResultadoRequestDto dto);

        Task<ResultadoResponseDto> SincronizarPartidoAsync(Guid partidoId);

        Task<int> SincronizarPartidosPendientesAsync();
    }
}