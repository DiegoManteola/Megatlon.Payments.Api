namespace Megatlon.Payments.Application.Contracts.Responses
{
    public sealed record ApiResponse(
        bool Success,
        string Message,
        object? Data = null,
        List<string>? Errors = null
    );
}
