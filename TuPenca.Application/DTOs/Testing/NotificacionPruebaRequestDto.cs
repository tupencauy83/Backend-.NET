using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.Testing
{
    public class NotificacionPruebaRequestDto
    {
        public string FcmToken { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string Cuerpo { get; set; } = null!;
    }
}
