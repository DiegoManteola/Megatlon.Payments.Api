using Microsoft.EntityFrameworkCore;
using Megatlon.Payments.Infrastructure.Persistence;
using FluentValidation;

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
            builder.Services.AddDbContext<PaymentsDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

            // FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            app.Run();
        }
    }
}
