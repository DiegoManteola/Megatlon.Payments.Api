using Megatlon.Payments.Application.Rules.Interfaces;

namespace Megatlon.Payments.Application.Rules
{
    public sealed class MinMaxAmountRule : IPaymentRule
    {
        private readonly IPaymentRulesConfig _cfg;
        public string Name => nameof(MinMaxAmountRule);

        public MinMaxAmountRule(IPaymentRulesConfig cfg) => _cfg = cfg;

        public Task<string?> ValidateAsync(PaymentContext ctx, CancellationToken ct = default)
        {
            var limits = _cfg.GetLimitsFor(ctx.MedioPagoCode);
            if (limits is null) return Task.FromResult<string?>(null);

            var monto = ctx.Request.Monto;
            if (limits.Value.Min.HasValue && monto < limits.Value.Min.Value)
                return Task.FromResult<string?>($"El monto mínimo para {ctx.MedioPagoCode} es {limits.Value.Min.Value:N0}.");

            if (limits.Value.Max.HasValue && monto > limits.Value.Max.Value)
                return Task.FromResult<string?>($"El monto máximo para {ctx.MedioPagoCode} es {limits.Value.Max.Value:N0}.");

            return Task.FromResult<string?>(null);
        }
    }
}
