using Megatlon.Payments.Application.Rules.Interfaces;

namespace Megatlon.Payments.Application.Rules
{
    public sealed class PaymentRuleEngine : IPaymentRuleEngine
    {
        private readonly IEnumerable<IPaymentRule> _rules;

        public PaymentRuleEngine(IEnumerable<IPaymentRule> rules)
            => _rules = rules;

        public async Task<IReadOnlyList<string>> ValidateAsync(PaymentContext ctx, CancellationToken ct = default)
        {
            var errors = new List<string>();

            foreach (var rule in _rules)
            {
                var err = await rule.ValidateAsync(ctx, ct);
                if (!string.IsNullOrWhiteSpace(err))
                    errors.Add(err!);
            }

            return errors;
        }
    }
}
