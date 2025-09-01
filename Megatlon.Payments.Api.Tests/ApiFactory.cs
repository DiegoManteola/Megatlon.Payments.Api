using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Megatlon.Payments.Application.Rules.Interfaces;
using Megatlon.Payments.Infrastructure.Persistence;

namespace Megatlon.Payments.Api.Tests
{
    public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private SqliteConnection _connection = default!;

        public async Task InitializeAsync()
        {
            // SQLite en memoria compartida para toda la vida del factory
            _connection = new SqliteConnection("DataSource=:memory:;Cache=Shared");
            await _connection.OpenAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Reemplazo el DbContext registrado por la API
                var old = services.FirstOrDefault(d => d.ServiceType == typeof(DbContextOptions<PaymentsDbContext>));
                if (old is not null) services.Remove(old);

                services.AddDbContext<PaymentsDbContext>(opt => opt.UseSqlite(_connection));

                // Reemplazo el provider de reglas por uno de pruebas (no dependemos de rulesconfiguration.json)
                var existingRulesProvider = services.FirstOrDefault(s => s.ServiceType == typeof(IPaymentRulesConfig));
                if (existingRulesProvider is not null) services.Remove(existingRulesProvider);
                services.AddSingleton<IPaymentRulesConfig, TestRulesConfig>();

                // Aplico migraciones sobre la DB en memoria
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                db.Database.Migrate();
            });
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}
