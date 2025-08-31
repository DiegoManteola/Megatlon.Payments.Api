using Megatlon.Payments.Application.Contracts.Requests;

namespace Megatlon.Payments.Application.Rules
{
    public sealed class PaymentContext
    {
        public required RegistrarPagoRequest Request { get; init; }

        public required int MedioPagoId { get; init; }
        public required string MedioPagoCode { get; init; }

        public required int MonedaId { get; init; }
        public required string MonedaISO { get; init; }

        // Extensible: ej. BIN de tarjeta
        public string? CardBin { get; init; }
    }
}
