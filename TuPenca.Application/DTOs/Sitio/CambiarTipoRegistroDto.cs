using System;
using System.Collections.Generic;
using System.Text;
using TuPenca.Domain.Enums;

namespace TuPenca.Application.DTOs.Sitio
{
    public record CambiarTipoRegistroDto(Guid SitioId, TipoRegistro TipoRegistro);
}