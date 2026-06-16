namespace TuPenca.Application.DTOs.Prediccion
{
    public class HistorialPencaResponseDto
    {
        public Guid PencaId { get; set; }
        public string NombrePenca { get; set; } = null!;
        public int PuntosTotales { get; set; }
        public int PartidosPredichos { get; set; }
        public int PartidosConResultado { get; set; }
        public List<HistorialPartidoDto> Partidos { get; set; } = new();
    }

    public class HistorialPartidoDto
    {
        public Guid PartidoId { get; set; }
        public DateTime FechaPartido { get; set; }
        public string Fase { get; set; } = null!;
        public string EquipoLocal { get; set; } = null!;
        public string EquipoVisitante { get; set; } = null!;
        public bool Predijo { get; set; }
        public int? PrediccionLocal { get; set; }
        public int? PrediccionVisitante { get; set; }
        public int? ResultadoLocal { get; set; }
        public int? ResultadoVisitante { get; set; }
        public bool PartidoJugado { get; set; }
        public int PuntosObtenidos { get; set; }
    }
}
