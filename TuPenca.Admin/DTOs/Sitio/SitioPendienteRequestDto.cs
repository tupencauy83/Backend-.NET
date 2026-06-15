using TuPenca.Admin.Models.Enums;

namespace TuPenca.Admin.DTOs.Sitio
{
    public class SitioPendienteRequestDto
    {
        public string NombreUsuario { get; set; } = null!;
        public string Email { get; set; } = null!; // Email de usuario
        public string Nombre { get; set; } = null!;
        public string UrlPropia { get; set; } = null!;
        public string ColorPrimario { get; set; } = null!;
        public string ColorSecundario { get; set; } = null!;
        public string? ConfiguracionSitio { get; set; } // JSON con config extra
        public TipoRegistro TipoRegistro { get; set; } = TipoRegistro.Con_Autorizacion;
        public byte[]? Logo { get; set; } = null;
    }
}
