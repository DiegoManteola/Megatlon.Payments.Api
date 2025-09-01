using System;
using System.Text.Json;

namespace Megatlon.Payments.Api.Tests
{
    public static class Json
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public sealed record ApiResponse<T>(bool Success, string Message, T? Data, string[]? Errors);
    public sealed record PagoIdDto(Guid PagoId);
}
