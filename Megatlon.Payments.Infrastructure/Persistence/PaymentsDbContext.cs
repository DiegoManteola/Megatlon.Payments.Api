using Megatlon.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Megatlon.Payments.Infrastructure.Persistence
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
            : base(options) { }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<MedioPago> MediosPago => Set<MedioPago>();
        public DbSet<Moneda> Monedas => Set<Moneda>();
        public DbSet<Pago> Pagos => Set<Pago>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            // Idempotencia
            b.Entity<Pago>()
                .HasIndex(p => new { p.Source, p.ExternalReference })
                .IsUnique();

            // Opcional: longitudes mínimas
            b.Entity<MedioPago>().Property(x => x.Code).HasMaxLength(20);
            b.Entity<Moneda>().Property(x => x.ISOCode).HasMaxLength(10);

            // SEED (3 monedas + 3 medios de pago)
            b.Entity<Moneda>().HasData(
                new Moneda { Id = 1, Nombre = "Peso Argentino", Descripcion = "Pesos argentinos", ISOCode = "ARS" },
                new Moneda { Id = 2, Nombre = "Dólar", Descripcion = "Dólar estadounidense", ISOCode = "USD" },
                new Moneda { Id = 3, Nombre = "Peso Uruguayo", Descripcion = "Pesos uruguayos", ISOCode = "UYU" }
            );

            b.Entity<MedioPago>().HasData(
                new MedioPago { Id = 1, Nombre = "Efectivo", Descripcion = "Pago en efectivo", Code = "EFEC" },
                new MedioPago { Id = 2, Nombre = "Cheque", Descripcion = "Pago con Cheque", Code = "CHEQ" },
                new MedioPago { Id = 3, Nombre = "Tarjeta", Descripcion = "Pago con Tarjeta crédito/débito", Code = "TARJ" }
            );
        }
    }
}
