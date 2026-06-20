using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Infrastructure.Services
{
    public class ResumenSemanalBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ResumenSemanalBackgroundService(IServiceScopeFactory scopeFactory)
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

                        var pagos = await unitOfWork.Pagos.GetAllAsync();
                        var porPenca = pagos.GroupBy(p => p.PencaId);

                        foreach (var grupo in porPenca)
                        {

                        var pencaId = grupo.Key;
                        var penca = await unitOfWork.Pencas.GetByIdAsync(pencaId); // una sola vez acá

                        var parametros = await unitOfWork.ParametrosSitio.GetBySitioIdAsync(penca.SitioId);
                        if (parametros == null || !parametros.NotifResumenSemanal)
                            continue;

                        var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Montevideo");
                        var ahoraUruguay = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

                        // Día en horario de Uruguay; hora almacenada en UTC (la UI convierte UTC−3).
                        if (ahoraUruguay.DayOfWeek != parametros.DiaResumenSemanal
                            || DateTime.UtcNow.Hour != parametros.HoraResumenSemanal)
                            continue;

                        var puntajes = await unitOfWork.PuntajesUsuario.GetAllAsync();
                            var ranking = puntajes
                                .Where(p => p.PencaId == pencaId)
                                .GroupBy(p => p.UsuarioId)
                                .Select(g => new { UsuarioId = g.Key, Total = g.Sum(p => p.PuntosPartido) })
                                .OrderByDescending(x => x.Total)
                                .ToList();

                            foreach (var pago in grupo)
                            {
                                var usuario = await unitOfWork.Usuarios.GetByIdAsync(pago.UsuarioId);
                                if (usuario == null || !usuario.NotifResumenSemanal)
                                    continue;

                                var posicion = ranking.FindIndex(r => r.UsuarioId == pago.UsuarioId) + 1;
                                var puntos = ranking.FirstOrDefault(r => r.UsuarioId == pago.UsuarioId)?.Total ?? 0;

                                var titulo = $"📊 Resumen semanal — {penca?.Nombre}";
                                var cuerpo = $"Estás en el puesto {posicion} con {puntos} puntos. ¡Seguí prediciendo!";

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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en ResumenSemanalBackgroundService: {ex.Message}");
                }

                // Check once a day
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}