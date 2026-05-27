using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Resource.Payment;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
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
        private readonly IConfiguration _config;

        public PagoService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
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
                Monto = monto, // 👈
                Fecha = DateTime.UtcNow,
                Estado = EstadoPago.Pendiente,
                UsuarioId = usuarioId,
                PencaId = dto.PencaId
            };

            await _unitOfWork.Pagos.AddAsync(pago);
            await _unitOfWork.SaveChangesAsync();

            // Generar preferencia de pago en MercadoPago
            var baseUrl = _config["App:BaseUrl"]; // ej: "https://tuapp.com"

            var preferenceRequest = new PreferenceRequest
            {
                Items = new List<PreferenceItemRequest>
            {
                new PreferenceItemRequest
                {
                    Title = $"Inscripción Penca - {penca.Nombre}",
                    Quantity = 1,
                    CurrencyId = "UYU", // 👈 pesos uruguayos, cambiá si es otro
                    UnitPrice = monto,
                }
            },
                // El ExternalReference vincula el pago de MP con tu pago interno
                ExternalReference = pago.Id.ToString(),
                BackUrls = new PreferenceBackUrlsRequest
                {
                    Success = $"{baseUrl}/pago/exito",
                    Failure = $"{baseUrl}/pago/error",
                    Pending = $"{baseUrl}/pago/pendiente",
                },
                AutoReturn = "approved",
                NotificationUrl = $"{baseUrl}/api/webhook/mercadopago",
            };

            var client = new PreferenceClient();
            var preference = await client.CreateAsync(preferenceRequest);

            return new PagoResponseDto
            {
                Id = pago.Id,
                PencaId = pago.PencaId,
                UsuarioId = pago.UsuarioId,
                Monto = pago.Monto,
                Estado = pago.Estado,
                Fecha = pago.Fecha,
                LinkPago = preference.InitPoint, // 👈 link para redirigir al usuario
                PreferenceId = preference.Id      // 👈 útil para el frontend con Checkout Pro
            };
        }

        public async Task ProcesarWebhookAsync(string pagoMpId)
        {
            // Consultar el pago en la API de MP para obtener el ExternalReference
            var client = new PaymentClient();
            var pagoMp = await client.GetAsync(long.Parse(pagoMpId));

            if (pagoMp == null)
                throw new Exception("Pago no encontrado en MercadoPago");

            // El ExternalReference es el Id de tu pago interno
            if (!Guid.TryParse(pagoMp.ExternalReference, out var pagoId))
                return;

            var pago = await _unitOfWork.Pagos.GetByIdAsync(pagoId);
            if (pago == null)
                return;

            // Solo actualizar si el estado de MP es "approved"
            if (pagoMp.Status == "approved" && pago.Estado != EstadoPago.Aprobado)
            {
                pago.Estado = EstadoPago.Aprobado;
                pago.Fecha = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(); // EF ya sabe que pago cambió
            }
            else if (pagoMp.Status == "rejected")
            {
                pago.Estado = EstadoPago.Rechazado;
                await _unitOfWork.SaveChangesAsync();
            }}

        public async Task<bool> UsuarioPagoEnPencaAsync(Guid usuarioId, Guid pencaId)
        {
            var pagos = await _unitOfWork.Pagos.GetAllAsync();
            return pagos.Any(p =>
                p.UsuarioId == usuarioId &&
                p.PencaId == pencaId &&
                p.Estado == EstadoPago.Aprobado);
        }
    }
}