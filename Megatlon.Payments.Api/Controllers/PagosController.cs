using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Megatlon.Payments.Application.Contracts.Requests;
using Megatlon.Payments.Application.Contracts.Responses;
using Megatlon.Payments.Application.Rules;
using Megatlon.Payments.Application.Rules.Interfaces;
using Megatlon.Payments.Domain.Entities;
using Megatlon.Payments.Infrastructure.Persistence;

namespace Megatlon.Payments.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public sealed class PagosController : ControllerBase
    {
        private readonly PaymentsDbContext _db;
        private readonly IValidator<RegistrarPagoRequest> _validator;
        private readonly IPaymentRuleEngine _ruleEngine;

        public PagosController(
            PaymentsDbContext db,
            IValidator<RegistrarPagoRequest> validator,
            IPaymentRuleEngine ruleEngine)
        {
            _db = db;
            _validator = validator;
            _ruleEngine = ruleEngine;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegistrarPagoRequest req, CancellationToken ct)
        {
            // Validación (FluentValidation)
            var vr = await _validator.ValidateAsync(req, ct);
            if (!vr.IsValid)
                return BadRequest(new ApiResponse(false, "Datos inválidos", null, vr.Errors.Select(e => e.ErrorMessage).ToList()));

            // Idempotencia
            var existing = await _db.Pagos.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Source == req.Source && p.ExternalReference == req.ExternalReference, ct);

            if (existing is not null)
                return Ok(new ApiResponse(true, "Pago ya registrado", new { pagoId = existing.Id }));

            // Referencias (Moneda / Medio)
            var moneda = await _db.Monedas.FirstOrDefaultAsync(m => m.ISOCode == req.MonedaISOCode, ct);
            var medio = await _db.MediosPago.FirstOrDefaultAsync(m => m.Code == req.MedioPagoCode, ct);
            if (moneda is null || medio is null)
                return BadRequest(new ApiResponse(false, "Moneda o medio inválido", null,
                    new() { "Verifique MonedaISOCode y MedioPagoCode" }));

            // Reglas de negocio configurables
            var ctxRules = new PaymentContext
            {
                Request = req,
                MedioPagoId = medio.Id,
                MedioPagoCode = medio.Code,
                MonedaId = moneda.Id,
                MonedaISO = moneda.ISOCode,
                CardBin = req.CardBin
            };

            var ruleErrors = await _ruleEngine.ValidateAsync(ctxRules, ct);
            if (ruleErrors.Count > 0)
                return BadRequest(new ApiResponse(false, "Reglas de negocio no satisfechas", null, ruleErrors.ToList()));

            // Cliente
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.Email == req.Cliente.Email, ct);
            if (cliente is null)
            {
                cliente = new Cliente
                {
                    Id = Guid.NewGuid(),
                    Nombre = req.Cliente.Nombre,
                    Email = req.Cliente.Email,
                    FechaAlta = DateTime.UtcNow
                };
                _db.Clientes.Add(cliente);
            }

            // Alta del pago
            var pago = new Pago
            {
                Id = Guid.NewGuid(),
                Cliente = cliente,
                Monto = req.Monto,
                FechaPago = req.FechaPago,
                MedioPagoId = medio.Id,
                MonedaId = moneda.Id,
                MedioPago = medio,   // si tus navegaciones son 'required'
                Moneda = moneda,     // idem
                Source = req.Source,
                ExternalReference = req.ExternalReference
            };

            _db.Pagos.Add(pago);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                // Carreras por índice único (Source, ExternalReference)
                var again = await _db.Pagos.AsNoTracking()
                    .FirstAsync(p => p.Source == req.Source && p.ExternalReference == req.ExternalReference, ct);

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
