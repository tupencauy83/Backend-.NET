using TuPenca.Domain.Enums;

namespace TuPenca.Application.DTOs.Estadisticas
{
    public class EstadisticasSitioDto
    {
        public string NombreSitio { get; set; } = null!;
        public int TotalUsuarios { get; set; }
        public int UsuariosPendientes { get; set; }
        public int TotalPencasActivas { get; set; }
        public int TotalPencasFinalizadas { get; set; }
        public int TotalInscripciones { get; set; }
        public int TotalComisionesGeneradas { get; set; }
        public int TotalRecaudado { get; set; }
        public List<EstadisticaPencaDto> EstadisticasPorPenca { get; set; } = new();
    }

    public class EstadisticaPencaDto
    {
        public Guid PencaId { get; set; }
        public string NombrePenca { get; set; } = null!;
        public EstadoPenca Estado { get; set; }
        public int MontoEntrada { get; set; }
        public string LiderActual { get; set; } = null!;
        public int PuntosLider { get; set; }
        public int TotalParticipantes { get; set; }
        public int TotalPartidosConPrediccion { get; set; }
        public int TotalPredicciones { get; set; }
        public int TotalRecaudado { get; set; }
        public int TotalComision { get; set; }
    }
}
