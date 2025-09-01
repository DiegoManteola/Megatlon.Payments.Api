using System.Collections.Generic;
using Megatlon.Payments.Application.Rules.Interfaces;

namespace Megatlon.Payments.Api.Tests
{
    public sealed class TestRulesConfig : IPaymentRulesConfig
    {
        public (decimal? Min, decimal? Max)? GetLimitsFor(string medioPagoCode) =>
            medioPagoCode.ToUpperInvariant() switch
            {
                "CASH" => (3000000000m, null),
                "CARD" => (3000000000m, 20000000000m),
                "CHEQUE" => (3000000000m, 5000000000m),
                _ => null
            };

        public IReadOnlyList<string>? GetAllowedCurrenciesFor(string medioPagoCode) =>
            medioPagoCode.ToUpperInvariant() switch
            {
                "CASH" => new[] { "ARS", "USD", "UYU" },
                "CARD" => new[] { "ARS", "USD" },
                "CHEQUE" => new[] { "ARS" },
                _ => null
            };

        public IReadOnlyList<string>? GetAllowedBinsFor(string medioPagoCode) =>
            medioPagoCode.ToUpperInvariant() switch
            {
                "CARD" => new[] { "411111", "522222" },
                _ => null
            };
    }
}
