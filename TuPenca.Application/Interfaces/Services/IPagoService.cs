using System;
using System.Collections.Generic;
using System.Text;

using TuPenca.Application.DTOs.Pago;

namespace TuPenca.Application.Interfaces.Services
{
    public interface IPagoService
    {
        Task<PagoResponseDto> RealizarPagoAsync(PagoRequestDto dto, Guid usuarioId);
        Task<bool> UsuarioPagoEnPencaAsync(Guid usuarioId, Guid pencaId);
        Task ProcesarWebhookAsync(string pagoMpId); // MercadoPago
    }
}