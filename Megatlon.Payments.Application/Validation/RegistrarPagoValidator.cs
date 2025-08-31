using FluentValidation;
using Megatlon.Payments.Application.Contracts.Requests;

namespace Megatlon.Payments.Application.Validation
{
    public sealed class RegistrarPagoValidator : AbstractValidator<RegistrarPagoRequest>
    {
        public RegistrarPagoValidator()
        {
            RuleFor(x => x.Monto)
                .GreaterThanOrEqualTo(3000000000m)
                .WithMessage("El monto debe ser mayor o igual a 3.000.000.000");

            RuleFor(x => x.Cliente.Nombre)
                .NotEmpty()
                .WithMessage("El nombre del cliente es obligatorio");

            RuleFor(x => x.Cliente.Email)
                .NotEmpty().WithMessage("El email del cliente es obligatorio")
                .EmailAddress().WithMessage("El email del cliente no es válido");

            RuleFor(x => x.MedioPagoCode)
                .NotEmpty()
                .WithMessage("Debe especificar un medio de pago");

            RuleFor(x => x.MonedaISOCode)
                .NotEmpty()
                .WithMessage("Debe especificar una moneda");

            RuleFor(x => x.ExternalReference)
                .NotEmpty()
                .WithMessage("Debe especificar una referencia externa");

            RuleFor(x => x.Source)
                .NotEmpty()
                .WithMessage("Debe especificar el origen del pago");
        }
    }
}
