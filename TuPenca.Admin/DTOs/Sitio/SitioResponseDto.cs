namespace TuPenca.Admin.DTOs.Sitio
{
    public class SitioResponseDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
    }
}
