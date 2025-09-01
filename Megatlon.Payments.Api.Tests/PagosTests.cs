using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace Megatlon.Payments.Api.Tests
{
    public sealed class PagosTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client;

        public PagosTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HappyPath_Cash_Ars_MinimoAceptado()
        {
            var body = """
            {
              "cliente": { "nombre": "Juan Perez", "email": "juan@megatlon.com" },
              "monto": 3000000000,
              "fechaPago": "2025-08-31T15:00:00Z",
              "medioPagoCode": "CASH",
              "monedaISOCode": "ARS",
              "externalReference": "op-100",
              "source": "megatlon"
            }
            """;

            var resp = await _client.PostAsync("/pagos", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var dto = await resp.Content.ReadFromJsonAsync<ApiResponse<PagoIdDto>>(Json.Options);
            dto!.Success.Should().BeTrue();
            dto.Data!.PagoId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task FallaPorMinimo_Cash()
        {
            var body = """
            {
              "cliente": { "nombre": "Ana Lopez", "email": "ana@megatlon.com" },
              "monto": 2999999999,
              "fechaPago": "2025-08-31T15:00:00Z",
              "medioPagoCode": "CASH",
              "monedaISOCode": "ARS",
              "externalReference": "op-101",
              "source": "megatlon"
            }
            """;

            var resp = await _client.PostAsync("/pagos", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var dto = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>(Json.Options);
            dto!.Success.Should().BeFalse();
            dto!.Errors!.Should().Contain(e =>
            e.Contains("mayor o igual", StringComparison.OrdinalIgnoreCase) &&
            e.Contains("3.000.000.000"));
        }

        [Fact]
        public async Task FallaPorMaximo_Card()
        {
            var body = """
            {
              "cliente": { "nombre": "Laura Gomez", "email": "laura@mail.com" },
              "monto": 25000000000,
              "fechaPago": "2025-08-31T15:00:00Z",
              "medioPagoCode": "CARD",
              "monedaISOCode": "USD",
              "externalReference": "op-102",
              "source": "megatlon",
              "cardBin": "411111"
            }
            """;

            var resp = await _client.PostAsync("/pagos", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var dto = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>(Json.Options);
            dto!.Errors!.Should().Contain(e => e.Contains("máximo", System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task FallaPorMonedaNoHabilitada_ChequeUsd()
        {
            var body = """
            {
              "cliente": { "nombre": "Diego Manteola", "email": "diego@mail.com" },
              "monto": 3500000000,
              "fechaPago": "2025-08-31T15:00:00Z",
              "medioPagoCode": "CHEQUE",
              "monedaISOCode": "USD",
              "externalReference": "op-103",
              "source": "megatlon"
            }
            """;

            var resp = await _client.PostAsync("/pagos", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var dto = await resp.Content.ReadFromJsonAsync<ApiResponse<object>>(Json.Options);
            dto!.Errors!.Should().Contain(e => e.Contains("no está habilitada", System.StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Idempotencia_MismoSourceYExternalReference()
        {
            var body = """
            {
              "cliente": { "nombre": "Juan Perez", "email": "juan@megatlon.com" },
              "monto": 3000000000,
              "fechaPago": "2025-08-31T15:00:00Z",
              "medioPagoCode": "CASH",
              "monedaISOCode": "ARS",
              "externalReference": "op-200",
              "source": "megatlon"
            }
            """;

            var r1 = await _client.PostAsync("/pagos", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            var d1 = await r1.Content.ReadFromJsonAsync<ApiResponse<PagoIdDto>>(Json.Options);

            var r2 = await _client.PostAsync("/pagos", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
            var d2 = await r2.Content.ReadFromJsonAsync<ApiResponse<PagoIdDto>>(Json.Options);

            r1.StatusCode.Should().Be(HttpStatusCode.OK);
            r2.StatusCode.Should().Be(HttpStatusCode.OK);
            d1!.Data!.PagoId.Should().Be(d2!.Data!.PagoId);
            d2!.Message.Should().ContainEquivalentOf("ya registrado");
        }
    }
}
