using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TuPenca.Domain.Entities;

namespace TuPenca.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _db;

        public ChatHub(AppDbContext db)
        {
            _db = db;
        }

        public async Task UnirseAPenca(string pencaId)
        {
            if (!Guid.TryParse(pencaId, out var pencaGuid))
                throw new HubException("Penca inválida.");

            var sitioId = Context.User?.FindFirst("sitioId")?.Value;

            if (string.IsNullOrEmpty(sitioId))
                throw new HubException("Sitio no identificado.");

            var pertenecePenca = await _db.Pencas
                .AnyAsync(p => p.Id == pencaGuid && p.SitioId.ToString() == sitioId);

            if (!pertenecePenca)
                throw new HubException("La penca no pertenece al sitio del usuario.");

            var groupName = ObtenerNombreGrupo(pencaGuid);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SalirDePenca(string pencaId)
        {
            if (!Guid.TryParse(pencaId, out var pencaGuid))
                throw new HubException("Penca inválida.");

            var groupName = ObtenerNombreGrupo(pencaGuid);

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                groupName
            );
        }

        public async Task EnviarMensaje(string pencaId, string mensaje)
        {
            if (!Guid.TryParse(pencaId, out var pencaGuid))
                throw new HubException("Penca inválida.");

            if (string.IsNullOrWhiteSpace(mensaje))
                throw new HubException("El mensaje no puede estar vacío.");

            var user = Context.User;

            var sitioId = user?.FindFirst("sitioId")?.Value;

            var usuarioIdClaim =
                user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user?.FindFirst("sub")?.Value;

            if (!Guid.TryParse(usuarioIdClaim, out var usuarioId))
                throw new HubException("Usuario no identificado.");

            if (string.IsNullOrEmpty(sitioId))
                throw new HubException("Sitio no identificado.");

            var penca = await _db.Pencas
                .FirstOrDefaultAsync(p =>
                    p.Id == pencaGuid &&
                    p.SitioId.ToString() == sitioId);

            if (penca == null)
                throw new HubException("Penca no encontrada para este sitio.");

            var chatMessage = new MensajeChat
            {
                Id = Guid.NewGuid(),
                PencaId = pencaGuid,
                UsuarioId = usuarioId,
                Mensaje = mensaje.Trim(),
                FechaCreacion = DateTime.UtcNow
            };

            _db.MensajesChat.Add(chatMessage);
            await _db.SaveChangesAsync();

            var usuario = await _db.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => new
                {
                    u.Id,
                    u.Nombre
                })
                .FirstOrDefaultAsync();

            var groupName = ObtenerNombreGrupo(pencaGuid);

            await Clients.Group(groupName).SendAsync("RecibirMensaje", new
            {
                Id = chatMessage.Id,
                PencaId = chatMessage.PencaId,
                UsuarioId = chatMessage.UsuarioId,
                UsuarioNombre = usuario?.Nombre ?? "Usuario",
                Mensaje = chatMessage.Mensaje,
                FechaCreacion = chatMessage.FechaCreacion
            });
        }

        private static string ObtenerNombreGrupo(Guid pencaId)
        {
            return $"penca-{pencaId}";
        }
    }
}
