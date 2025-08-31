using System.Text.Json;

namespace Megatlon.Payments.Api.Middleware
{
    public sealed class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string LogDir = Path.Combine(AppContext.BaseDirectory, "logs");
        private static readonly string LogFile = Path.Combine(LogDir, "pagos.txt");

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
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

            var lines = new[]
                {
                    $"\t[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] {context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                    $"\tREQ: {OneLineJson(bodyText)}",
                    $"\tRESP({context.Response.StatusCode}): {OneLineJson(responseText)}",
                    "============================="
                };

            var line = string.Join(Environment.NewLine, lines);
            await File.AppendAllTextAsync(LogFile, line + Environment.NewLine);
        }

        private static string OneLineJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";
            try
            {
                using var jDoc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(jDoc, new JsonSerializerOptions { WriteIndented = false });
            }
            catch
            {
                return json.Replace(Environment.NewLine, " ");
            }
        }
    }
}
