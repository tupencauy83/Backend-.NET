using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.DTOs.Penca
{
    public class CambiarEstadoPencaDto
    {
        public Guid PencaId { get; set; }
        public EstadoPenca NuevoEstado { get; set; }
    }
}
