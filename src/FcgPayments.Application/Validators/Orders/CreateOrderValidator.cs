using FluentValidation;
using FcgPayments.Application.Commands.Orders;

namespace FcgPayments.Application.Validators.Orders;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Customer)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TotalAmount)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2);

        RuleFor(x => x.Cep)
            .NotEmpty()
            .Matches(@"^\d{8}$")
            .WithMessage("CEP inválido.");
    }
}
