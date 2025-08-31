namespace Megatlon.Payments.Application.Rules.Interfaces
{
    public interface IPaymentRuleEngine
    {
        Task<IReadOnlyList<string>> ValidateAsync(PaymentContext ctx, CancellationToken ct = default);
    }
}