using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Admin.DTOs.Estadisticas
{
    public class EstadisticasGlobalesDto
    {
        public int TotalSitios { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalPencasActivas { get; set; }
        public int TotalPencasFinalizadas { get; set; }
        public int TotalRecaudado { get; set; }
        public int TotalComisionesGeneradas { get; set; }
        public List<EstadisticaSitioResumenDto> TopSitiosPorUsuarios { get; set; } = new();
        public List<EstadisticaSitioResumenDto> TopSitiosPorRecaudacion { get; set; } = new();
    }

    public class EstadisticaSitioResumenDto
    {
        public string NombreSitio { get; set; } = null!;
        public int Valor { get; set; }
    }
}