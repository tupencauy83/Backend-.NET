using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Domain.Entities
{
    public class MensajeChat : BaseEntity
    {
        public Guid Id { get; set; }

        public Guid PencaId { get; set; }
        public Guid UsuarioId { get; set; }

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }

        public Penca Penca { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;

    }
}
