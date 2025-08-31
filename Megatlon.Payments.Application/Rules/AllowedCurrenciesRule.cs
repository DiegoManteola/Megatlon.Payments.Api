using Megatlon.Payments.Application.Rules.Interfaces;

namespace Megatlon.Payments.Application.Rules
{
    public sealed class AllowedCurrenciesRule : IPaymentRule
    {
        private readonly IPaymentRulesConfig _cfg;
        public string Name => nameof(AllowedCurrenciesRule);

        public AllowedCurrenciesRule(IPaymentRulesConfig cfg) => _cfg = cfg;

        public Task<string?> ValidateAsync(PaymentContext ctx, CancellationToken ct = default)
        {
            var allowed = _cfg.GetAllowedCurrenciesFor(ctx.MedioPagoCode);
            if (allowed is null || allowed.Count == 0) return Task.FromResult<string?>(null);

            if (!allowed.Contains(ctx.MonedaISO, StringComparer.OrdinalIgnoreCase))
                return Task.FromResult<string?>($"La moneda {ctx.MonedaISO} no está habilitada para {ctx.MedioPagoCode}.");

            return Task.FromResult<string?>(null);
        }
    }
}
