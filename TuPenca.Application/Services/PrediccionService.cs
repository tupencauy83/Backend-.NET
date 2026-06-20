using TuPenca.Application.Common;
using TuPenca.Application.DTOs.Prediccion;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class PrediccionService : IPrediccionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagoService _pagoService;
        private readonly IEventoDeportivoService _eventoDeportivoService;

        public PrediccionService(IUnitOfWork unitOfWork, IPagoService pagoService, IEventoDeportivoService eventoDeportivoService)
        {
            _unitOfWork = unitOfWork;
            _pagoService = pagoService;
            _eventoDeportivoService = eventoDeportivoService;
        }

        public async Task<PrediccionResponseDto> CrearOModificarAsync(PrediccionRequestDto dto, Guid usuarioId)
        {
            // 1. Verificar que el usuario pagó en la penca
            var pago = await _pagoService.UsuarioPagoEnPencaAsync(usuarioId, dto.PencaId);
            if (!pago)
                throw new Exception("Debes estar inscripto en la penca para predecir");

            // 2. Verificar que el partido existe y pertenece al evento de la penca
            var partido = await _unitOfWork.Partidos.GetByIdAsync(dto.PartidoId);
            if (partido == null)
                throw new Exception("Partido no encontrado");

            // 3. Verificar que el partido no empezó todavía
            var penca = await _unitOfWork.Pencas.GetByIdConDetalleAsync(dto.PencaId);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            if (penca.Estado != EstadoPenca.EnCurso)
            {
                var mensaje = penca.Estado switch
                {
                    EstadoPenca.Abierta => "Las predicciones se habilitan cuando la penca esté en juego",
                    EstadoPenca.Finalizada => "La penca finalizó; no se pueden modificar predicciones",
                    EstadoPenca.Cancelada => "Esta penca fue cancelada",
                    _ => "No se pueden cargar predicciones en el estado actual de la penca"
                };
                throw new Exception(mensaje);
            }

            var partidoUtc = UruguayTimeHelper.AsUtc(partido.Fecha);
            var tiempoLimite = partidoUtc.AddMinutes(-penca.Plantilla.TiempoLimitePrevioMinutos);
            if (DateTime.UtcNow >= tiempoLimite)
                throw new Exception("El tiempo límite para predecir este partido ya cerró");

            // 4. Verificar si ya existe una predicción para este partido en esta penca
            var predicciones = await _unitOfWork.Predicciones.GetAllAsync();
            var prediccionExistente = predicciones.FirstOrDefault(p =>
                p.UsuarioId == usuarioId &&
                p.PartidoId == dto.PartidoId &&
                p.PencaId == dto.PencaId);

            Guid prediccionId;

            if (prediccionExistente != null)
            {
                prediccionId = prediccionExistente.Id;
                prediccionExistente.GolesLocal = dto.GolesLocal;
                prediccionExistente.GolesVisitante = dto.GolesVisitante;
                prediccionExistente.EquipoGanadorPredichoId = dto.EquipoGanadorPredichoId;
                await _unitOfWork.Predicciones.UpdateAsync(prediccionExistente);
            }
            else
            {
                prediccionId = Guid.NewGuid();
                var prediccion = new Prediccion
                {
                    Id = prediccionId,
                    UsuarioId = usuarioId,
                    PartidoId = dto.PartidoId,
                    PencaId = dto.PencaId,
                    GolesLocal = dto.GolesLocal,
                    GolesVisitante = dto.GolesVisitante,
                    EquipoGanadorPredichoId = dto.EquipoGanadorPredichoId
                };
                await _unitOfWork.Predicciones.AddAsync(prediccion);
            }

            await _unitOfWork.SaveChangesAsync();

            // Cargar equipos para el response
            var equipoLocal = await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoLocalId);
            var equipoVisitante = await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoVisitanteId);

            return new PrediccionResponseDto
            {
                Id = prediccionId,
                PartidoId = dto.PartidoId,
                EquipoLocal = equipoLocal?.Nombre ?? string.Empty,
                EquipoVisitante = equipoVisitante?.Nombre ?? string.Empty,
                GolesLocal = dto.GolesLocal,
                GolesVisitante = dto.GolesVisitante,
                EquipoGanadorPredichoId = dto.EquipoGanadorPredichoId,
                FechaPartido = UruguayTimeHelper.AsUtc(partido.Fecha)
            };
        }

        public async Task<IEnumerable<PrediccionResponseDto>> ObtenerMisPrediccionesAsync(Guid usuarioId, Guid pencaId)
        {
            var predicciones = await _unitOfWork.Predicciones.GetAllAsync();
            var misPredicciones = predicciones
                .Where(p => p.UsuarioId == usuarioId && p.PencaId == pencaId)
                .ToList();

            var resultado = new List<PrediccionResponseDto>();
            foreach (var pred in misPredicciones)
            {
                var partido = await _unitOfWork.Partidos.GetByIdAsync(pred.PartidoId);
                var equipoLocal = await _unitOfWork.Equipos.GetByIdAsync(partido!.EquipoLocalId);
                var equipoVisitante = await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoVisitanteId);

                resultado.Add(new PrediccionResponseDto
                {
                    Id = pred.Id,
                    PartidoId = pred.PartidoId,
                    EquipoLocal = equipoLocal?.Nombre ?? string.Empty,
                    EquipoVisitante = equipoVisitante?.Nombre ?? string.Empty,
                    GolesLocal = pred.GolesLocal,
                    GolesVisitante = pred.GolesVisitante,
                    EquipoGanadorPredichoId = pred.EquipoGanadorPredichoId,
                    FechaPartido = UruguayTimeHelper.AsUtc(partido.Fecha)
                });
            }

            return resultado;
        }

        // NUEVA FUNCION PARA MOBILE
        public async Task<IEnumerable<PrediccionResponseDto>> ObtenerMisPrediccionesYTodosLosPartidosAsync(Guid usuarioId,Guid pencaId)
        {
            // Obtener la penca
            var penca = await _unitOfWork.Pencas.GetByIdAsync(pencaId);

            if (penca == null)
                throw new Exception("Penca no encontrada");

            // Obtener la plantilla asociada
            var plantilla = await _unitOfWork.PlantillasPenca
                .GetByIdAsync(penca.PlantillaPencaId);

            if (plantilla == null)
                throw new Exception("Plantilla no encontrada");

            // Obtener el evento con todos sus partidos
            var evento = await _eventoDeportivoService
                .ObtenerPorIdAsync(plantilla.EventoDeportivoId);

            if (evento == null)
                throw new Exception("Evento deportivo no encontrado");

            // Obtener las predicciones del usuario para esa penca
            var todasPredicciones = await _unitOfWork.Predicciones.GetAllAsync();

            var misPredicciones = todasPredicciones
                .Where(p =>
                    p.UsuarioId == usuarioId &&
                    p.PencaId == pencaId)
                .ToList();

            var resultado = new List<PrediccionResponseDto>();

            foreach (var partido in evento.Partidos)
            {
                var prediccion = misPredicciones
                    .FirstOrDefault(p => p.PartidoId == partido.Id);

                resultado.Add(new PrediccionResponseDto
                {
                    Id = prediccion?.Id ?? Guid.Empty,

                    PartidoId = partido.Id,

                    EquipoLocal = partido.EquipoLocal,
                    EquipoVisitante = partido.EquipoVisitante,

                    GolesLocal = prediccion?.GolesLocal ?? 0,
                    GolesVisitante = prediccion?.GolesVisitante ?? 0,
                    EquipoGanadorPredichoId = prediccion?.EquipoGanadorPredichoId,

                    FechaPartido = UruguayTimeHelper.AsUtc(partido.Fecha)
                });
            }

            return resultado
                .OrderBy(p => p.FechaPartido)
                .ToList();
        }

        public async Task<HistorialPencaResponseDto> ObtenerHistorialAsync(Guid usuarioId, Guid pencaId)
        {
            var penca = await _unitOfWork.Pencas.GetByIdConDetalleAsync(pencaId);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            var plantilla = penca.Plantilla
                ?? await _unitOfWork.PlantillasPenca.GetByIdAsync(penca.PlantillaPencaId);
            if (plantilla == null)
                throw new Exception("Plantilla no encontrada");

            var evento = await _eventoDeportivoService.ObtenerPorIdAsync(plantilla.EventoDeportivoId);
            if (evento == null)
                throw new Exception("Evento deportivo no encontrado");

            var todasPredicciones = await _unitOfWork.Predicciones.GetAllAsync();
            var misPredicciones = todasPredicciones
                .Where(p => p.UsuarioId == usuarioId && p.PencaId == pencaId)
                .ToDictionary(p => p.PartidoId);

            var puntajesPenca = await _unitOfWork.PuntajesUsuario.GetByPencaAsync(pencaId);
            var misPuntajes = puntajesPenca
                .Where(p => p.UsuarioId == usuarioId)
                .ToDictionary(p => p.PartidoId, p => p.PuntosPartido);

            var partidosHistorial = new List<HistorialPartidoDto>();

            foreach (var partido in (evento.Partidos ?? []).OrderBy(p => p.Fecha))
            {
                misPredicciones.TryGetValue(partido.Id, out var prediccion);
                misPuntajes.TryGetValue(partido.Id, out var puntosPartido);

                var jugado = partido.ResultadoLocal.HasValue;

                partidosHistorial.Add(new HistorialPartidoDto
                {
                    PartidoId = partido.Id,
                    FechaPartido = UruguayTimeHelper.AsUtc(partido.Fecha),
                    Fase = partido.Fase,
                    EquipoLocal = partido.EquipoLocal,
                    EquipoVisitante = partido.EquipoVisitante,
                    Predijo = prediccion != null,
                    PrediccionLocal = prediccion?.GolesLocal,
                    PrediccionVisitante = prediccion?.GolesVisitante,
                    ResultadoLocal = partido.ResultadoLocal,
                    ResultadoVisitante = partido.ResultadoVisitante,
                    PartidoJugado = jugado,
                    PuntosObtenidos = prediccion != null && jugado ? puntosPartido : 0,
                });
            }

            return new HistorialPencaResponseDto
            {
                PencaId = pencaId,
                NombrePenca = penca.Nombre,
                PuntosTotales = misPuntajes.Values.Sum(),
                PartidosPredichos = misPredicciones.Count,
                PartidosConResultado = partidosHistorial.Count(p => p.PartidoJugado),
                Partidos = partidosHistorial,
            };
        }

    }
}