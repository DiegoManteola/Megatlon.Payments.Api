using Megatlon.Payments.Application.Contracts.Requests;
using Megatlon.Payments.Application.Contracts.Responses;
using Megatlon.Payments.Domain.Entities;
using Megatlon.Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Megatlon.Payments.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly PaymentsDbContext _db;
        public PagosController(PaymentsDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegistrarPagoRequest request, CancellationToken ct)
        {
            // Idempotencia: si ya existe un pago con Source + ExternalReference, no crear otro
            var existing = await _db.Pagos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Source == request.Source && p.ExternalReference == request.ExternalReference, ct);

            if (existing is not null)
                return Ok(new ApiResponse(true, "Pago ya registrado", new { pagoId = existing.Id }));

            var moneda = await _db.Monedas.FirstOrDefaultAsync(m => m.ISOCode == request.MonedaISOCode, ct);
            var medio = await _db.MediosPago.FirstOrDefaultAsync(m => m.Code == request.MedioPagoCode, ct);

            if (moneda is null || medio is null)
                return BadRequest(new ApiResponse(false, "Moneda o medio inválido", null,
                    new List<string> { "Verifique MonedaISOCode y MedioPagoCode" }));

            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email == request.Cliente.Email, ct);
            if (cliente is null)
            {
                cliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nombre = request.Cliente.Nombre,
                    Email = request.Cliente.Email,
                    FechaAlta = DateTime.UtcNow
                };
                _db.Clientes.Add(cliente);
            }

            var pago = new Pago
            {
                Id = Guid.NewGuid(),
                Cliente = cliente,
                Monto = request.Monto,
                FechaPago = request.FechaPago,
                MedioPagoId = medio.Id,
                MedioPago = medio,
                MonedaId = moneda.Id,
                Moneda = moneda,
                Source = request.Source,
                ExternalReference = request.ExternalReference
            };

            _db.Pagos.Add(pago);
            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                var again = await _db.Pagos
                    .AsNoTracking()
                    .FirstAsync(p => p.Source == request.Source && p.ExternalReference == request.ExternalReference, ct);
                return Ok(new ApiResponse(true, "Pago ya registrado", new { pagoId = again.Id }));
            }

            return Ok(new ApiResponse(true, "Pago registrado correctamente", new { pagoId = pago.Id }));
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string source, [FromQuery] string externalReference, CancellationToken ct)
        {
            var pago = await _db.Pagos
                .Include(p => p.Cliente)
                .Include(p => p.Moneda)
                .Include(p => p.MedioPago)
                .FirstOrDefaultAsync(p => p.Source == source && p.ExternalReference == externalReference, ct);

            if (pago is null)
                return NotFound(new ApiResponse(false, "Pago no encontrado"));

            var data = new
            {
                clienteNombre = pago.Cliente.Nombre,
                clienteEmail = pago.Cliente.Email,
                monto = pago.Monto,
                medioPagoNombre = pago.MedioPago.Nombre,
                monedaNombre = pago.Moneda.Nombre
            };
            return Ok(new ApiResponse(true, "OK", data));
        }
    }
}
