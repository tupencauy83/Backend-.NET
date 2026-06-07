using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.Infrastructure.Services
{
    public class ResultadoSyncBackgroundService
        : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ResultadoSyncBackgroundService(
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope =
                        _scopeFactory.CreateScope();

                    var eventoService =
                        scope.ServiceProvider
                            .GetRequiredService<IEventoDeportivoService>();

                    var cantidad =
                        await eventoService
                            .SincronizarPartidosPendientesAsync();

                    Console.WriteLine(
                        $"Sincronización completada. Partidos actualizados: {cantidad}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Error en BackgroundService: {ex.Message}");
                }

                await Task.Delay(
                    TimeSpan.FromMinutes(1),
                    stoppingToken);
            }
        }
    }
}