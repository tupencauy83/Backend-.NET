using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.Penca;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class PencaService : IPencaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPagoService _pagoService;

        public PencaService(IUnitOfWork unitOfWork, IPagoService pagoService)
        {
            _unitOfWork = unitOfWork;
            _pagoService = pagoService;
        }

        public async Task<IEnumerable<PencaResponseDto>> ObtenerTodasAsync()
        {
            var pencas = await _unitOfWork.Pencas.GetAllConDetalleAsync();
            return pencas.Select(p => new PencaResponseDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Estado = p.Estado,
                PlantillaNombre = p.Plantilla?.Nombre ?? string.Empty,
                EventoDeportivo = p.Plantilla?.Evento?.Nombre ?? string.Empty,
                EventoDeportivoId = p.Plantilla?.EventoDeportivoId,
                MontoEntrada = p.Plantilla?.MontoEntrada ?? 0
            });
        }

        public async Task<PencaResponseDto?> ObtenerPorIdAsync(Guid id)
        {
            var penca = await _unitOfWork.Pencas.GetByIdConDetalleAsync(id);
            if (penca == null) return null;

            return new PencaResponseDto
            {
                Id = penca.Id,
                Nombre = penca.Nombre,
                Estado = penca.Estado,
                PlantillaNombre = penca.Plantilla?.Nombre ?? string.Empty,
                EventoDeportivo = penca.Plantilla?.Evento?.Nombre ?? string.Empty,
                EventoDeportivoId = penca.Plantilla?.EventoDeportivoId,
                MontoEntrada = penca.Plantilla?.MontoEntrada ?? 0
            };
        }

        public async Task<PencaResponseDto> CrearAsync(PencaRequestDto dto, Guid sitioId)
        {
            var plantilla = await _unitOfWork.PlantillasPenca.GetByIdConDetalleAsync(dto.PlantillaPencaId);
            if (plantilla == null)
                throw new Exception("Plantilla no encontrada");

            if (plantilla.Evento == null)
                throw new Exception("La plantilla no tiene un evento deportivo asociado");

            if (DateTime.UtcNow >= plantilla.Evento.FechaInicio)
                throw new Exception("No se puede crear la penca: la competición ya comenzó. Solo se permiten instancias previas al inicio del evento.");

            // Validación individual
            if (dto.PorcentajePremio1 < 0 || dto.PorcentajePremio2 < 0 || dto.PorcentajePremio3 < 0)
                throw new Exception("Los porcentajes de premios no pueden ser negativos");

            var porcentajeDisponible = 100 - plantilla.PorcentajeComision;
            var sumaPorcentajes = dto.PorcentajePremio1 + dto.PorcentajePremio2 + dto.PorcentajePremio3;

            if (sumaPorcentajes > porcentajeDisponible)
                throw new Exception($"La suma de los porcentajes ({sumaPorcentajes}%) supera el {porcentajeDisponible}% disponible tras descontar la comisión");

            if (sumaPorcentajes < porcentajeDisponible)
                throw new Exception($"La suma de los porcentajes ({sumaPorcentajes}%) debe ser exactamente {porcentajeDisponible}% — no puede quedar pozo sin asignar");


            var penca = new Penca
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Estado = EstadoPenca.Abierta,
                PlantillaPencaId = dto.PlantillaPencaId,
                SitioId = sitioId,
                PorcentajePremio1 = dto.PorcentajePremio1,
                PorcentajePremio2 = dto.PorcentajePremio2,
                PorcentajePremio3 = dto.PorcentajePremio3
            };

            await _unitOfWork.Pencas.AddAsync(penca);
            await _unitOfWork.SaveChangesAsync();

            return new PencaResponseDto
            {
                Id = penca.Id,
                Nombre = penca.Nombre,
                Estado = penca.Estado,
                PlantillaNombre = plantilla.Nombre,
                EventoDeportivo = plantilla.Evento?.Nombre ?? string.Empty,
                MontoEntrada = plantilla.MontoEntrada  // ❌ faltaba esto
            };
        }

        public async Task<PencaResponseDto> CambiarEstadoAsync(Guid id, EstadoPenca nuevoEstado)
        {
            var penca = await _unitOfWork.Pencas.GetByIdConDetalleAsync(id);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            // Si se está finalizando, calcular ganadores
            if (nuevoEstado == EstadoPenca.Finalizada)
                await CerrarPencaAsync(penca);

            penca.Estado = nuevoEstado;
            await _unitOfWork.Pencas.UpdateAsync(penca);
            await _unitOfWork.SaveChangesAsync();

            return new PencaResponseDto
            {
                Id = penca.Id,
                Nombre = penca.Nombre,
                Estado = penca.Estado,
                PlantillaNombre = penca.Plantilla?.Nombre ?? string.Empty,
                EventoDeportivo = penca.Plantilla?.Evento?.Nombre ?? string.Empty
            };
        }

        public async Task EliminarAsync(Guid id)
        {
            var penca = await _unitOfWork.Pencas.GetByIdAsync(id);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            if (penca.Estado == EstadoPenca.EnCurso)
                throw new Exception("No se puede eliminar una penca que está en curso");

            await _unitOfWork.Pencas.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<TablaPosicionesDto> ObtenerTablaPosicionesAsync(Guid pencaId, Guid usuarioId, string rol)
        {
            var penca = await _unitOfWork.Pencas.GetByIdAsync(pencaId);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            // Verificar acceso si es usuario común
            if (rol == "UsuarioComun")
            {
                var tienePago = await _pagoService.UsuarioPagoEnPencaAsync(usuarioId, pencaId);
                if (!tienePago)
                    throw new Exception("Debes estar inscripto en la penca para ver la tabla de posiciones");
            }

            // Traer todos los puntajes de la penca
            var puntajes = await _unitOfWork.PuntajesUsuario.GetByPencaAsync(pencaId);

            // Agrupar por usuario y sumar puntos
            var agrupados = puntajes
                .GroupBy(p => new { p.UsuarioId, p.Usuario.Nombre })
                .Select(g => new PosicionUsuarioDto
                {
                    UsuarioId = g.Key.UsuarioId,
                    NombreUsuario = g.Key.Nombre,
                    PuntosTotales = g.Sum(p => p.PuntosPartido),
                    PartidosPredichos = g.Count()
                })
                .OrderByDescending(p => p.PuntosTotales)
                .ToList();

            // Asignar posiciones
            for (int i = 0; i < agrupados.Count; i++)
                agrupados[i].Posicion = i + 1;

            return new TablaPosicionesDto
            {
                PencaId = pencaId,
                NombrePenca = penca.Nombre,
                Posiciones = agrupados
            };
        }


        private async Task CerrarPencaAsync(Penca penca)
        {
            // 1. Obtener tabla de posiciones
            var puntajes = await _unitOfWork.PuntajesUsuario.GetByPencaAsync(penca.Id);

            var posiciones = puntajes
                .GroupBy(p => new { p.UsuarioId, p.Usuario.Nombre })
                .Select(g => new
                {
                    UsuarioId = g.Key.UsuarioId,
                    PuntosTotales = g.Sum(p => p.PuntosPartido)
                })
                .OrderByDescending(p => p.PuntosTotales)
                .ToList();

            if (!posiciones.Any())
                return;

            // 2. Calcular pozo total
            var pagos = (await _unitOfWork.Pagos.GetAllAsync())
                .Where(p => p.PencaId == penca.Id && p.Estado == EstadoPago.Aprobado)
                .ToList();
            var pozoTotal = pagos.Sum(p => p.Monto);

            // 3. Obtener porcentajes de la plantilla y penca
            var plantilla = await _unitOfWork.PlantillasPenca.GetByIdConDetalleAsync(penca.PlantillaPencaId);
            if (plantilla == null) return;

            // 4. Calcular montos de premios
            var montoPremio1 = (int)(pozoTotal * penca.PorcentajePremio1 / 100m);
            var montoPremio2 = (int)(pozoTotal * penca.PorcentajePremio2 / 100m);
            var montoPremio3 = (int)(pozoTotal * penca.PorcentajePremio3 / 100m);

            // 5. Crear o actualizar premios con ganadores
            var premiosExistentes = await _unitOfWork.Premios.GetByPencaAsync(penca.Id);

            var datosPremios = new[]
            {
        new { Posicion = 1, Monto = montoPremio1, UsuarioId = posiciones.ElementAtOrDefault(0)?.UsuarioId },
        new { Posicion = 2, Monto = montoPremio2, UsuarioId = posiciones.ElementAtOrDefault(1)?.UsuarioId },
        new { Posicion = 3, Monto = montoPremio3, UsuarioId = posiciones.ElementAtOrDefault(2)?.UsuarioId }
    };

            foreach (var dato in datosPremios)
            {
                if (dato.UsuarioId == null) continue;

                var premioExistente = premiosExistentes.FirstOrDefault(p => p.Posicion == dato.Posicion);
                if (premioExistente != null)
                {
                    premioExistente.UsuarioGanadorId = dato.UsuarioId;
                    premioExistente.Monto = dato.Monto;
                    await _unitOfWork.Premios.UpdateAsync(premioExistente);
                }
                else
                {
                    var premio = new Premio
                    {
                        Id = Guid.NewGuid(),
                        PencaId = penca.Id,
                        Posicion = dato.Posicion,
                        Monto = dato.Monto,
                        Descripcion = $"{dato.Posicion}er lugar",
                        UsuarioGanadorId = dato.UsuarioId
                    };
                    await _unitOfWork.Premios.AddAsync(premio);
                }
            }
        }

        public async Task<IEnumerable<PremioResponseDto>> ObtenerGanadoresAsync(Guid pencaId)
        {
            var penca = await _unitOfWork.Pencas.GetByIdAsync(pencaId);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            if (penca.Estado != EstadoPenca.Finalizada)
                throw new Exception("La penca todavía no finalizó");

            var premios = await _unitOfWork.Premios.GetByPencaAsync(pencaId);

            return premios
                .Where(p => p.UsuarioGanadorId != null)
                .Select(p => new PremioResponseDto
                {
                    Posicion = p.Posicion,
                    NombreUsuario = p.UsuarioGanador?.Nombre ?? string.Empty,
                    Monto = p.Monto,
                    Descripcion = p.Descripcion
                });
        }


        // EDITAR VALOR % DE PREMIOS EN LA PENCA PARA CADA GANADOR


        public async Task<PencaEditPremioDto> EditarPremiosAsync(Guid PencaId, PencaEditPremioDto dto)
        {

            var penca = await _unitOfWork.Pencas.GetByIdAsync(PencaId);
            if (penca == null)
                throw new Exception("Penca no encontrada");


            var plantilla = await _unitOfWork.PlantillasPenca.GetByIdConDetalleAsync(penca.PlantillaPencaId);
            if (plantilla == null)
                throw new Exception("Plantilla no encontrada");


            // Validación individual
            if (dto.PorcentajePremio1 < 0 || dto.PorcentajePremio2 < 0 || dto.PorcentajePremio3 < 0)
                throw new Exception("Los porcentajes de premios no pueden ser negativos");

            var porcentajeDisponible = 100 - plantilla.PorcentajeComision;
            var sumaPorcentajes = dto.PorcentajePremio1 + dto.PorcentajePremio2 + dto.PorcentajePremio3;

            if (sumaPorcentajes > porcentajeDisponible)
                throw new Exception($"La suma de los porcentajes ({sumaPorcentajes}%) supera el {porcentajeDisponible}% disponible tras descontar la comisión");

            if (sumaPorcentajes < porcentajeDisponible)
                throw new Exception($"La suma de los porcentajes ({sumaPorcentajes}%) debe ser exactamente {porcentajeDisponible}% — no puede quedar pozo sin asignar");


                penca.PorcentajePremio1 = dto.PorcentajePremio1;
                penca.PorcentajePremio2 = dto.PorcentajePremio2;
                penca.PorcentajePremio3 = dto.PorcentajePremio3;
       

            await _unitOfWork.SaveChangesAsync();

            return new PencaEditPremioDto
            {
                PorcentajePremio1 = penca.PorcentajePremio1,
                PorcentajePremio2 = penca.PorcentajePremio2,
                PorcentajePremio3 = penca.PorcentajePremio3

            };
        }

    }
}