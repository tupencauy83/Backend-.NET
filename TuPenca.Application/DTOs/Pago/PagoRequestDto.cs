using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.DTOs.Pago
{
    public class PagoRequestDto
    {
        public Guid PencaId { get; set; }

        /// <summary>
        /// Origen del frontend desde el que se inició el pago (ej. https://parlamento.tupencauy.lat).
        /// Se valida contra el sitio de la penca antes de usarlo como URL de retorno en Stripe.
        /// </summary>
        public string? ReturnOrigin { get; set; }
    }
}