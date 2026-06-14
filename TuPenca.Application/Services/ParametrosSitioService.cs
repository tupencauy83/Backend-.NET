using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Application.DTOs.ParametrosSitio;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class ParametrosSitioService : IParametrosSitioService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ParametrosSitioService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ParametrosSitioResponseDto> ObtenerAsync(Guid sitioId)
        {
            var parametros = await GetOrCreateAsync(sitioId);
            return ToDto(parametros);
        }

        public async Task<ParametrosSitioResponseDto> ActualizarAsync(Guid sitioId, ActualizarParametrosSitioDto dto)
        {
            var parametros = await GetOrCreateAsync(sitioId);

            if (dto.NotifRecordatorioPrediccion.HasValue)
                parametros.NotifRecordatorioPrediccion = dto.NotifRecordatorioPrediccion.Value;

            if (dto.HorasAntesRecordatorio.HasValue)
            {
                if (dto.HorasAntesRecordatorio.Value < 1 || dto.HorasAntesRecordatorio.Value > 72)
                    throw new Exception("HorasAntesRecordatorio debe estar entre 1 y 72");
                parametros.HorasAntesRecordatorio = dto.HorasAntesRecordatorio.Value;
            }

            if (dto.NotifResumenSemanal.HasValue)
                parametros.NotifResumenSemanal = dto.NotifResumenSemanal.Value;

            if (dto.DiaResumenSemanal.HasValue)
                parametros.DiaResumenSemanal = dto.DiaResumenSemanal.Value;

            if (dto.HoraResumenSemanal.HasValue)
            {
                if (dto.HoraResumenSemanal.Value < 0 || dto.HoraResumenSemanal.Value > 23)
                    throw new Exception("HoraResumenSemanal debe estar entre 0 y 23");
                parametros.HoraResumenSemanal = dto.HoraResumenSemanal.Value;
            }

            if (dto.NotifResultadoPartido.HasValue)
                parametros.NotifResultadoPartido = dto.NotifResultadoPartido.Value;

            await _unitOfWork.ParametrosSitio.UpdateAsync(parametros);
            await _unitOfWork.SaveChangesAsync();

            return ToDto(parametros);
        }

        private async Task<Domain.Entities.ParametrosSitio> GetOrCreateAsync(Guid sitioId)
        {
            var parametros = await _unitOfWork.ParametrosSitio.GetBySitioIdAsync(sitioId);
            if (parametros != null)
                return parametros;

            parametros = new Domain.Entities.ParametrosSitio
            {
                Id = Guid.NewGuid(),
                SitioId = sitioId,
            };

            await _unitOfWork.ParametrosSitio.AddAsync(parametros);
            await _unitOfWork.SaveChangesAsync();

            return parametros;
        }

        private static ParametrosSitioResponseDto ToDto(Domain.Entities.ParametrosSitio p) => new()
        {
            Id = p.Id,
            SitioId = p.SitioId,
            NotifRecordatorioPrediccion = p.NotifRecordatorioPrediccion,
            HorasAntesRecordatorio = p.HorasAntesRecordatorio,
            NotifResumenSemanal = p.NotifResumenSemanal,
            DiaResumenSemanal = p.DiaResumenSemanal,
            HoraResumenSemanal = p.HoraResumenSemanal,
            NotifResultadoPartido = p.NotifResultadoPartido
        };
    }
}