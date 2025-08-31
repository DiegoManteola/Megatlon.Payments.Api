using Microsoft.Extensions.Configuration;
using Megatlon.Payments.Application.Rules.Interfaces;

namespace Megatlon.Payments.Api.Rules
{
    public sealed class PaymentRulesConfig : IPaymentRulesConfig
    {
        private readonly IConfiguration _cfg;
        public PaymentRulesConfig(IConfiguration cfg) => _cfg = cfg;

        public (decimal? Min, decimal? Max)? GetLimitsFor(string code)
        {
            var sec = _cfg.GetSection($"PaymentRules:{code}");
            if (!sec.Exists()) return null;
            return (sec.GetValue<decimal?>("Min"), sec.GetValue<decimal?>("Max"));
        }

        public IReadOnlyList<string>? GetAllowedCurrenciesFor(string code)
            => _cfg.GetSection($"PaymentRules:{code}:AllowedCurrencies").Get<string[]>();

        public IReadOnlyList<string>? GetAllowedBinsFor(string code)
            => _cfg.GetSection($"PaymentRules:{code}:AllowedBins").Get<string[]>();
    }
}
