using System;
using System.Collections.Generic;
using System.Text;

using TuPenca.Domain.Enums;

namespace TuPenca.Application.DTOs.Pago
{
    public class PagoResponseDto
    {
        public Guid Id { get; set; }
        public Guid PencaId { get; set; }
        public Guid UsuarioId { get; set; }
        public int Monto { get; set; }
        public EstadoPago Estado { get; set; }
        public DateTime Fecha { get; set; }
        public string? LinkPago { get; set; }
        public string? PreferenceId { get; set; } 
    }
}