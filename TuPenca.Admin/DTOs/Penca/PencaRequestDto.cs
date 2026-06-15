using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Penca
{
    public class PencaRequestDto
    {
        public string Nombre { get; set; } = null!;
        public Guid PlantillaPencaId { get; set; }

        public int PorcentajePremio1 { get; set; }
        public int PorcentajePremio2 { get; set; }
        public int PorcentajePremio3 { get; set; }
    }
}