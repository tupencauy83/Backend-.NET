using TuPenca.Application.DTOs.Invitacion;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace TuPenca.Application.Services
{
    public class InvitacionService : IInvitacionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<InvitacionService> _logger;

        public InvitacionService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<InvitacionService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<InvitacionResponseDto> GenerarInvitacionAsync(
            InvitacionRequestDto dto, Guid usuarioId, Guid sitioId)
        {
            var sitio = await _unitOfWork.Sitios.GetByIdAsync(sitioId);
            if (sitio == null)
                throw new Exception("Sitio no encontrado");

            if (sitio.TipoRegistro != TipoRegistro.Con_Invitacion)
                throw new Exception("Este sitio no usa invitaciones");

            // Check for existing pending invitation
            var invitaciones = await _unitOfWork.Invitaciones.GetAllAsync();
            var invitacionExistente = invitaciones.FirstOrDefault(i =>
                i.EmailInvitado == dto.EmailInvitado &&
                i.SitioId == sitioId &&
                !i.Aceptada);

            if (invitacionExistente != null)
                throw new Exception("Ya existe una invitación pendiente para ese email");

            var invitacion = new Invitacion
            {
                Id = Guid.NewGuid(),
                EmailInvitado = dto.EmailInvitado,
                Codigo = Guid.NewGuid().ToString("N")[..8].ToUpper(),
                Aceptada = false,
                SitioId = sitioId,
                UsuarioId = usuarioId
            };

            await _unitOfWork.Invitaciones.AddAsync(invitacion);
            await _unitOfWork.SaveChangesAsync();

            // Send email — non-blocking: a failed email shouldn't roll back the invitation
            try
            {
                await _emailService.EnviarAsync(
                    destinatario: invitacion.EmailInvitado,
                    asunto: $"Fuiste invitado a {sitio.Nombre}",
                    cuerpo: BuildInvitacionEmail(sitio.Nombre, invitacion.Codigo)
                );
            }
            catch (Exception ex)
            {
                // Log but don't throw — invitation is already saved, admin can resend manually
                _logger.LogError(ex,
                    "No se pudo enviar email de invitación a {Email} para sitio {SitioId}",
                    invitacion.EmailInvitado, sitioId);
            }

            return new InvitacionResponseDto
            {
                Id = invitacion.Id,
                EmailInvitado = invitacion.EmailInvitado,
                Codigo = invitacion.Codigo,
                Aceptada = invitacion.Aceptada
            };
        }

        public async Task<IEnumerable<InvitacionResponseDto>> ObtenerInvitacionesSitioAsync(Guid sitioId)
        {
            var invitaciones = await _unitOfWork.Invitaciones.GetAllAsync();
            return invitaciones
                .Where(i => i.SitioId == sitioId)
                .Select(i => new InvitacionResponseDto
                {
                    Id = i.Id,
                    EmailInvitado = i.EmailInvitado,
                    Codigo = i.Codigo,
                    Aceptada = i.Aceptada
                });
        }

        private static string BuildInvitacionEmail(string sitioNombre, string codigo) => $"""
            <div style="font-family: sans-serif; max-width: 480px; margin: auto;">
                <h2>Fuiste invitado a unirte a <strong>{sitioNombre}</strong></h2>
                <p>Alguien te invitó a participar en la plataforma <strong>TuPenca</strong>.</p>
                <p>Usá el siguiente código al registrarte:</p>
                <div style="
                    font-size: 2rem;
                    font-weight: bold;
                    letter-spacing: 0.3rem;
                    text-align: center;
                    background: #f4f4f4;
                    padding: 1rem;
                    border-radius: 8px;
                    margin: 1.5rem 0;
                ">
                    {codigo}
                </div>
                <p style="color: #666; font-size: 0.85rem;">
                    Una vez registrado, tu cuenta quedará pendiente de aprobación 
                    por un administrador del sitio.
                </p>
            </div>
        """;
    }
}