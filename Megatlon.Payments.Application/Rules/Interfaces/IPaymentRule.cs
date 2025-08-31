namespace Megatlon.Payments.Application.Rules.Interfaces
{
    public interface IPaymentRule
    {
        string Name { get; }
        Task<string?> ValidateAsync(PaymentContext ctx, CancellationToken ct = default);
    }
}