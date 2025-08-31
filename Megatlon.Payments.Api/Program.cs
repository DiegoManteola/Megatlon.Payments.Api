using FluentValidation;
using Megatlon.Payments.Api.Middleware;
using Megatlon.Payments.Application.Validation;
using Megatlon.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Megatlon.Payments.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers + Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // EF Core SQLite
            builder.Services.AddDbContext<PaymentsDbContext>(opt => opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddValidatorsFromAssembly(typeof(RegistrarPagoValidator).Assembly);

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.MapControllers();

            app.Run();
        }
    }
}
