using FluentValidation;
using FcgPayments.Application.Commands.Payments;

namespace FcgPayments.Application.Validators.Payments;

public class ProcessPaymentValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.GameId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
