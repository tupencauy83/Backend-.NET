using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.SportsApi
{
    public class ResultadoExternoDto
    {
        public int GolesLocal { get; set; }

        public int GolesVisitante { get; set; }

        public bool Finalizado { get; set; }
    }
}
