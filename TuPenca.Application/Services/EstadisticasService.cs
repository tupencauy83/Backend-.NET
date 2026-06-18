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

        public async Task<EstadisticasGlobalesDto> ObtenerGlobalesAsync(EstadisticasGlobalesFiltroDto? filtro = null)
        {
            filtro ??= new EstadisticasGlobalesFiltroDto();

            var sitios = (await _unitOfWork.Sitios.GetAllAsync()).ToList();

            if (filtro.EstadoSitio.HasValue)
                sitios = sitios.Where(s => s.Estado == filtro.EstadoSitio.Value).ToList();

            if (!string.IsNullOrWhiteSpace(filtro.Buscar))
            {
                var termino = filtro.Buscar.Trim();
                sitios = sitios.Where(s =>
                    s.Nombre.Contains(termino, StringComparison.OrdinalIgnoreCase) ||
                    s.UrlPropia.Contains(termino, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var sitioIds = sitios.Select(s => s.Id).ToHashSet();

            var usuarios = (await _unitOfWork.Usuarios.GetAllAsync())
                .Where(u => sitioIds.Contains(u.SitioId))
                .ToList();

            var pencas = (await _unitOfWork.Pencas.GetAllConDetalleAsync())
                .Where(p => sitioIds.Contains(p.SitioId))
                .ToList();

            var pencaIds = pencas.Select(p => p.Id).ToHashSet();
            var pencasPorId = pencas.ToDictionary(p => p.Id);

            var pagos = (await _unitOfWork.Pagos.GetAllAsync())
                .Where(p => p.Estado == EstadoPago.Aprobado && pencaIds.Contains(p.PencaId))
                .ToList();

            if (filtro.FechaDesde.HasValue)
            {
                var desde = filtro.FechaDesde.Value.Date;
                pagos = pagos.Where(p => p.Fecha.Date >= desde).ToList();
            }

            if (filtro.FechaHasta.HasValue)
            {
                var hasta = filtro.FechaHasta.Value.Date;
                pagos = pagos.Where(p => p.Fecha.Date <= hasta).ToList();
            }

            var detallePorSitio = new List<EstadisticaSitioDetalleDto>();
            var totalRecaudado = 0;
            var totalComisiones = 0;

            foreach (var sitio in sitios)
            {
                var usuariosSitio = usuarios.Where(u => u.SitioId == sitio.Id).ToList();
                var pencasSitio = pencas.Where(p => p.SitioId == sitio.Id).ToList();
                var pencaIdsSitio = pencasSitio.Select(p => p.Id).ToHashSet();
                var pagosSitio = pagos.Where(p => pencaIdsSitio.Contains(p.PencaId)).ToList();

                var recaudadoSitio = 0;
                var comisionSitio = 0;

                foreach (var pago in pagosSitio)
                {
                    if (!pencasPorId.TryGetValue(pago.PencaId, out var penca) || penca.Plantilla == null)
                        continue;

                    recaudadoSitio += pago.Monto;
                    comisionSitio += pago.Monto * penca.Plantilla.PorcentajeComision / 100;
                }

                totalRecaudado += recaudadoSitio;
                totalComisiones += comisionSitio;

                detallePorSitio.Add(new EstadisticaSitioDetalleDto
                {
                    SitioId = sitio.Id,
                    NombreSitio = sitio.Nombre,
                    UrlPropia = sitio.UrlPropia,
                    Estado = sitio.Estado,
                    TotalUsuarios = usuariosSitio.Count,
                    UsuariosPendientes = usuariosSitio.Count(u => u.Estado == EstadoUsuario.Pendiente),
                    TotalPencasActivas = pencasSitio.Count(p => p.Estado == EstadoPenca.Abierta || p.Estado == EstadoPenca.EnCurso),
                    TotalPencasFinalizadas = pencasSitio.Count(p => p.Estado == EstadoPenca.Finalizada),
                    TotalInscripciones = pagosSitio.Count,
                    TotalRecaudado = recaudadoSitio,
                    TotalComisiones = comisionSitio
                });
            }

            detallePorSitio = detallePorSitio
                .OrderByDescending(d => d.TotalRecaudado)
                .ThenByDescending(d => d.TotalUsuarios)
                .ToList();

            var topPorUsuarios = detallePorSitio
                .OrderByDescending(d => d.TotalUsuarios)
                .Take(5)
                .Select(d => new EstadisticaSitioResumenDto
                {
                    SitioId = d.SitioId,
                    NombreSitio = d.NombreSitio,
                    Valor = d.TotalUsuarios
                })
                .ToList();

            var topPorRecaudacion = detallePorSitio
                .OrderByDescending(d => d.TotalRecaudado)
                .Take(5)
                .Select(d => new EstadisticaSitioResumenDto
                {
                    SitioId = d.SitioId,
                    NombreSitio = d.NombreSitio,
                    Valor = d.TotalRecaudado
                })
                .ToList();

            return new EstadisticasGlobalesDto
            {
                TotalSitios = sitios.Count,
                TotalUsuarios = usuarios.Count,
                TotalPencasActivas = pencas.Count(p => p.Estado == EstadoPenca.Abierta || p.Estado == EstadoPenca.EnCurso),
                TotalPencasFinalizadas = pencas.Count(p => p.Estado == EstadoPenca.Finalizada),
                TotalRecaudado = totalRecaudado,
                TotalComisionesGeneradas = totalComisiones,
                TopSitiosPorUsuarios = topPorUsuarios,
                TopSitiosPorRecaudacion = topPorRecaudacion,
                EstadisticasPorSitio = detallePorSitio
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
