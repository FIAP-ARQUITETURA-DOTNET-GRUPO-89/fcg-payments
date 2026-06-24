using FluentValidation;
using FcgPayments.Application.Commands.Orders;
using FcgPayments.Domain.Enums;

namespace FcgPayments.Application.Validators.Orders;

public class ForceOrderStatusValidator : AbstractValidator<ForceOrderStatusCommand>
{
    public ForceOrderStatusValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(x => Enum.TryParse<OrderStatus>(x, true, out _))
            .WithMessage($"Status inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<OrderStatus>())}.");
    }
}
