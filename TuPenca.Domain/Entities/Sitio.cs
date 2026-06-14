using TuPenca.Domain.Enums;

namespace TuPenca.Domain.Entities
{
    public class Sitio : BaseEntity
    {
        public string Nombre { get; set; } = null!;
        public string UrlPropia { get; set; } = null!;
        public string ColorPrimario { get; set; } = null!;
        public string ColorSecundario { get; set; } = null!;
        public string? ConfiguracionSitio { get; set; } // JSON con config extra
        public EstadoSitio Estado { get; set; } = EstadoSitio.Pendiente;
        public byte[]? Logo { get; set; } = null;

        public TipoRegistro TipoRegistro { get; set; } = TipoRegistro.Abierta;

        // 1 Sitio → N Usuarios
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();

        // 1 Sitio → N Pencas
        public ICollection<Penca> Pencas { get; set; } = new List<Penca>();

        // 1 Sitio → N Administradores
        // public ICollection<Administrador> Administradores { get; set; } = new List<Administrador>();

        // 1 Sitio → N Invitaciones
        public ICollection<Invitacion> Invitaciones { get; set; } = new List<Invitacion>();

        //Parametros de recordatorios para los sitios
        // 1 Sitio → 1 ParametrosSitio
        public ParametrosSitio? Parametros { get; set; }
    }

}
