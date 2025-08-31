namespace Megatlon.Payments.Api.Middleware
{
    public sealed class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private static readonly string LogDir = Path.Combine(AppContext.BaseDirectory, "logs");
        private static readonly string LogFile = Path.Combine(LogDir, "pagos.txt");

        public RequestResponseLoggingMiddleware(RequestDelegate next, 
                                                ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            Directory.CreateDirectory(LogDir);

            // Leer request body
            context.Request.EnableBuffering();
            string bodyText = "";
            if (context.Request.ContentLength > 0 && context.Request.Body.CanSeek)
            {
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                bodyText = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            // Capturar response
            var originalBody = context.Response.Body;
            await using var mem = new MemoryStream();
            context.Response.Body = mem;

            await _next(context);

            mem.Position = 0;
            var responseText = await new StreamReader(mem).ReadToEndAsync();
            mem.Position = 0;
            await mem.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            var line = $@"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}Z] {context.Request.Method} {context.Request.Path}{context.Request.QueryString}
                            REQ: {bodyText}
                            RESP({context.Response.StatusCode}): {responseText}
                            ----";
            await File.AppendAllTextAsync(LogFile, line + Environment.NewLine);
        }
    }
}
