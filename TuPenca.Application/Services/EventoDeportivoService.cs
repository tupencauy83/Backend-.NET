using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.EventoDeportivo;
using TuPenca.Application.DTOs.Partido;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class EventoDeportivoService : IEventoDeportivoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISportsApiService _sportsApiService;
        private readonly INotificationService _notificationService;


        public EventoDeportivoService(
            IUnitOfWork unitOfWork,
            ISportsApiService sportsApiService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _sportsApiService = sportsApiService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<EventoDeportivoResponseDto>> ObtenerTodosAsync()
        {
            var eventos = await _unitOfWork.EventosDeportivos.GetAllAsync();
            return eventos.Select(e => new EventoDeportivoResponseDto
            {
                Id = e.Id,
                Nombre = e.Nombre,
                FechaInicio = e.FechaInicio,
                FechaFin = e.FechaFin
            });
        }

        public async Task<EventoDeportivoResponseDto?> ObtenerPorIdAsync(Guid id)
        {
            var evento = await _unitOfWork.EventosDeportivos.GetByIdAsync(id);
            if (evento == null) return null;

            // Cargar partidos del evento
            var todosPartidos = await _unitOfWork.Partidos.GetAllAsync();
            var partidosEvento = todosPartidos.Where(p => p.EventoDeportivoId == id).ToList();

            var partidosDto = new List<PartidoResponseDto>();
            foreach (var p in partidosEvento)
            {
                var eqLocal = await _unitOfWork.Equipos.GetByIdAsync(p.EquipoLocalId);
                var eqVisitante = await _unitOfWork.Equipos.GetByIdAsync(p.EquipoVisitanteId);
                partidosDto.Add(new PartidoResponseDto
                {
                    Id = p.Id,
                    Fecha = p.Fecha,
                    Fase = p.Fase,
                    EquipoLocalId = p.EquipoLocalId,
                    EquipoLocal = eqLocal?.Nombre ?? string.Empty,
                    EquipoVisitanteId = p.EquipoVisitanteId,
                    EquipoVisitante = eqVisitante?.Nombre ?? string.Empty,
                    ResultadoLocal = p.ResultadoLocal,
                    ResultadoVisitante = p.ResultadoVisitante
                });
            }

            return new EventoDeportivoResponseDto
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                FechaInicio = evento.FechaInicio,
                FechaFin = evento.FechaFin,
                Partidos = partidosDto
            };
        }

        public async Task<EventoDeportivoResponseDto> CrearAsync(EventoDeportivoRequestDto dto)
        {
            var evento = new EventoDeportivo
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                DeporteId = dto.DeporteId,
                TipoCompetenciaId = dto.TipoCompetenciaId
            };

            await _unitOfWork.EventosDeportivos.AddAsync(evento);
            await _unitOfWork.SaveChangesAsync();

            return new EventoDeportivoResponseDto
            {
                Id = evento.Id,
                Nombre = evento.Nombre,
                FechaInicio = evento.FechaInicio,
                FechaFin = evento.FechaFin
            };
        }

        public async Task<PartidoResponseDto> AgregarPartidoAsync(PartidoRequestDto dto)
        {
            var evento = await _unitOfWork.EventosDeportivos.GetByIdAsync(dto.EventoDeportivoId);
            if (evento == null)
                throw new Exception("Evento deportivo no encontrado");

            var equipoLocal = await _unitOfWork.Equipos.GetByIdAsync(dto.EquipoLocalId);
            var equipoVisitante = await _unitOfWork.Equipos.GetByIdAsync(dto.EquipoVisitanteId);

            if (equipoLocal == null || equipoVisitante == null)
                throw new Exception("Uno o ambos equipos no encontrados");

            if (dto.EquipoLocalId == dto.EquipoVisitanteId)
                throw new Exception("El equipo local y visitante no pueden ser el mismo");

            var partido = new Partido
            {
                Id = Guid.NewGuid(),
                Fecha = dto.Fecha,
                Fase = dto.Fase,
                EquipoLocalId = dto.EquipoLocalId,
                EquipoVisitanteId = dto.EquipoVisitanteId,
                EventoDeportivoId = dto.EventoDeportivoId,
                ExternalMatchId = dto.ExternalMatchId
            };

            await _unitOfWork.Partidos.AddAsync(partido);
            await _unitOfWork.SaveChangesAsync();

            return new PartidoResponseDto
            {
                Id = partido.Id,
                Fecha = partido.Fecha,
                Fase = partido.Fase,
                EquipoLocalId = partido.EquipoLocalId,
                EquipoLocal = equipoLocal.Nombre,
                EquipoVisitanteId = partido.EquipoVisitanteId,
                EquipoVisitante = equipoVisitante.Nombre
            };
        }


        public async Task<ResultadoResponseDto> CargarResultadoAsync(ResultadoRequestDto dto)
        {
            // 1. Verificar que el partido existe
            var partido = await _unitOfWork.Partidos.GetByIdAsync(dto.PartidoId);
            if (partido == null)
                throw new Exception("Partido no encontrado");

            // 2. Cargar resultado
            partido.ResultadoLocal = dto.GolesLocal;
            partido.ResultadoVisitante = dto.GolesVisitante;
            await _unitOfWork.Partidos.UpdateAsync(partido);

            // 3. Traer todas las predicciones de este partido con detalle
            var predicciones = await _unitOfWork.Predicciones.GetByPartidoConDetalleAsync(dto.PartidoId);

            int usuariosActualizados = 0;

            foreach (var prediccion in predicciones)
            {
                // 4. Calcular desviacion
                var desviacion = Math.Abs(dto.GolesLocal - prediccion.GolesLocal)
                               + Math.Abs(dto.GolesVisitante - prediccion.GolesVisitante);

                // 5. Buscar puntaje en reglas de la plantilla de esa penca
                var reglas = prediccion.Penca.Plantilla.Reglas
                    .OrderBy(r => r.Desviacion)
                    .ToList();

                var regla = reglas.FirstOrDefault(r => r.Desviacion == desviacion);
                var puntos = regla?.Puntaje ?? 0;


                // 5.5 agregar puntos por elegir el ganador correcto si corresponde
                if (dto.EquipoGanadorId.HasValue &&
     prediccion.EquipoGanadorPredichoId.HasValue &&
     prediccion.EquipoGanadorPredichoId == dto.EquipoGanadorId)
                {
                    puntos += prediccion.Penca.Plantilla.PuntajeGanador;
                }

                // 6. Buscar si ya existe un PuntajeUsuario para este usuario/penca/partido
                var puntajeExistente = await _unitOfWork.PuntajesUsuario
                    .GetByUsuarioPencaPartidoAsync(prediccion.UsuarioId, prediccion.PencaId, dto.PartidoId);

                if (puntajeExistente != null)
                {
                    puntajeExistente.PuntosPartido = puntos;
                    await _unitOfWork.PuntajesUsuario.UpdateAsync(puntajeExistente);
                }
                else
                {
                    var nuevoPuntaje = new PuntajeUsuario
                    {
                        Id = Guid.NewGuid(),
                        UsuarioId = prediccion.UsuarioId,
                        PencaId = prediccion.PencaId,
                        PartidoId = dto.PartidoId,
                        PuntosPartido = puntos
                    };
                    await _unitOfWork.PuntajesUsuario.AddAsync(nuevoPuntaje);
                }

                usuariosActualizados++;

                // NOTIFICACION DE PARTIDO FINALIZADO Y RESULTADO CARGADO
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(prediccion.UsuarioId);
                if (usuario != null && usuario.NotifResultadoPartido)
                {
                    var equipoLocalNombre = (await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoLocalId))?.Nombre ?? "";
                    var equipoVisitanteNombre = (await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoVisitanteId))?.Nombre ?? "";

                    var titulo = "Resultado cargado 🏆";
                    var cuerpo = $"{equipoLocalNombre} {dto.GolesLocal} - {dto.GolesVisitante} {equipoVisitanteNombre}. Obtuviste {puntos} puntos.";

                    var tokens = new[]
                    {
                        usuario.FcmToken,
                        usuario.FcmTokenWeb
                    }
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                    foreach (var token in tokens)
                    {
                        try
                        {
                            await _notificationService.EnviarAsync(token!, titulo, cuerpo);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error de notificacion para token: {token}: {ex}");
                        }
                    }
                }
            }



            await _unitOfWork.SaveChangesAsync();

            var equipoLocal = await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoLocalId);
            var equipoVisitante = await _unitOfWork.Equipos.GetByIdAsync(partido.EquipoVisitanteId);

            return new ResultadoResponseDto
            {
                PartidoId = partido.Id,
                EquipoLocal = equipoLocal?.Nombre ?? string.Empty,
                EquipoVisitante = equipoVisitante?.Nombre ?? string.Empty,
                GolesLocal = dto.GolesLocal,
                GolesVisitante = dto.GolesVisitante,
                UsuariosActualizados = usuariosActualizados
            };
        }

        // Para consumir API Externa de resultados

        public async Task<ResultadoResponseDto> SincronizarPartidoAsync(Guid partidoId)
        {
            // Buscar partido
            var partido = await _unitOfWork.Partidos.GetByIdAsync(partidoId);

            if (partido == null)
                throw new Exception("Partido no encontrado");

            // Verificar que tenga vinculación con API
            if (string.IsNullOrWhiteSpace(partido.ExternalMatchId))
                throw new Exception("El partido no tiene ExternalMatchId configurado");

            // Consultar proveedor externo
            var resultadoApi = await _sportsApiService
                .ObtenerResultadoAsync(partido.ExternalMatchId);

            if (resultadoApi == null)
                throw new Exception("No se pudo obtener información desde TheSportsDB");

            // Verificar si el partido terminó
            if (!resultadoApi.Finalizado)
                throw new Exception("El partido todavía no ha finalizado");

            // Determinar ganador automáticamente
            Guid? ganadorId = null;

            if (resultadoApi.GolesLocal > resultadoApi.GolesVisitante)
            {
                ganadorId = partido.EquipoLocalId;
            }
            else if (resultadoApi.GolesVisitante > resultadoApi.GolesLocal)
            {
                ganadorId = partido.EquipoVisitanteId;
            }

            if (partido.ResultadoLocal.HasValue ||
    partido.ResultadoVisitante.HasValue)
            {
                throw new Exception("El partido ya tiene resultado cargado");
            }

            // Reutilizar toda la lógica existente
            return await CargarResultadoAsync(
                new ResultadoRequestDto
                {
                    PartidoId = partido.Id,
                    GolesLocal = resultadoApi.GolesLocal,
                    GolesVisitante = resultadoApi.GolesVisitante,
                    EquipoGanadorId = ganadorId
                });
        }

        public async Task<int> SincronizarPartidosPendientesAsync()
        {
            var partidos =
                await _unitOfWork.Partidos
                    .ObtenerPendientesConExternalMatchIdAsync();

            int sincronizados = 0;

            foreach (var partido in partidos)
            {
                try
                {
                    await SincronizarPartidoAsync(partido.Id);
                    sincronizados++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error sincronizando partido {partido.Id}: {ex.Message}");
                }
            }

            return sincronizados;
        }

    }
}