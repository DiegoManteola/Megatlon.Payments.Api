using FluentValidation;
using Megatlon.Payments.Application.Rules;
using Megatlon.Payments.Application.Rules.Interfaces;
using Megatlon.Payments.Application.Validation; 
using Megatlon.Payments.Api.Rules;
using Megatlon.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Megatlon.Payments.Api.Middleware;

namespace Megatlon.Payments.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Archivos de configuración
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("rulesconfiguration.json", optional: false, reloadOnChange: true);

            // Servicios core
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // EF Core (SQLite)
            builder.Services.AddDbContext<PaymentsDbContext>(opt =>
                opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

            // FluentValidation
            builder.Services.AddValidatorsFromAssembly(typeof(RegistrarPagoValidator).Assembly);

            // Reglas de negocio configurables
            //    - Provider de configuración
            builder.Services.AddSingleton<IPaymentRulesConfig, PaymentRulesConfig>();
            //    - Reglas de validacion
            builder.Services.AddScoped<IPaymentRule, MinMaxAmountRule>();
            builder.Services.AddScoped<IPaymentRule, AllowedCurrenciesRule>();
            // builder.Services.AddScoped<IPaymentRule, AllowedBinsRule>(); // Agregable cuando se adicione bin

            //    - Engine
            builder.Services.AddScoped<IPaymentRuleEngine, PaymentRuleEngine>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Logging a archivo
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.MapControllers();

            // Aplicar migraciones al arrancar
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
