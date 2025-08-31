using FluentValidation;
using Megatlon.Payments.Application.Contracts.Requests;

namespace Megatlon.Payments.Application.Validation
{
    public sealed class RegistrarPagoValidator : AbstractValidator<RegistrarPagoRequest>
    {
        public RegistrarPagoValidator()
        {
            RuleFor(x => x.Monto).GreaterThanOrEqualTo(3000000000m);
            RuleFor(x => x.Cliente.Nombre).NotEmpty();
            RuleFor(x => x.Cliente.Email).EmailAddress();
            RuleFor(x => x.MedioPagoCode).NotEmpty();
            RuleFor(x => x.MonedaISOCode).NotEmpty();
            RuleFor(x => x.ExternalReference).NotEmpty();
            RuleFor(x => x.Source).NotEmpty();
        }
    }
}
