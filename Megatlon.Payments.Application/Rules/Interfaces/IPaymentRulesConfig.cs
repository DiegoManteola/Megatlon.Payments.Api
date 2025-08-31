namespace Megatlon.Payments.Application.Rules.Interfaces
{
    public interface IPaymentRulesConfig
    {
        (decimal? Min, decimal? Max)? GetLimitsFor(string medioPagoCode);
        IReadOnlyList<string>? GetAllowedCurrenciesFor(string medioPagoCode);
        IReadOnlyList<string>? GetAllowedBinsFor(string medioPagoCode);
    }
}
