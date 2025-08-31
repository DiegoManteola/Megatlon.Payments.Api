using Megatlon.Payments.Application.Contracts.Dtos;

namespace Megatlon.Payments.Application.Contracts.Requests
{
    public sealed record RegistrarPagoRequest(
        ClienteDto Cliente,
        decimal Monto,
        DateTime FechaPago,
        string MedioPagoCode,
        string MonedaISOCode,
        string ExternalReference,
        string Source,
        string? CardBin = null // <- opcional
    );
}
