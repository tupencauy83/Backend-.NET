using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.DTOs.Penca
{
    public class PencaResponseDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public EstadoPenca Estado { get; set; }
        public string PlantillaNombre { get; set; } = null!;
        public string EventoDeportivo { get; set; } = null!;
        public Guid? EventoDeportivoId { get; set; }
        public int MontoEntrada { get; set; }
        public string SitioNombre { get; set; } = string.Empty;
        public string SitioUrl { get; set; } = string.Empty;
    }
}