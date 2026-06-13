using TuPenca.Application.DTOs.Estadisticas;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class EstadisticasService : IEstadisticasService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EstadisticasService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EstadisticasGlobalesDto> ObtenerGlobalesAsync()
        {
            var sitios = await _unitOfWork.Sitios.GetAllAsync();
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var pencas = await _unitOfWork.Pencas.GetAllConDetalleAsync();
            var pagos = await _unitOfWork.Pagos.GetAllAsync();

            var pagosAprobados = pagos.Where(p => p.Estado == EstadoPago.Aprobado).ToList();

            var totalRecaudado = 0;
            var totalComisiones = 0;

            foreach (var pago in pagosAprobados)
            {
                var penca = pencas.FirstOrDefault(p => p.Id == pago.PencaId);
                if (penca?.Plantilla == null) continue;

                totalRecaudado += pago.Monto;
                totalComisiones += pago.Monto * penca.Plantilla.PorcentajeComision / 100;
            }

            var topPorUsuarios = usuarios
                .GroupBy(u => u.SitioId)
                .Select(g => new
                {
                    SitioId = g.Key,
                    Total = g.Count()
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .Select(x => new EstadisticaSitioResumenDto
                {
                    NombreSitio = sitios.FirstOrDefault(s => s.Id == x.SitioId)?.Nombre ?? string.Empty,
                    Valor = x.Total
                })
                .ToList();

            var topPorRecaudacion = pagosAprobados
                .Join(pencas, p => p.PencaId, penca => penca.Id, (p, penca) => new { p.Monto, penca.SitioId })
                .GroupBy(x => x.SitioId)
                .Select(g => new
                {
                    SitioId = g.Key,
                    Total = g.Sum(x => x.Monto)
                })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .Select(x => new EstadisticaSitioResumenDto
                {
                    NombreSitio = sitios.FirstOrDefault(s => s.Id == x.SitioId)?.Nombre ?? string.Empty,
                    Valor = x.Total
                })
                .ToList();

            return new EstadisticasGlobalesDto
            {
                TotalSitios = sitios.Count(),
                TotalUsuarios = usuarios.Count(),
                TotalPencasActivas = pencas.Count(p => p.Estado == EstadoPenca.Abierta || p.Estado == EstadoPenca.EnCurso),
                TotalPencasFinalizadas = pencas.Count(p => p.Estado == EstadoPenca.Finalizada),
                TotalRecaudado = totalRecaudado,
                TotalComisionesGeneradas = totalComisiones,
                TopSitiosPorUsuarios = topPorUsuarios,
                TopSitiosPorRecaudacion = topPorRecaudacion
            };
        }

        public async Task<EstadisticasSitioDto> ObtenerPorSitioAsync(Guid sitioId, EstadisticasSitioFiltroDto? filtro = null)
        {
            filtro ??= new EstadisticasSitioFiltroDto();

            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId);
            if (sitio == null)
                throw new Exception("Sitio no encontrado");

            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();
            var usuariosSitio = usuarios.Where(u => u.SitioId == sitioId).ToList();

            var pencas = await _unitOfWork.Pencas.GetAllConDetalleAsync();
            var pencasSitio = pencas.Where(p => p.SitioId == sitioId).ToList();

            if (filtro.EstadoPenca.HasValue)
                pencasSitio = pencasSitio.Where(p => p.Estado == filtro.EstadoPenca.Value).ToList();

            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var termino = filtro.Buscar.Trim();
                pencasSitio = pencasSitio
                    .Where(p => p.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var pencaIds = pencasSitio.Select(p => p.Id).ToHashSet();

            var pagos = await _unitOfWork.Pagos.GetAllAsync();
            var pagosSitio = pagos
                .Where(p => p.Estado == EstadoPago.Aprobado && pencaIds.Contains(p.PencaId))
                .ToList();

            if (filtro.FechaDesde.HasValue)
            {
                var desde = filtro.FechaDesde.Value.Date;
                pagosSitio = pagosSitio.Where(p => p.Fecha.Date >= desde).ToList();
            }

            if (filtro.FechaHasta.HasValue)
            {
                var hasta = filtro.FechaHasta.Value.Date;
                pagosSitio = pagosSitio.Where(p => p.Fecha.Date <= hasta).ToList();
            }

            var totalRecaudadoSitio = 0;
            var totalComisionesSitio = 0;

            foreach (var pago in pagosSitio)
            {
                var penca = pencasSitio.FirstOrDefault(p => p.Id == pago.PencaId);
                if (penca?.Plantilla == null) continue;

                totalRecaudadoSitio += pago.Monto;
                totalComisionesSitio += pago.Monto * penca.Plantilla.PorcentajeComision / 100;
            }

            var predicciones = await _unitOfWork.Predicciones.GetAllAsync();
            var estadisticasPorPenca = new List<EstadisticaPencaDto>();

            foreach (var penca in pencasSitio)
            {
                var pagosPenca = pagosSitio.Where(p => p.PencaId == penca.Id).ToList();
                var participantes = pagosPenca.Count;

                var recaudadoPenca = 0;
                var comisionPenca = 0;
                foreach (var pago in pagosPenca)
                {
                    recaudadoPenca += pago.Monto;
                    if (penca.Plantilla != null)
                        comisionPenca += pago.Monto * penca.Plantilla.PorcentajeComision / 100;
                }

                var puntajes = await _unitOfWork.PuntajesUsuario.GetByPencaAsync(penca.Id);
                var lider = puntajes
                    .GroupBy(p => new { p.UsuarioId, p.Usuario.Nombre })
                    .Select(g => new
                    {
                        Nombre = g.Key.Nombre,
                        Puntos = g.Sum(p => p.PuntosPartido)
                    })
                    .OrderByDescending(x => x.Puntos)
                    .FirstOrDefault();

                var predsPenca = predicciones.Where(p => p.PencaId == penca.Id).ToList();
                var partidosConPrediccion = predsPenca
                    .Select(p => p.PartidoId)
                    .Distinct()
                    .Count();

                estadisticasPorPenca.Add(new EstadisticaPencaDto
                {
                    PencaId = penca.Id,
                    NombrePenca = penca.Nombre,
                    Estado = penca.Estado,
                    MontoEntrada = penca.Plantilla?.MontoEntrada ?? 0,
                    LiderActual = lider?.Nombre ?? "Sin predicciones",
                    PuntosLider = lider?.Puntos ?? 0,
                    TotalParticipantes = participantes,
                    TotalPartidosConPrediccion = partidosConPrediccion,
                    TotalPredicciones = predsPenca.Count,
                    TotalRecaudado = recaudadoPenca,
                    TotalComision = comisionPenca
                });
            }

            return new EstadisticasSitioDto
            {
                NombreSitio = sitio.Nombre,
                TotalUsuarios = usuariosSitio.Count,
                UsuariosPendientes = usuariosSitio.Count(u => u.Estado == EstadoUsuario.Pendiente),
                TotalPencasActivas = pencasSitio.Count(p => p.Estado == EstadoPenca.Abierta || p.Estado == EstadoPenca.EnCurso),
                TotalPencasFinalizadas = pencasSitio.Count(p => p.Estado == EstadoPenca.Finalizada),
                TotalInscripciones = pagosSitio.Count,
                TotalRecaudado = totalRecaudadoSitio,
                TotalComisionesGeneradas = totalComisionesSitio,
                EstadisticasPorPenca = estadisticasPorPenca
                    .OrderByDescending(p => p.TotalRecaudado)
                    .ThenByDescending(p => p.TotalParticipantes)
                    .ToList()
            };
        }
    }
}
