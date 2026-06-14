using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Infrastructure.Services
{
    public class RecordatorioPrediccionBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RecordatorioPrediccionBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var todosPartidos = await unitOfWork.Partidos.GetAllAsync();
                    var partidosPendientes = todosPartidos
                        .Where(p => p.ResultadoLocal == null && p.Fecha > DateTime.UtcNow)
                        .ToList();

                    var pencas = await unitOfWork.Pencas.GetAllConDetalleAsync();
                    var todasPredicciones = await unitOfWork.Predicciones.GetAllAsync();
                    var pagos = await unitOfWork.Pagos.GetAllAsync();

                    foreach (var partido in partidosPendientes)
                    {
                        var pencasDelPartido = pencas
                            .Where(p => p.Plantilla.EventoDeportivoId == partido.EventoDeportivoId)
                            .ToList();

                        foreach (var penca in pencasDelPartido)
                        {
                            var deadline = partido.Fecha.AddMinutes(-penca.Plantilla.TiempoLimitePrevioMinutos);
                            var ahora = DateTime.UtcNow;

                            // Solo notificar si el deadline cae dentro del parametro fijado
                            var parametros = await unitOfWork.ParametrosSitio.GetBySitioIdAsync(penca.SitioId);
                            if (parametros == null || !parametros.NotifRecordatorioPrediccion)
                                continue;

                            var ventana = TimeSpan.FromHours(parametros.HorasAntesRecordatorio);
                            if (deadline <= ahora || deadline > ahora.Add(ventana))
                                continue;

                            var usuarioIdsPenca = pagos
                                .Where(p => p.PencaId == penca.Id)
                                .Select(p => p.UsuarioId)
                                .ToList();

                            foreach (var usuarioId in usuarioIdsPenca)
                            {
                                var yaPredicho = todasPredicciones.Any(pr =>
                                    pr.UsuarioId == usuarioId &&
                                    pr.PartidoId == partido.Id &&
                                    pr.PencaId == penca.Id);

                                if (yaPredicho) continue;

                                var usuario = await unitOfWork.Usuarios.GetByIdAsync(usuarioId);
                                if (usuario == null || !usuario.NotifRecordatorioPrediccion)
                                    continue;

                                var equipoLocal = await unitOfWork.Equipos.GetByIdAsync(partido.EquipoLocalId);
                                var equipoVisitante = await unitOfWork.Equipos.GetByIdAsync(partido.EquipoVisitanteId);

                                var titulo = "⏰ ¡Faltan pocas horas para cerrar predicciones!";
                                var cuerpo = $"Todavía no predijiste {equipoLocal?.Nombre} vs {equipoVisitante?.Nombre} en {penca.Nombre}. ¡No te quedes afuera!";

                                var tokens = new[] { usuario.FcmToken, usuario.FcmTokenWeb }
                                    .Where(t => !string.IsNullOrEmpty(t))
                                    .ToList();

                                if (tokens.Count == 0) continue;

                                foreach (var token in tokens)
                                {
                                    try
                                    {
                                        await notificationService.EnviarAsync(token!, titulo, cuerpo);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Notification error for token {token}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en RecordatorioPrediccionBackgroundService: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
        }
    }
}