using Stripe;
using Stripe.Checkout;
using TuPenca.Application.Common;
using TuPenca.Application.DTOs.Pago;
using TuPenca.Application.Interfaces.Services;
using TuPenca.Domain.Entities;
using TuPenca.Domain.Enums;
using TuPenca.Domain.Interfaces;

namespace TuPenca.Application.Services
{
    public class PagoService : IPagoService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PagoService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagoResponseDto> RealizarPagoAsync(PagoRequestDto dto, Guid usuarioId)
        {
            var penca = await _unitOfWork.Pencas.GetByIdAsync(dto.PencaId);
            if (penca == null)
                throw new Exception("Penca no encontrada");

            if (penca.Estado != EstadoPenca.Abierta)
                throw new Exception("La penca no está abierta para nuevos participantes");

            var plantilla = await _unitOfWork.PlantillasPenca.GetByIdAsync(penca.PlantillaPencaId);
            if (plantilla == null)
                throw new Exception("Plantilla de la penca no encontrada");

            int monto = plantilla.MontoEntrada;

            var pagos = await _unitOfWork.Pagos.GetAllAsync();
            var pagoExistente = pagos.FirstOrDefault(p =>
                p.UsuarioId == usuarioId &&
                p.PencaId == dto.PencaId &&
                p.Estado == EstadoPago.Aprobado);

            if (pagoExistente != null)
                throw new Exception("Ya estás inscripto en esta penca");

            // Crear pago en estado Pendiente
            var pago = new Pago
            {
                Id = Guid.NewGuid(),
                Monto = monto,
                Fecha = DateTime.UtcNow,
                Estado = EstadoPago.Pendiente,
                UsuarioId = usuarioId,
                PencaId = dto.PencaId
            };

            await _unitOfWork.Pagos.AddAsync(pago);
            await _unitOfWork.SaveChangesAsync();

            var sitio = await _unitOfWork.Sitios.GetByIdAsync(penca.SitioId);
            if (sitio == null)
                throw new Exception("Sitio de la penca no encontrado");

            var frontendOrigin = SitioUrlHelper.ResolverOriginFrontend(dto.ReturnOrigin, sitio.UrlPropia);
            var successUrl = $"{frontendOrigin.TrimEnd('/')}/pago/exito?pencaId={dto.PencaId}";
            var cancelUrl = $"{frontendOrigin.TrimEnd('/')}/pago/error";

            // Crear Stripe Checkout Session
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd", // Stripe no soporta UYU, usamos USD para pruebas
                            UnitAmount = monto * 100, // Stripe maneja centavos
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Inscripción Penca - {penca.Nombre}",
                            },
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                // Vinculamos el pago interno con la session de Stripe
                Metadata = new Dictionary<string, string>
                {
                    { "pagoId", pago.Id.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new PagoResponseDto
            {
                Id = pago.Id,
                PencaId = pago.PencaId,
                UsuarioId = pago.UsuarioId,
                Monto = pago.Monto,
                Estado = pago.Estado,
                Fecha = pago.Fecha,
                LinkPago = session.Url,       // 👈 link para redirigir al usuario
                PreferenceId = session.Id     // 👈 session id de Stripe
            };
        }

        public async Task ProcesarWebhookAsync(string pagoId, string nuevoEstado)
        {
            if (!Guid.TryParse(pagoId, out var pagoGuid))
                return;

            var pago = await _unitOfWork.Pagos.GetByIdAsync(pagoGuid);
            if (pago == null)
                return;

            if (nuevoEstado == "approved" && pago.Estado != EstadoPago.Aprobado)
            {
                pago.Estado = EstadoPago.Aprobado;
                pago.Fecha = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
            else if (nuevoEstado == "rejected")
            {
                pago.Estado = EstadoPago.Rechazado;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> UsuarioPagoEnPencaAsync(Guid usuarioId, Guid pencaId)
        {
            var pagos = await _unitOfWork.Pagos.GetAllAsync();
            return pagos.Any(p =>
                p.UsuarioId == usuarioId &&
                p.PencaId == pencaId &&
                p.Estado == EstadoPago.Aprobado);
        }


        public async Task<List<Guid>> ObtenerInscripcionesAsync(Guid usuarioId)
        {
            var pagos = await _unitOfWork.Pagos.GetAllAsync();
            return pagos
                .Where(p => p.UsuarioId == usuarioId && p.Estado == EstadoPago.Aprobado)
                .Select(p => p.PencaId)
                .Distinct()
                .ToList();
        }

    }
}