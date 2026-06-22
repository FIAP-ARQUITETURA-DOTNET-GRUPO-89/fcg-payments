using FluentValidation;
using FcgPayments.Application.Commands.Orders;

namespace FcgPayments.Application.Validators.Orders;

public class UpdateDeliveryAddressValidator : AbstractValidator<UpdateDeliveryAddressCommand>
{
    public UpdateDeliveryAddressValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.City)
            .NotEmpty();

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2);

        RuleFor(x => x.Cep)
            .NotEmpty()
            .Matches(@"^\d{5}-\d{3}$|\d{8}$")
            .WithMessage("O CEP deve estar no formato 00000-000 ou apenas números.");
    }
}
