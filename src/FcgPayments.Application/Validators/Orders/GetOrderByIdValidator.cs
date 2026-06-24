using FluentValidation;
using FcgPayments.Application.Queries.Orders;
using FcgPayments.SharedKernel.Validators;

namespace FcgPayments.Application.Validators.Orders;

public class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>, IValidatableRequest
{
    public GetOrderByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
