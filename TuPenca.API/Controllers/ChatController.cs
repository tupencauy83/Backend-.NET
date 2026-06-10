using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuPenca.Infrastructure.Data;

namespace TuPenca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "UsuarioComun,AdministradorSitio")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ChatController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("{pencaId}/mensajes")]
        public async Task<IActionResult> ObtenerMensajes(Guid pencaId)
        {
            var sitioId = User.FindFirst("sitioId")?.Value;
            if (string.IsNullOrEmpty(sitioId))
                return Unauthorized("No se pudo determinar el sitio");

            var pertenecePenca = await _db.Pencas
                .AnyAsync(p => p.Id == pencaId && p.SitioId.ToString() == sitioId);

            if (!pertenecePenca)
                return NotFound("Penca no encontrada para este sitio");

            var mensajes = await _db.MensajesChat
                .Where(m => m.PencaId == pencaId)
                .OrderBy(m => m.FechaCreacion)
                .Take(200)
                .Select(m => new
                {
                    m.Id,
                    m.PencaId,
                    m.UsuarioId,
                    UsuarioNombre = m.Usuario.Nombre,
                    m.Mensaje,
                    m.FechaCreacion
                })
                .ToListAsync();

            return Ok(mensajes);
        }
    }
}
